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
    using System.Text;
    using System.Threading.Tasks;
    using HtmlToPdf.Extensions;
    using PuppeteerSharp;
    using PuppeteerSharp.Media;

    /// <summary>
    /// The HTML to PDF processor
    /// </summary>
    public class HtmlToPdfProcessor
    {
        private static bool coverAdded = false;

        /// <summary>
        /// Converts the HTML files to a PDF.
        /// </summary>
        /// <param name="options">The options.</param>
        /// <param name="logger">The logger.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public static async Task ProcessAsync(Options options, ILogger logger)
        {
            DateTime dtNow = DateTime.Now; // local time

            Encoding encoding = Encoding.Default;
            if (!string.IsNullOrEmpty(options.Encoding))
            {
                encoding = Encoding.GetEncoding(options.Encoding);
            }

            if (options.Inputs.Count() < 1)
            {
                throw new ApplicationException($"At least one input must be specified.");
            }

            if (string.IsNullOrEmpty(options.OutputFilePath))
            {
                throw new ApplicationException($"An output must be specified.");
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

            using (Browser browser = await Puppeteer.LaunchAsync(new LaunchOptions
            {
                SlowMo = 0,
                Headless = true,
                Timeout = 0,
                LogProcess = false,
                EnqueueTransportMessages = true,
                Devtools = false
            }))
            {
                MarginOptions marginOptions = new MarginOptions
                {
                    Bottom = options.BottomMargin,
                    Left = options.LeftMargin,
                    Right = options.RightMargin,
                    Top = options.TopMargin
                };

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
                        PrintBackground = options.PrintBackground
                    };

                    if (!string.IsNullOrEmpty(options.Cover) && (!coverAdded))
                    {
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
                            NumberOfPages = numberOfPages
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
                        { "doctitle", title }
                    };

                    // count the number of PDF pages each HTML file will be printed as
                    var tasks = options.Inputs.Where(x => !htmlToPdfFiles.Any(y => y.Input == x)).Select(async input =>
                    {
                        // print as pdf
                        string pdfFile = await pdfPrinter.PrintAsPdfAsync(
                            input,
                            htmlToPdfOptions,
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
                            NumberOfPages = numberOfPages
                        };

                        htmlToPdfFiles.Add(htmlToPdfFile);
                    });

                    await Task.WhenAll(tasks);

                    variables.Add("topage", htmlToPdfFiles.Sum(x => x.NumberOfPages).ToString());

                    // update models and optionally re-print HTML files to include footers with page numbers
                    tasks = htmlToPdfFiles.Select(async htmlToPdfFile =>
                    {
                        if (string.IsNullOrEmpty(title)
                            && (htmlToPdfFile.Index == 0))
                        {
                            HtmlFileParser htmlFileParser = new HtmlFileParser(htmlToPdfFile.Input);
                            title = htmlFileParser.GetTitle();
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
                        htmlToPdfOptions.PageOffset = currentPageNumber - 1;
                        htmlToPdfOptions.PageNumberOffset = options.PageOffset;
                        htmlToPdfOptions.NumberOfPages = htmlToPdfFile.NumberOfPages;

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
                    });

                    await Task.WhenAll(tasks);
                }
                finally
                {
                    await browser.CloseAsync();
                }
            }

            // merge pdf files
            byte[] mergedBytes = PdfMerger.Merge(htmlToPdfFiles
                .Where(x => !x.Skip)
                .OrderBy(x => x.OutputPdfFilePageNumber)
                .Select(x => x.PdfFilePath));

            File.WriteAllBytes(outputFilePath, mergedBytes);

            // update external file links to internal document links
            PdfDocument.UpdateLinks(outputFilePath, htmlToPdfFiles, logger);

            PdfDocument.SetTitle(outputFilePath, title);

            // delete temporary PDF files
            var deleteTempFileTasks = htmlToPdfFiles.Where(x => !string.IsNullOrEmpty(x.PdfFilePath)).Select(async input =>
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
        }
    }
}
