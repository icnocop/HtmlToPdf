// <copyright file="PdfPrinter.cs" company="HtmlToPdf">
// Copyright (c) HtmlToPdf. All rights reserved.
// </copyright>

namespace HtmlToPdf
{
    using System.IO;
    using System.Threading.Tasks;
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

            string tempPdfFilePath = input + ".pdf";

            using (Page page = await this.browser.NewPageAsync())
            {
                NavigationOptions navigationOptions = new NavigationOptions
                {
                    WaitUntil = new WaitUntilNavigation[] { WaitUntilNavigation.Networkidle0 }
                };

                string fullPath = Path.GetFullPath(input);

                await page.GoToAsync(fullPath, navigationOptions);

                if (!string.IsNullOrEmpty(options.StyleSheet))
                {
                    if (!File.Exists(options.StyleSheet))
                    {
                        throw new FileNotFoundException($"File not found: {options.StyleSheet}");
                    }

                    await page.AddStyleTagAsync(new AddTagOptions
                    {
                        Type = "text/css",
                        Path = options.StyleSheet
                    });
                }

                await page.WaitForTimeoutAsync(options.JavascriptDelayInMilliseconds);

                // the paper format takes priority over width and height
                // if a page width or height is set, unset the paper format
                PaperFormat paperFormat = options.PaperFormat;
                if ((!string.IsNullOrEmpty(options.Width))
                    || (!string.IsNullOrEmpty(options.Height)))
                {
                    paperFormat = null;
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
                    PageRanges = string.Empty,
                    PrintBackground = options.PrintBackground,
                    Scale = 1,
                    Width = options.Width
                };

                await page.PdfAsync(tempPdfFilePath, pdfOptions);

                await page.CloseAsync();
            }

            return tempPdfFilePath;
        }
    }
}
