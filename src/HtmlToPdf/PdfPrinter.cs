// <copyright file="PdfPrinter.cs" company="HtmlToPdf">
// Copyright (c) HtmlToPdf. All rights reserved.
// </copyright>

namespace HtmlToPdf
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Threading;
    using System.Threading.Tasks;
    using HtmlAgilityPack;
    using HtmlToPdf.Exceptions;
    using Polly;
    using PuppeteerSharp;
    using PuppeteerSharp.Media;

    /// <summary>
    /// PDF Printer.
    /// </summary>
    internal class PdfPrinter
    {
        private readonly Browser browser;
        private readonly ILogger logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="PdfPrinter" /> class.
        /// </summary>
        /// <param name="browser">The browser.</param>
        /// <param name="logger">The logger.</param>
        public PdfPrinter(
            Browser browser,
            ILogger logger)
        {
            this.browser = browser;
            this.logger = logger;
        }

        /// <summary>
        /// Prints as PDF asynchronously.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <param name="options">The options.</param>
        /// <param name="variables">The variables.</param>
        /// <param name="createLinksForHeadings">if set to <c>true</c> creates links for headings.</param>
        /// <returns>
        /// A task with the PDF file path as a result.
        /// </returns>
        /// <exception cref="System.IO.FileNotFoundException">File not found: {fullPath}.</exception>
        internal async Task<string> PrintAsPdfAsync(
            string input,
            HtmlToPdfOptions options,
            Dictionary<string, string> variables,
            bool createLinksForHeadings = true)
        {
            string fullPath = Path.GetFullPath(input);
            if (!File.Exists(fullPath))
            {
                throw new FileNotFoundException($"File not found: {fullPath}", fullPath);
            }

            string tempPdfFilePath = TempPdfFile.Create();

            NavigationOptions navigationOptions = new NavigationOptions
            {
                WaitUntil = new[] { WaitUntilNavigation.Networkidle0 },
                Timeout = 0,
            };

            AddTagOptions addTagOptions = null;
            if (!string.IsNullOrEmpty(options.StyleSheet))
            {
                if (File.Exists(options.StyleSheet))
                {
                    addTagOptions = new AddTagOptions
                    {
                        Type = "text/css",
                        Path = options.StyleSheet,
                    };
                }
                else
                {
                    this.logger.LogWarning($"File not found: {options.StyleSheet}");
                }
            }

            // the paper format takes priority over width and height
            // if a page width or height is set, unset the paper format
            PaperFormat paperFormat = options.PaperFormat;
            string width = options.Width;
            string height = options.Height;
            if ((!string.IsNullOrEmpty(width))
                || (!string.IsNullOrEmpty(height)))
            {
                paperFormat = null;
            }

            using (TempCopyHtmlFile tempHtmlFile = new TempCopyHtmlFile(fullPath))
            {
                this.PrependEmptyPages(tempHtmlFile.FilePath, options.PageOffset + options.PageNumberOffset);

                // TODO: do not create links for headings if not generating an outline
                if (createLinksForHeadings)
                {
                    this.CreateLinksForHeadings(tempHtmlFile.FilePath);
                }

                string pageRanges = string.Empty;
                if ((options.PageOffset + options.PageNumberOffset) > 0)
                {
                    int fromPage = options.PageOffset + options.PageNumberOffset + 1;
                    int toPage = options.PageOffset + options.PageNumberOffset + options.NumberOfPages;

                    pageRanges = $"{fromPage}-";
                    if (options.NumberOfPages != 0)
                    {
                        pageRanges += $"{toPage}";
                    }

                    this.logger.LogDebug($"Printing pages {pageRanges} of page '{fullPath}'.");
                }

                // page variables
                Dictionary<string, string> pageVariables = new Dictionary<string, string>();
                if (variables != null)
                {
                    foreach (KeyValuePair<string, string> keyValuePair in variables)
                    {
                        pageVariables.Add(keyValuePair.Key, keyValuePair.Value);
                    }
                }

                pageVariables.Add("webpage", fullPath);

                string footerTemplate = string.Empty;
                bool displayFooter = options.FooterTemplateBuilder.DisplayTemplate;
                if (displayFooter)
                {
                    footerTemplate = options.FooterTemplateBuilder.Build(pageVariables);
                }

                string headerTemplate = string.Empty;
                bool displayHeader = options.HeaderTemplateBuilder.DisplayTemplate;
                if (displayHeader)
                {
                    headerTemplate = options.HeaderTemplateBuilder.Build(pageVariables);
                }

                PdfOptions pdfOptions = new PdfOptions
                {
                    DisplayHeaderFooter = displayHeader || displayFooter,
                    FooterTemplate = footerTemplate,
                    Format = paperFormat,
                    HeaderTemplate = headerTemplate,
                    Height = height,
                    Landscape = options.Landscape,
                    MarginOptions = options.MarginOptions,
                    PreferCSSPageSize = false,
                    PageRanges = pageRanges,
                    PrintBackground = options.PrintBackground,
                    Scale = 1,
                    Width = width,
                };

                PolicyResult policyResult = await Policy
                    .Handle<TargetClosedException>()
                    .Or<Exception>()
                    .RetryAsync(2, onRetry: (ex, retryCount, context) =>
                    {
                        // executed before each retry
                        // ex. PuppeteerSharp.TargetClosedException: Protocol error(IO.read): Target closed. (Page failed to process Inspector.targetCrashed. Exception of type 'PuppeteerSharp.TargetCrashedException' was thrown..    at PuppeteerSharp.Page.OnTargetCrashed()
                        //     at PuppeteerSharp.Page.<Client_MessageReceived>d__230.MoveNext())
                        //     at System.Runtime.ExceptionServices.ExceptionDispatchInfo.Throw()
                        //     at System.Runtime.CompilerServices.TaskAwaiter.HandleNonSuccessAndDebuggerNotification(Task task)
                        //     at PuppeteerSharp.CDPSession.<SendAsync>d__30.MoveNext()
                        //     --- End of stack trace from previous location where exception was thrown ---
                        //     at System.Runtime.ExceptionServices.ExceptionDispatchInfo.Throw()
                        //     at System.Runtime.CompilerServices.TaskAwaiter.HandleNonSuccessAndDebuggerNotification(Task task)
                        //     at PuppeteerSharp.CDPSession.<SendAsync>d__29`1.MoveNext()
                        //     --- End of stack trace from previous location where exception was thrown ---
                        //     at System.Runtime.ExceptionServices.ExceptionDispatchInfo.Throw()
                        //     at PuppeteerSharp.Helpers.ProtocolStreamReader.<ReadProtocolStreamByteAsync>d__1.MoveNext()
                        //     --- End of stack trace from previous location where exception was thrown ---
                        //     at System.Runtime.ExceptionServices.ExceptionDispatchInfo.Throw()
                        //     at System.Runtime.CompilerServices.TaskAwaiter.HandleNonSuccessAndDebuggerNotification(Task task)
                        //     at PuppeteerSharp.Page.<PdfInternalAsync>d__171.MoveNext()
                        //     --- End of stack trace from previous location where exception was thrown ---
                        //     at System.Runtime.ExceptionServices.ExceptionDispatchInfo.Throw()
                        //     at System.Runtime.CompilerServices.TaskAwaiter.HandleNonSuccessAndDebuggerNotification(Task task)
                        //     at PuppeteerSharp.Page.<PdfAsync>d__166.MoveNext()
                        //      --- End of stack trace from previous location where exception was thrown ---
                        //     at System.Runtime.ExceptionServices.ExceptionDispatchInfo.Throw()
                        //     at System.Runtime.CompilerServices.TaskAwaiter.HandleNonSuccessAndDebuggerNotification(Task task)
                        //     at HtmlToPdf.PdfPrinter.<PrintAsPdfAsync>d__3.MoveNext() in D:\a\HtmlToPdf\HtmlToPdf\src\HtmlToPdf\PdfPrinter.cs:line 167
                        //     --- End of inner exception stack trace ---
                        //     at HtmlToPdf.PdfPrinter.<PrintAsPdfAsync>d__3.MoveNext() in D:\a\HtmlToPdf\HtmlToPdf\src\HtmlToPdf\PdfPrinter.cs:line 171
                        //     --- End of stack trace from previous location where exception was thrown ---
                        //     at System.Runtime.ExceptionServices.ExceptionDispatchInfo.Throw()
                        //     at System.Runtime.CompilerServices.TaskAwaiter.HandleNonSuccessAndDebuggerNotification(Task task)
                        //     at HtmlToPdf.HtmlToPdfProcessor.<>c__DisplayClass0_1.<<ProcessAsync>b__6>d.MoveNext() in D:\a\HtmlToPdf\HtmlToPdf\src\HtmlToPdf\HtmlToPdfProcessor.cs:line 183
                        //     --- End of stack trace from previous location where exception was thrown ---
                        //     at System.Runtime.ExceptionServices.ExceptionDispatchInfo.Throw()
                        //     at HtmlToPdf.HtmlToPdfProcessor.<ProcessAsync>d__0.MoveNext() in D:\a\HtmlToPdf\HtmlToPdf\src\HtmlToPdf\HtmlToPdfProcessor.cs:line 206
                        //     --- End of stack trace from previous location where exception was thrown ---
                        //     Verbose:[PdfCommand.PDF.wkhtmltopdf]got ... HtmlToPdf.Console.exe output 0Bytes
                        //     at System.Runtime.ExceptionServices.ExceptionDispatchInfo.Throw()
                        //     at HtmlToPdf.HtmlToPdfProcessor.<ProcessAsync>d__0.MoveNext() in D:\a\HtmlToPdf\HtmlToPdf\src\HtmlToPdf\HtmlToPdfProcessor.cs:line 294
                        //     --- End of stack trace from previous location where exception was thrown ---
                        //     at System.Runtime.ExceptionServices.ExceptionDispatchInfo.Throw()
                        //     at System.Runtime.CompilerServices.TaskAwaiter.HandleNonSuccessAndDebuggerNotification(Task task)
                        //     at HtmlToPdf.Console.Program.<RunAsync>d__7.MoveNext() in D:\a\HtmlToPdf\HtmlToPdf\src\HtmlToPdf.Console\Program.cs:line 216
                        //     Error:[PdfCommand.PDF]Error happen when converting articles/toc.json to Pdf. Details: iTextSharp.text.exceptions.InvalidPdfException: PDF header signature not found.
                        //     at iTextSharp.text.pdf.PdfReader..ctor(ReaderProperties properties, IRandomAccessSource byteSource)
                        //     at Microsoft.DocAsCode.HtmlToPdf.HtmlToPdfConverter.SaveCore(Stream stream)
                        //     at Microsoft.DocAsCode.HtmlToPdf.HtmlToPdfConverter.Save(String outputFileName)
                        //     at Microsoft.DocAsCode.HtmlToPdf.ConvertWrapper.<>c__DisplayClass7_0.<ConvertCore>b__1(ManifestItem tocFile)
                        this.logger.LogWarning(ex.ToString());
                        Thread.Sleep(1000);
                    })
                    .ExecuteAndCaptureAsync(async () =>
                    {
                        using (Page page = await this.browser.NewPageAsync())
                        {
                            // disable navigation timeout
                            // otherwise, the following exception occurs:
                            // PuppeteerSharp.NavigationException: Timeout of 30000 ms exceeded ---> System.TimeoutException: Timeout of 30000 ms exceeded
                            page.DefaultNavigationTimeout = 0;
                            page.DefaultTimeout = 0;

                            await page.GoToAsync(tempHtmlFile.FilePath, navigationOptions);

                            if (addTagOptions != null)
                            {
                                await page.AddStyleTagAsync(addTagOptions);
                            }

                            await page.WaitForTimeoutAsync(options.JavascriptDelayInMilliseconds);

                            try
                            {
                                await page.PdfAsync(tempPdfFilePath, pdfOptions);
                            }
                            finally
                            {
                                await page.CloseAsync();
                            }
                        }
                    });

                if (policyResult.Outcome == OutcomeType.Failure)
                {
                    if (policyResult.FinalException is TargetClosedException)
                    {
                        throw policyResult.FinalException;
                    }
                    else
                    {
                        throw new PrintToPdfException(fullPath, policyResult.FinalException);
                    }
                }
            }

            return tempPdfFilePath;
        }

        private void CreateLinksForHeadings(string filePath)
        {
            HtmlDocument doc = new HtmlDocument();
            doc.Load(filePath);

            // get headings
            string xpathQuery = "//*[starts-with(name(),'h') and string-length(name()) = 2 and number(substring(name(), 2)) <= 6]";
            var nodes = doc.DocumentNode.SelectNodes(xpathQuery);

            if (nodes == null)
            {
                return;
            }

            // create hyperlinks
            foreach (var node in nodes)
            {
                int headingLevel = int.Parse(node.Name.Substring(1));

                // create anchor
                var hiddenAnchor = doc.CreateElement("a");

                // trying to use an 'href' with fragments (ex. '#fragment-name') generates the PDF without a hyperlink
                hiddenAnchor.Attributes.Add("href", $"{filePath}?headingLevel={headingLevel}&headingText={node.InnerText}");

                // trying to use the style 'visibility:hidden;' generates the PDF without a hyperlink
                // trying to use the style 'font-size:0;' generates the PDF without a hyperlink
                // trying to use the style 'line-height:0;' doesn't seem to do anything
                hiddenAnchor.Attributes.Add("style", "font-size:0;color:transparent;");
                hiddenAnchor.InnerHtml = node.InnerHtml;
                node.AppendChild(hiddenAnchor);
            }

            doc.Save(filePath);
        }

        private void PrependEmptyPages(string htmlFilePath, int pages)
        {
            if (pages <= 0)
            {
                return;
            }

            this.logger.LogDebug($"Inserting {pages} empty page(s) in {htmlFilePath}.");

            // insert empty pages
            HtmlDocument html = new HtmlDocument();
            html.Load(htmlFilePath);

            string style = @"<style type=""text/css"" media=""print"">
                .page-break
                {
                    page-break-after: always !important;
                    font-size: 0 !important;
                    visibility: hidden !important;
                }
            </style>";
            string emptyPage = "<div class=\"page-break\">Hidden Page {0}<br></div>";

            // Get head node
            HtmlNode head = html.DocumentNode.SelectSingleNode("//head");

            // Create new node
            HtmlNode newHeadNode = HtmlNode.CreateNode(style);

            // Add new node as last child of head
            head.AppendChild(newHeadNode);

            // Get body node
            HtmlNode body = html.DocumentNode.SelectSingleNode("//body");

            // create container
            HtmlNode containerNode = HtmlNode.CreateNode("<div style=\"position: relative !important;\"></div>");

            for (int i = 1; i <= pages; i++)
            {
                // Create new node
                HtmlNode newNode = HtmlNode.CreateNode(string.Format(emptyPage, i));

                // Add new node as child of container
                containerNode.ChildNodes.Add(newNode);
            }

            // create an empty node
            containerNode.ChildNodes.Add(HtmlNode.CreateNode("<div></div>"));

            // Add container as first child of body
            body.PrependChild(containerNode);

            html.Save(htmlFilePath);
        }
    }
}
