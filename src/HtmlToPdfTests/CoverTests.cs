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
    /// Cover Tests.
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
        /// <param name="exeFileName">Name of the executable file.</param>
        [TestMethod]
        [DataRow(HtmlToPdfRunner.HtmlToPdfExe, DisplayName = "HtmlToPdf.exe")]
        [DataRow(HtmlToPdfRunner.WkhtmltopdfExe, DisplayName = "wkhtmltopdf.exe")]
        public void Cover_WithHtmlFile_InsertsCoverAsFirstPage(string exeFileName)
        {
            HtmlToPdfRunner runner = new HtmlToPdfRunner(exeFileName);

            string cover = @"<!DOCTYPE html>
<html>
  <head>
  </head>
  <body>
   Cover Page
  </body>
</html>";

            string html = @"<!DOCTYPE html>
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
        /// Asserts that passing a footer or header doesn't apply to the cover page.
        /// </summary>
        /// <param name="exeFileName">Name of the executable file.</param>
        /// <param name="headerOrFooterCommandLineArgument">The header or footer command line argument.</param>
        /// <param name="expectedText">The expected text.</param>
        [TestMethod]
        [DataRow(HtmlToPdfRunner.HtmlToPdfExe, "--margin-top 5mm --header-left [page]", "2 Page 2", DisplayName = "HtmlToPdf.exe Header")]
        [DataRow(HtmlToPdfRunner.WkhtmltopdfExe, "--margin-top 5mm --header-left [page]", "2 Page 2", DisplayName = "wkhtmltopdf.exe Header")]
        [DataRow(HtmlToPdfRunner.HtmlToPdfExe, "--footer-right [page]", "Page 2 2", DisplayName = "HtmlToPdf.exe Footer")]
        [DataRow(HtmlToPdfRunner.WkhtmltopdfExe, "--footer-right [page]", "Page 2 2", DisplayName = "wkhtmltopdf.exe Footer")]
        public void Cover_WithHeaderOrFooter_DoesNotApplyToCoverPage(string exeFileName, string headerOrFooterCommandLineArgument, string expectedText)
        {
            HtmlToPdfRunner runner = new HtmlToPdfRunner(exeFileName);

            string cover = @"<!DOCTYPE html>
<html>
  <head>
  </head>
  <body>
   Cover Page
  </body>
</html>";

            string html = @"<!DOCTYPE html>
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
                        string commandLine = $"{headerOrFooterCommandLineArgument} cover \"{coverFile.FilePath}\" \"{htmlFile.FilePath}\" \"{pdfFile.FilePath}\"";
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
                            Assert.AreEqual(expectedText, string.Join(" ", words));
                        }
                    }
                }
            }
        }
    }
}
