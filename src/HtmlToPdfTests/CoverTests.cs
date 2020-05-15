// <copyright file="CoverTests.cs" company="HtmlToPdf">
// Copyright (c) HtmlToPdf. All rights reserved.
// </copyright>

namespace HtmlToPdfTests
{
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using UglyToad.PdfPig.Content;

    /// <summary>
    /// Cover Tests
    /// </summary>
    [TestClass]
    public class CoverTests
    {
        /// <summary>
        /// Gets or sets the test context.
        /// </summary>
        public TestContext TestContext { get; set; }

        /// <summary>
        /// Asserts that passing a cover with an HTML file inserts a cover as the first page.
        /// </summary>
        [TestMethod]
        public void Cover_WithHtmlFile_InsertsCoverAsFirstPage()
        {
            HtmlToPdfRunner runner = new HtmlToPdfRunner();

            string cover = @"
<html>
  <head>
  </head>
  <body>
   Cover Page
  </body>
</html>";

            string html = @"
<html>
  <head>
  </head>
  <body>
   Page 2
  </body>
</html>";

            using (TempHtmlFile coverFile = new TempHtmlFile(cover, this.TestContext))
            {
                using (TempHtmlFile htmlFile = new TempHtmlFile(html, this.TestContext))
                {
                    using (TempPdfFile pdfFile = new TempPdfFile(this.TestContext))
                    {
                        string commandLine = $"cover \"{coverFile.FilePath}\" \"{htmlFile.FilePath}\" \"{pdfFile.FilePath}\"";
                        HtmlToPdfRunResult result = runner.Run(commandLine);
                        Assert.AreEqual(0, result.ExitCode, result.Output);

                        using (var pdfDocument = UglyToad.PdfPig.PdfDocument.Open(pdfFile.FilePath))
                        {
                            Assert.AreEqual(2, pdfDocument.NumberOfPages);
                            Page coverPage = pdfDocument.GetPage(1);
                            Assert.AreEqual("Cover Page", coverPage.Text);
                            Page page2 = pdfDocument.GetPage(2);
                            Assert.AreEqual("Page 2", page2.Text);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Asserts that passing a footer doesn't apply to the cover page.
        /// </summary>
        [TestMethod]
        public void Cover_WithFooter_DoesNotApplyToCoverPage()
        {
            HtmlToPdfRunner runner = new HtmlToPdfRunner();

            string cover = @"
<html>
  <head>
  </head>
  <body>
   Cover Page
  </body>
</html>";

            string html = @"
<html>
  <head>
  </head>
  <body>
   Page 2
  </body>
</html>";

            using (TempHtmlFile coverFile = new TempHtmlFile(cover))
            {
                using (TempHtmlFile htmlFile = new TempHtmlFile(html))
                {
                    using (TempPdfFile pdfFile = new TempPdfFile(this.TestContext))
                    {
                        string commandLine = $"--footer-right [page] cover \"{coverFile.FilePath}\" \"{htmlFile.FilePath}\" \"{pdfFile.FilePath}\"";
                        HtmlToPdfRunResult result = runner.Run(commandLine);
                        Assert.AreEqual(0, result.ExitCode, result.Output);

                        using (var pdfDocument = UglyToad.PdfPig.PdfDocument.Open(pdfFile.FilePath))
                        {
                            Assert.AreEqual(2, pdfDocument.NumberOfPages);
                            Page coverPage = pdfDocument.GetPage(1);
                            Assert.AreEqual("Cover Page", coverPage.Text);
                            Page page2 = pdfDocument.GetPage(2);
                            IEnumerable<Word> words = page2.GetWords();
                            Assert.AreEqual(3, words.Count());
                            Assert.AreEqual("Page 2", $"{words.ElementAt(0)} {words.ElementAt(1)}");
                            Assert.AreEqual("2", words.Last().Text); // the page number
                        }
                    }
                }
            }
        }
    }
}
