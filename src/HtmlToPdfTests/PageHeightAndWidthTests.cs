// <copyright file="PageHeightAndWidthTests.cs" company="HtmlToPdf">
// Copyright (c) HtmlToPdf. All rights reserved.
// </copyright>

namespace HtmlToPdfTests
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using UglyToad.PdfPig.Content;

    /// <summary>
    /// Page Height and Width Tests.
    /// </summary>
    [TestClass]
    public class PageHeightAndWidthTests
    {
        /// <summary>
        /// Gets or sets the test context.
        /// </summary>
        public TestContext TestContext { get; set; }

        /// <summary>
        /// Asserts that passing page height and width values sets the page height and width.
        /// </summary>
        /// <param name="exeFileName">Name of the executable file.</param>
        /// <param name="height">The height.</param>
        /// <param name="width">The width.</param>
        /// <param name="expectedHeight">The expected height.</param>
        /// <param name="expectedWidth">The expected width.</param>
        [TestMethod]
        [DataRow(HtmlToPdfRunner.HtmlToPdfExe, null, null, 841, 594, DisplayName = "HtmlToPdf.exe Default")]
        [DataRow(HtmlToPdfRunner.WkhtmltopdfExe, null, null, 842, 595, DisplayName = "wkhtmltopdf.exe Default")]
        [DataRow(HtmlToPdfRunner.HtmlToPdfExe, "12in", "12in", 864, 864, DisplayName = "HtmlToPdf.exe 12in")]
        [DataRow(HtmlToPdfRunner.WkhtmltopdfExe, "12in", "12in", 864, 864, DisplayName = "wkhtmltopdf.exe 12in")]
        public void PageHeightAndWidthTest(
            string exeFileName,
            string height,
            string width,
            int expectedHeight,
            int expectedWidth)
        {
            HtmlToPdfRunner runner = new HtmlToPdfRunner(exeFileName);

            string html = @"<!DOCTYPE html>
<html>
  <head>
  </head>
  <body>
   Test Page
  </body>
</html>";

            using (TempHtmlFile htmlFile = new TempHtmlFile(html))
            {
                using (TempPdfFile pdfFile = new TempPdfFile(this.TestContext))
                {
                    string commandLine = string.Empty;

                    if (!string.IsNullOrEmpty(height))
                    {
                        commandLine += $"--page-height {height} ";
                    }

                    if (!string.IsNullOrEmpty(width))
                    {
                        commandLine += $"--page-width {width} ";
                    }

                    commandLine += $"\"{htmlFile.FilePath}\" \"{pdfFile.FilePath}\"";
                    HtmlToPdfRunResult result = runner.Run(commandLine);
                    Assert.AreEqual(0, result.ExitCode, result.Output);

                    using (var pdfDocument = UglyToad.PdfPig.PdfDocument.Open(pdfFile.FilePath))
                    {
                        Assert.AreEqual(1, pdfDocument.NumberOfPages);
                        Page page = pdfDocument.GetPage(1);

                        string message = $"Size: {page.Size} Height: {page.Height} Width: {page.Width}";
                        Assert.AreEqual(expectedWidth, page.Width, message);
                        Assert.AreEqual(expectedHeight, page.Height, message);
                    }
                }
            }
        }
    }
}
