// <copyright file="FooterFontSizeTests.cs" company="HtmlToPdf">
// Copyright (c) HtmlToPdf. All rights reserved.
// </copyright>

namespace HtmlToPdfTests
{
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using UglyToad.PdfPig.Content;

    /// <summary>
    /// Footer Font Size Tests.
    /// </summary>
    [TestClass]
    public class FooterFontSizeTests
    {
        /// <summary>
        /// Gets or sets the test context.
        /// </summary>
        public TestContext TestContext { get; set; }

        /// <summary>
        /// Asserts the default footer-font-size.
        /// </summary>
        /// <param name="exeFileName">Name of the executable file.</param>
        /// <param name="expectedFontSize">Expected size of the font.</param>
        [TestMethod]
        [DataRow(HtmlToPdfRunner.HtmlToPdfExe, "12", DisplayName = "HtmlToPdf.exe")]
        [DataRow(HtmlToPdfRunner.WkhtmltopdfExe, "18", DisplayName = "wkhtmltopdf.exe")]
        public void FooterFontSize_DefaultValue(string exeFileName, string expectedFontSize)
        {
            HtmlToPdfRunner runner = new HtmlToPdfRunner(exeFileName);

            string html = @"<!DOCTYPE html>
<html>
  <head>
  </head>
  <body>
   Page 1
  </body>
</html>";

            using (TempHtmlFile htmlFile = new TempHtmlFile(html))
            {
                using (TempPdfFile pdfFile = new TempPdfFile(this.TestContext))
                {
                    string commandLine = $"--footer-right [page] \"{htmlFile.FilePath}\" \"{pdfFile.FilePath}\"";
                    HtmlToPdfRunResult result = runner.Run(commandLine);
                    Assert.AreEqual(0, result.ExitCode, result.Output);

                    using (var pdfDocument = UglyToad.PdfPig.PdfDocument.Open(pdfFile.FilePath))
                    {
                        Assert.AreEqual(1, pdfDocument.NumberOfPages);
                        Page page1 = pdfDocument.GetPage(1);
                        IEnumerable<Word> words = page1.GetWords();
                        Assert.AreEqual(3, words.Count());
                        Assert.AreEqual("Page 1", $"{words.ElementAt(0)} {words.ElementAt(1)}");

                        Assert.AreEqual(expectedFontSize, $"{words.ElementAt(2).Letters[0].FontSize}");
                    }
                }
            }
        }

        /// <summary>
        /// Asserts that passing footer-font-size sets the footer font size.
        /// </summary>
        /// <param name="exeFileName">Name of the executable file.</param>
        /// <param name="fontSize">Size of the font.</param>
        /// <param name="expectedFontSize">Expected size of the font.</param>
        [TestMethod]
        [DataRow(HtmlToPdfRunner.HtmlToPdfExe, "10", "10", DisplayName = "HtmlToPdf.exe")]
        [DataRow(HtmlToPdfRunner.WkhtmltopdfExe, "10", "16", DisplayName = "wkhtmltopdf.exe")]
        public void FooterFontSize_SetsTheFooterFontSize(string exeFileName, string fontSize, string expectedFontSize)
        {
            HtmlToPdfRunner runner = new HtmlToPdfRunner(exeFileName);

            string html = @"<!DOCTYPE html>
<html>
  <head>
  </head>
  <body>
   Page 1
  </body>
</html>";

            using (TempHtmlFile htmlFile = new TempHtmlFile(html))
            {
                using (TempPdfFile pdfFile = new TempPdfFile(this.TestContext))
                {
                    string commandLine = $"--footer-font-size {fontSize} --footer-right [page] \"{htmlFile.FilePath}\" \"{pdfFile.FilePath}\"";
                    HtmlToPdfRunResult result = runner.Run(commandLine);
                    Assert.AreEqual(0, result.ExitCode, result.Output);

                    using (var pdfDocument = UglyToad.PdfPig.PdfDocument.Open(pdfFile.FilePath))
                    {
                        Assert.AreEqual(1, pdfDocument.NumberOfPages);
                        Page page1 = pdfDocument.GetPage(1);
                        IEnumerable<Word> words = page1.GetWords();
                        Assert.AreEqual(3, words.Count());
                        Assert.AreEqual("Page 1", $"{words.ElementAt(0)} {words.ElementAt(1)}");

                        Assert.AreEqual(expectedFontSize, $"{words.ElementAt(2).Letters[0].FontSize}");
                    }
                }
            }
        }
    }
}
