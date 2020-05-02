// <copyright file="PdfPrinter.cs" company="HtmlToPdf">
// Copyright (c) HtmlToPdf. All rights reserved.
// </copyright>

namespace HtmlToPdf
{
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

        /// <summary>
        /// Initializes a new instance of the <see cref="PdfPrinter"/> class.
        /// </summary>
        /// <param name="browser">The browser.</param>
        public PdfPrinter(Browser browser)
        {
            this.browser = browser;
        }

        /// <summary>
        /// Prints as PDF asynchronously.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <param name="options">The options.</param>
        /// <returns>A task with the PDF file path as a result.</returns>
        internal async Task<string> PrintAsPdfAsync(
            string input,
            HtmlToPdfOptions options)
        {
            if (!File.Exists(input))
            {
                throw new FileNotFoundException($"File not found: {input}", input);
            }

            if (options.PageOffset > 1)
            {
                // insert empty pages
                HtmlDocument html = new HtmlDocument();
                html.Load(input);

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

                for (int i = 1; i < options.PageOffset; i++)
                {
                    // Create new node
                    HtmlNode newNode = HtmlNode.CreateNode(string.Format(emptyPage, i));

                    // Add new node as first child of body
                    body.PrependChild(newNode);
                }

                html.Save(input);
            }

            string tempPdfFilePath = input + ".pdf";

            NavigationOptions navigationOptions = new NavigationOptions
            {
                WaitUntil = new[] { WaitUntilNavigation.Networkidle0 }
            };

            string fullPath = Path.GetFullPath(input);

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
                    Logger.LogWarning($"File not found: {options.StyleSheet}");
                }
            }

            // the paper format takes priority over width and height
            // if a page width or height is set, unset the paper format
            PaperFormat paperFormat = options.PaperFormat;
            if ((!string.IsNullOrEmpty(options.Width))
                || (!string.IsNullOrEmpty(options.Height)))
            {
                paperFormat = null;
            }

            string pageRanges = string.Empty;
            if (options.PageOffset > 1)
            {
                if (options.NumberOfPages == 1)
                {
                    pageRanges = $"{options.PageOffset}";
                }
                else
                {
                    pageRanges = $"{options.PageOffset} - {options.PageOffset + options.NumberOfPages - 1}";
                }

                // Logger.LogError($"Printing pages {pageRanges} of page '{fullPath}'.");
            }

            PdfOptions pdfOptions = new PdfOptions
            {
                DisplayHeaderFooter = options.DisplayHeaderFooter,
                FooterTemplate = options.FooterTemplate,
                Format = paperFormat,
                HeaderTemplate = string.Empty,
                Height = options.Height,
                Landscape = options.Landscape,
                MarginOptions = options.MarginOptions,
                PreferCSSPageSize = false,
                PageRanges = pageRanges,
                PrintBackground = options.PrintBackground,
                Scale = 1,
                Width = options.Width
            };

            using (Page page = await this.browser.NewPageAsync())
            {
                await page.GoToAsync(fullPath, navigationOptions);

                if (addTagOptions != null)
                {
                    await page.AddStyleTagAsync(addTagOptions);
                }

                await page.WaitForTimeoutAsync(options.JavascriptDelayInMilliseconds);

                await page.PdfAsync(tempPdfFilePath, pdfOptions);

                await page.CloseAsync();
            }

            return tempPdfFilePath;
        }
    }
}
