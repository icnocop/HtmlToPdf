// <copyright file="PageSizeTests.cs" company="HtmlToPdf">
// Copyright (c) HtmlToPdf. All rights reserved.
// </copyright>

namespace HtmlToPdfTests
{
    using System;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using UglyToad.PdfPig.Content;

    /// <summary>
    /// Page Size Tests
    /// </summary>
    [TestClass]
    public class PageSizeTests
    {
        /// <summary>
        /// Gets or sets the test context.
        /// </summary>
        public TestContext TestContext { get; set; }

        /// <summary>
        /// Asserts that passing an invalid page size returns an error.
        /// </summary>
        [TestMethod]
        public void InvalidPageSize_ReturnsError()
        {
            HtmlToPdfRunner runner = new HtmlToPdfRunner();

            string html = @"
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
                    string[] commandLine = new[]
                    {
                        $"--page-size invalid",
                        $"\"{htmlFile.FilePath}\"",
                        $"\"{pdfFile.FilePath}\""
                    };

                    HtmlToPdfRunResult result = runner.Run(string.Join(" ", commandLine));
                    Assert.AreEqual(1, result.ExitCode);

                    string[] expectedErrorMessage = new[]
                    {
                        "System.ArgumentOutOfRangeException: Specified argument was out of the range of valid values.",
                        "Parameter name: invalid"
                    };

                    string expectedOutput = HelpTextGenerator.Generate(string.Join(Environment.NewLine, expectedErrorMessage));
                    Assert.IsTrue(string.IsNullOrEmpty(result.StandardOutput), result.StandardOutput);
                    Assert.IsTrue(result.StandardError.Trim().StartsWith(expectedOutput), result.StandardError);
                }
            }
        }

        /// <summary>
        /// Asserts that passing a page size value sets the page size.
        /// </summary>
        /// <param name="pageSizeCommandLineParameter">The page size command line parameter.</param>
        /// <param name="pageSize">Size of the page.</param>
        /// <param name="width">The width.</param>
        /// <param name="height">The height.</param>
        [TestMethod]
        [DataRow(null, PageSize.Custom, 594, 841, DisplayName = "Default")]
        [DataRow("A4", PageSize.Custom, 594, 841, DisplayName = "A4")]
        [DataRow("A3", PageSize.Custom, 841, 1189, DisplayName = "A3")]
        [DataRow("A2", PageSize.Custom, 1189, 1684, DisplayName = "A2")]
        [DataRow("A1", PageSize.Custom, 1684, 2382, DisplayName = "A1")]
        [DataRow("A0", PageSize.Custom, 2382, 3368, DisplayName = "A0")]
        [DataRow("Ledger", PageSize.Ledger, 1224, 792, DisplayName = "Ledger")]
        [DataRow("Tabloid", PageSize.Tabloid, 792, 1224, DisplayName = "Tabloid")]
        [DataRow("A5", PageSize.Custom, 419, 594, DisplayName = "A5")]
        [DataRow("Legal", PageSize.Legal, 612, 1008, DisplayName = "Legal")]
        [DataRow("Letter", PageSize.Letter, 612, 792, DisplayName = "Letter")]
        [DataRow("A6", PageSize.Custom, 297, 419, DisplayName = "A6")]
        public void PageSizeTest(string pageSizeCommandLineParameter, PageSize pageSize, int width, int height)
        {
            HtmlToPdfRunner runner = new HtmlToPdfRunner();

            string html = @"
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

                    if (!string.IsNullOrEmpty(pageSizeCommandLineParameter))
                    {
                        commandLine += $"--page-size {pageSizeCommandLineParameter} ";
                    }

                    commandLine += $"\"{htmlFile.FilePath}\" \"{pdfFile.FilePath}\"";
                    HtmlToPdfRunResult result = runner.Run(commandLine);
                    Assert.AreEqual(0, result.ExitCode, result.Output);

                    using (var pdfDocument = UglyToad.PdfPig.PdfDocument.Open(pdfFile.FilePath))
                    {
                        Assert.AreEqual(1, pdfDocument.NumberOfPages);
                        Page page = pdfDocument.GetPage(1);
                        Assert.AreEqual(pageSize, page.Size);
                        Assert.AreEqual(width, page.Width);
                        Assert.AreEqual(height, page.Height);
                    }
                }
            }
        }
    }
}
