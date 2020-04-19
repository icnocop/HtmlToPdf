﻿// <copyright file="PageHeightTests.cs" company="HtmlToPdf">
// Copyright (c) HtmlToPdf. All rights reserved.
// </copyright>

namespace HtmlToPdfTests
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using UglyToad.PdfPig.Content;

    /// <summary>
    /// Page Height Tests
    /// </summary>
    [TestClass]
    public class PageHeightTests
    {
        /// <summary>
        /// Gets or sets the test context.
        /// </summary>
        public TestContext TestContext { get; set; }

        /// <summary>
        /// Asserts that passing a page height value sets the page height.
        /// </summary>
        /// <param name="height">The height.</param>
        /// <param name="expectedWidth">The expected width.</param>
        /// <param name="expectedHeight">The expected height.</param>
        [TestMethod]
        [DataRow(null, 594, 841, DisplayName = "Default")]
        [DataRow("12in", 612, 864, DisplayName = "12in")]
        public void PageHeightTest(string height, int expectedWidth, int expectedHeight)
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

                    if (!string.IsNullOrEmpty(height))
                    {
                        commandLine += $"--page-height {height} ";
                    }

                    commandLine += $"\"{htmlFile.FilePath}\" \"{pdfFile.FilePath}\"";
                    HtmlToPdfRunResult result = runner.Run(commandLine);
                    Assert.AreEqual(0, result.ExitCode, result.Output);

                    using (var pdfDocument = UglyToad.PdfPig.PdfDocument.Open(pdfFile.FilePath))
                    {
                        Assert.AreEqual(1, pdfDocument.NumberOfPages);
                        Page page = pdfDocument.GetPage(1);
                        Assert.AreEqual(expectedWidth, page.Width);
                        Assert.AreEqual(expectedHeight, page.Height);
                    }
                }
            }
        }
    }
}
