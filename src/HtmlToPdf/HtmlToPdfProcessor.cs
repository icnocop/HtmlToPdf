// <copyright file="HtmlToPdfProcessor.cs" company="HtmlToPdf">
// Copyright (c) HtmlToPdf. All rights reserved.
// </copyright>

namespace HtmlToPdf
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Net.WebSockets;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using HtmlToPdf.Exceptions;
    using HtmlToPdf.Extensions;
    using Polly;
    using PuppeteerSharp;
    using PuppeteerSharp.Media;

    /// <summary>
    /// The HTML to PDF processor.
    /// </summary>
    public class HtmlToPdfProcessor
    {
        /// <summary>
        /// Converts the HTML files to a PDF.
        /// </summary>
        /// <param name="options">The options.</param>
        /// <param name="logger">The logger.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public static async Task<ConcurrentBag<HtmlToPdfFile>> ProcessAsync(Options options, ILogger logger)
        {
            DateTime dtNow = DateTime.Now; // local time

            Encoding encoding = Encoding.Default;
            if (!string.IsNullOrEmpty(options.Encoding))
            {
                encoding = Encoding.GetEncoding(options.Encoding);
            }

            if (!options.Inputs.Any()
                && !options.DumpDefaultTocXsl)
            {
                throw new ApplicationException($"At least one input must be specified.");
            }

            if (string.IsNullOrEmpty(options.OutputFilePath)
                && !options.DumpDefaultTocXsl)
            {
                throw new ApplicationException($"An output must be specified.");
            }

            if (options.DumpDefaultTocXsl)
            {
                string defaultTocXsl = options.DefaultTableOfContentsStyleSheetBuilder(options.OutputDottedLinesInTableOfContents);
                logger.LogOutput(defaultTocXsl);
            }

            ConcurrentBag<HtmlToPdfFile> htmlToPdfFiles = new ConcurrentBag<HtmlToPdfFile>();

            foreach (string input in options.Inputs)
            {
                logger.LogDebug(input);
            }

            string outputFilePath = options.OutputFilePath;

            options.UserStyleSheet = options.UserStyleSheet?.Trim('"');

            string title = options.Title;

            BrowserDownloader.DownloadBrowser(logger);

            using (TempDirectory tempDirectory = new TempDirectory())
            {
                var launchOptions = new LaunchOptions
                {
                    SlowMo = 0,
                    Headless = true,
                    Timeout = 0,
                    LogProcess = false,
                    EnqueueTransportMessages = true,
                    Devtools = false,
                    WebSocketFactory = WebSocketFactory,
                    UserDataDir = tempDirectory.DirectoryPath,
                };

                MarginOptions marginOptions = new MarginOptions
                {
                    Bottom = options.BottomMargin,
                    Left = options.LeftMargin,
                    Right = options.RightMargin,
                    Top = options.TopMargin,
                };

                await Policy
                    .Handle<ChromiumProcessException>()
                    .Retry(2, onRetry: (ex, retryCount) =>
                    {
                        // executed before each retry
                        // https://github.com/hardkoded/puppeteer-sharp/issues/1509
                        // ex. PuppeteerSharp.ChromiumProcessException: Failed to launch Chromium! [0909/142354.872:FATAL:feature_list.cc(282)] Check failed: !g_initialized_from_accessor.
                        // Error: Backtrace:
                        // Error:   ovly_debug_event [0x00007FFE262A1252+16183762]
                        // Error:   ovly_debug_event [0x00007FFE262A0832+16181170]
                        // Error:   ovly_debug_event [0x00007FFE262B3383+16257795]
                        // Error:   ovly_debug_event [0x00007FFE262A3386+16192262]
                        // Error:   ovly_debug_event [0x00007FFE25DF4B2E+11283118]
                        // Error:   ovly_debug_event [0x00007FFE2621DB58+15645400]
                        // Error:   ovly_debug_event [0x00007FFE2621DACD+15645261]
                        // Error:   ovly_debug_event [0x00007FFE26248F28+15822504]
                        // Error:   ovly_debug_event [0x00007FFE2621D35E+15643358]
                        // Error:   ovly_debug_event [0x00007FFE262483E3+15819619]
                        // Error:   ovly_debug_event [0x00007FFE262482BB+15819323]
                        // Error:   ovly_debug_event [0x00007FFE262480F2+15818866]
                        // Error:   ChromeMain [0x00007FFE253311B6+286]
                        // Error:   Ordinal0 [0x00007FF65A33275F+10079]
                        // Error:   Ordinal0 [0x00007FF65A33182D+6189]
                        // Error:   GetHandleVerifier [0x00007FF65A43B7C2+697538]
                        // Error:   BaseThreadInitThunk [0x00007FFE5B2D84D4+20]
                        // Error:   RtlUserThreadStart [0x00007FFE5B95E871+33]
                        logger.LogWarning(ex.ToString());
                        Thread.Sleep(1000);
                    })
                    .Execute(async () =>
                    {
                        bool coverAdded = false;

                        using (Browser browser = await Puppeteer.LaunchAsync(launchOptions))
                        {
                            try
                            {
                                PdfPrinter pdfPrinter = new PdfPrinter(browser, logger);

                                // cover options
                                HtmlToPdfOptions htmlToPdfOptions = new HtmlToPdfOptions
                                {
                                    StyleSheet = options.UserStyleSheet,
                                    JavascriptDelayInMilliseconds = options.JavascriptDelayInMilliseconds,
                                    Landscape = options.Landscape,
                                    PaperFormat = options.PaperFormat,
                                    Height = options.PageHeight,
                                    Width = options.PageWidth,
                                    PrintBackground = options.PrintBackground,
                                };

                                if (!string.IsNullOrEmpty(options.Cover) && (!coverAdded))
                                {
                                    // print cover
                                    string pdfFile = await pdfPrinter.PrintAsPdfAsync(
                                        options.Cover,
                                        htmlToPdfOptions,
                                        null);

                                    int numberOfPages = PdfDocument.CountNumberOfPages(pdfFile);

                                    logger.LogDebug($"Cover file \"{options.Cover}\" contains number of PDF pages: {numberOfPages}.");

                                    HtmlToPdfFile htmlToPdfFile = new HtmlToPdfFile
                                    {
                                        Input = options.Cover,
                                        Index = 0,
                                        PdfFilePath = pdfFile,
                                        PrintFooter = false,
                                        NumberOfPages = numberOfPages,
                                    };

                                    htmlToPdfFiles.Add(htmlToPdfFile);

                                    coverAdded = true;
                                }

                                // page options
                                htmlToPdfOptions.MarginOptions = marginOptions;
                                htmlToPdfOptions.FooterTemplateBuilder.FooterLeft = options.FooterLeft;
                                htmlToPdfOptions.FooterTemplateBuilder.FooterCenter = options.FooterCenter;
                                htmlToPdfOptions.FooterTemplateBuilder.FooterRight = options.FooterRight;

                                string footerFontSize = options.FooterFontSize.AppendUnits("px");

                                htmlToPdfOptions.FooterTemplateBuilder.FooterFontSize = footerFontSize;
                                htmlToPdfOptions.FooterTemplateBuilder.FooterFontName = options.FooterFontName;
                                htmlToPdfOptions.FooterTemplateBuilder.FooterHtml = options.FooterHtml;

                                // global header/footer variables
                                // https://chromedevtools.github.io/devtools-protocol/tot/Page/#method-printToPDF
                                Dictionary<string, string> variables = new Dictionary<string, string>
                                {
                                    { "page", "<span class=\"pageNumber\"></span>" },
                                    { "date", "<span class=\"date\"></span>" }, // M/dd/yyyy
                                    { "title", "<span class=\"title\"></span>" },
                                    { "frompage",  (options.PageOffset + 1).ToString() },
                                    { "isodate", dtNow.ToString("yyyy-MM-dd") }, // ISO 8601 extended format
                                    { "time", dtNow.ToString("h:mm:ss tt") }, // ex. 3:58:45 PM
                                    { "doctitle", title },
                                };

                                // count the number of PDF pages each HTML file will be printed as
                                var tasks = options.Inputs
                                    .Where(x => htmlToPdfFiles.All(y => y.Input != x))
                                    .Select(async input =>
                                    {
                                        // print as pdf
                                        // insert an empty page to avoid unexpected margins on the first page, which would affect the page count
                                        // https://stackoverflow.com/a/55480268/90287
                                        // https://github.com/puppeteer/puppeteer/issues/2592
                                        HtmlToPdfOptions tempHtmlToPdfOptions = htmlToPdfOptions.Copy();
                                        tempHtmlToPdfOptions.PageOffset = 1;

                                        string pdfFile = await pdfPrinter.PrintAsPdfAsync(
                                            input,
                                            tempHtmlToPdfOptions,
                                            variables);

                                        // count the number of pages
                                        int numberOfPages = PdfDocument.CountNumberOfPages(pdfFile);
                                        logger.LogDebug($"\"{input}\" contains number of PDF pages: {numberOfPages}.");

                                        HtmlToPdfFile htmlToPdfFile = new HtmlToPdfFile
                                        {
                                            Input = input,
                                            Index = options.Inputs.IndexOf(input),
                                            PdfFilePath = pdfFile,
                                            PrintFooter = true,
                                            NumberOfPages = numberOfPages,
                                        };

                                        htmlToPdfFiles.Add(htmlToPdfFile);
                                    });

                                await Task.WhenAll(tasks);

                                variables.Add("topage", htmlToPdfFiles.Sum(x => x.NumberOfPages).ToString());

                                // update models with title and headings
                                List<Task> updateTitleAndHeadingsTasks = new List<Task>();

                                foreach (HtmlToPdfFile htmlToPdfFile in htmlToPdfFiles)
                                {
                                    updateTitleAndHeadingsTasks.Add(Task.Run(() =>
                                    {
                                        // set the title and headings
                                        HtmlFileParser htmlFileParser = new HtmlFileParser(htmlToPdfFile.Input);
                                        htmlToPdfFile.TitleAndHeadings = htmlFileParser.GetTitleAndHeadings(options.AddTableOfContents);
                                        htmlToPdfFile.Title = htmlToPdfFile.TitleAndHeadings.First().Text;
                                    }));
                                }

                                await Task.WhenAll(updateTitleAndHeadingsTasks);

                                // create table of contents
                                if (options.AddTableOfContents)
                                {
                                    await PdfOutlineBuilder.BuildOutlineAsync(
                                        coverAdded,
                                        options.AddTableOfContents,
                                        options.OutputDottedLinesInTableOfContents,
                                        htmlToPdfFiles,
                                        options.OutlineBuilder,
                                        options.DefaultTableOfContentsStyleSheetBuilder,
                                        pdfPrinter,
                                        htmlToPdfOptions,
                                        variables);
                                }

                                // update models and re-print HTML files to include footers with page numbers
                                tasks = htmlToPdfFiles.Select(async htmlToPdfFile =>
                                {
                                    if (string.IsNullOrEmpty(title)
                                        && (htmlToPdfFile.Index == 0))
                                    {
                                        // set the PDF title
                                        title = htmlToPdfFile.Title;
                                        variables["doctitle"] = title;
                                    }

                                    // sum the number of pages in previous documents to get the current page number offset
                                    int currentPageNumber = htmlToPdfFiles
                                        .Where(x => x.Index < htmlToPdfFile.Index)
                                        .Sum(x => x.NumberOfPages) + 1;

                                    if ((currentPageNumber + htmlToPdfFile.NumberOfPages) <= (options.PageOffset + 1))
                                    {
                                        logger.LogDebug($"Skipping printing {htmlToPdfFile.Input}");
                                        htmlToPdfFile.Skip = true;
                                        return;
                                    }

                                    // print as pdf with page number offset
                                    htmlToPdfFile.OutputPdfFilePageNumber = currentPageNumber;

                                    logger.LogDebug($"'{htmlToPdfFile.Input}' mapped to output PDF file page number {currentPageNumber}.");

                                    htmlToPdfOptions.PageOffset = currentPageNumber - 1;
                                    htmlToPdfOptions.PageNumberOffset = options.PageOffset;
                                    htmlToPdfOptions.NumberOfPages = htmlToPdfFile.NumberOfPages;

                                    // TODO: only print as PDF again if topage variable is actually used in the header/footer
                                    if (htmlToPdfFile.PrintFooter)
                                    {
                                        // delete previously created PDF file
                                        File.Delete(htmlToPdfFile.PdfFilePath);

                                        // print as pdf
                                        string pdfFile = await pdfPrinter.PrintAsPdfAsync(
                                            htmlToPdfFile.Input,
                                            htmlToPdfOptions,
                                            variables);

                                        htmlToPdfFile.PdfFilePath = pdfFile;
                                    }

                                    // parse PDF to get heading page numbers
                                    PdfDocument.SetHeadingPageNumbers(htmlToPdfFile);
                                });

                                await Task.WhenAll(tasks);
                            }
                            finally
                            {
                                await browser.CloseAsync();
                            }
                        }
                    });
            }

            // merge pdf files
            List<string> pdfFilesToMerge = htmlToPdfFiles
                .Where(x => !x.Skip)
                .OrderBy(x => x.OutputPdfFilePageNumber)
                .Select(x => x.PdfFilePath)
                .ToList();

            if (!string.IsNullOrEmpty(outputFilePath))
            {
                byte[] mergedBytes = PdfMerger.Merge(pdfFilesToMerge);

                File.WriteAllBytes(outputFilePath, mergedBytes);

                try
                {
                    // update external file links to internal document links
                    PdfDocument.UpdateLinks(outputFilePath, htmlToPdfFiles, logger);
                }
                catch (Exception ex)
                {
                    throw new UpdatePdfLinksException(outputFilePath, htmlToPdfFiles, ex);
                }

                PdfDocument.SetTitle(outputFilePath, title);
            }

            // delete temporary PDF files
            var deleteTempFileTasks = htmlToPdfFiles
                .Where(x => !string.IsNullOrEmpty(x.PdfFilePath))
                .Select(async input =>
                {
                    await Task.Factory.StartNew(() =>
                    {
                        if (File.Exists(input.PdfFilePath))
                        {
                            File.Delete(input.PdfFilePath);
                        }
                    });
                });

            await Task.WhenAll(deleteTempFileTasks);

            return htmlToPdfFiles;
        }

        /// <summary>
        /// WebSocket factory to support Windows 7 and Windows Server 2008.
        /// https://github.com/hardkoded/puppeteer-sharp/issues/1368#issuecomment-580946444
        /// The minimum Windows versions supporting the WebSocket library are Windows 8 and Windows Server 2012.
        /// <see href="https://github.com/hardkoded/puppeteer-sharp#prerequisites"/>.
        /// </summary>
        /// <param name="uri">The URI.</param>
        /// <param name="socketOptions">The socket options.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The WebSocket factory.</returns>
        private static async Task<WebSocket> WebSocketFactory(Uri uri, IConnectionOptions socketOptions, CancellationToken cancellationToken)
        {
            var client = SystemClientWebSocket.CreateClientWebSocket();
            if (client is System.Net.WebSockets.Managed.ClientWebSocket managed)
            {
                managed.Options.KeepAliveInterval = TimeSpan.FromSeconds(0);
                await managed.ConnectAsync(uri, cancellationToken);
            }
            else
            {
                var coreSocket = client as ClientWebSocket;
                coreSocket.Options.KeepAliveInterval = TimeSpan.FromSeconds(0);
                await coreSocket.ConnectAsync(uri, cancellationToken);
            }

            return client;
        }
    }
}
