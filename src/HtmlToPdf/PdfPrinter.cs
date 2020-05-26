// <copyright file="PdfPrinter.cs" company="HtmlToPdf">
// Copyright (c) HtmlToPdf. All rights reserved.
// </copyright>

namespace HtmlToPdf
{
    using System.Collections.Generic;
    using System.IO;
    using System.Threading.Tasks;
    using HtmlAgilityPack;
    using PuppeteerSharp;
    using PuppeteerSharp.Media;

    /// <summary>
    /// PDF Printer
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
        /// <returns>
        /// A task with the PDF file path as a result.
        /// </returns>
        /// <exception cref="FileNotFoundException">File not found: {fullPath}</exception>
        internal async Task<string> PrintAsPdfAsync(
            string input,
            HtmlToPdfOptions options,
            Dictionary<string, string> variables)
        {
            string fullPath = Path.GetFullPath(input);
            if (!File.Exists(fullPath))
            {
                throw new FileNotFoundException($"File not found: {fullPath}", fullPath);
            }

            string tempPdfFilePath = TempPdfFile.Create();

            NavigationOptions navigationOptions = new NavigationOptions
            {
                WaitUntil = new[] { WaitUntilNavigation.Networkidle0 }
            };

            AddTagOptions addTagOptions = null;
            if (!string.IsNullOrEmpty(options.StyleSheet))
            {
                if (File.Exists(options.StyleSheet))
                {
                    addTagOptions = new AddTagOptions
                    {
                        Type = "text/css",
                        Path = options.StyleSheet
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

                string pageRanges = string.Empty;
                if ((options.PageOffset + options.PageNumberOffset) > 0)
                {
                    pageRanges = $"{options.PageOffset + options.PageNumberOffset + 1} - {options.PageOffset + options.PageNumberOffset + options.NumberOfPages}";

                    this.logger.LogDebug($"Printing pages {pageRanges} of page '{fullPath}'.");
                }

                string footerTemplate = string.Empty;
                bool displayHeaderFooter = options.FooterTemplateBuilder.DisplayHeaderFooter;
                if (displayHeaderFooter)
                {
                    // page variables
                    Dictionary<string, string> pageVariables = new Dictionary<string, string>(variables)
                    {
                        { "webpage", fullPath }
                    };

                    footerTemplate = options.FooterTemplateBuilder.Build(pageVariables);
                }

                PdfOptions pdfOptions = new PdfOptions
                {
                    DisplayHeaderFooter = displayHeaderFooter,
                    FooterTemplate = footerTemplate,
                    Format = paperFormat,
                    HeaderTemplate = string.Empty,
                    Height = height,
                    Landscape = options.Landscape,
                    MarginOptions = options.MarginOptions,
                    PreferCSSPageSize = false,
                    PageRanges = pageRanges,
                    PrintBackground = options.PrintBackground,
                    Scale = 1,
                    Width = width
                };

                using (Page page = await this.browser.NewPageAsync())
                {
                    await page.GoToAsync(tempHtmlFile.FilePath, navigationOptions);

                    if (addTagOptions != null)
                    {
                        await page.AddStyleTagAsync(addTagOptions);
                    }

                    await page.WaitForTimeoutAsync(options.JavascriptDelayInMilliseconds);

                    await page.PdfAsync(tempPdfFilePath, pdfOptions);

                    await page.CloseAsync();
                }
            }

            return tempPdfFilePath;
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
    page-break-after: always;
    visibility: hidden;
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

            for (int i = pages; i > 0; i--)
            {
                // Create new node
                HtmlNode newNode = HtmlNode.CreateNode(string.Format(emptyPage, i));

                // Add new node as first child of body
                body.PrependChild(newNode);
            }

            html.Save(htmlFilePath);
        }
    }
}
