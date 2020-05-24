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
    /// Footer Font Size Tests
    /// </summary>
    [TestClass]
    public class FooterFontSizeTests
    {
        /// <summary>
        /// Gets or sets the test context.
        /// </summary>
        public TestContext TestContext { get; set; }

        /// <summary>
        /// Asserts that the default footer-font-size is 12.
        /// </summary>
        [TestMethod]
        public void FooterFontSize_DefaultValue_Is12()
        {
            HtmlToPdfRunner runner = new HtmlToPdfRunner();

            string html = @"
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

                        Assert.AreEqual("12", $"{words.ElementAt(2).Letters[0].FontSize}");
                    }
                }
            }
        }

        /// <summary>
        /// Asserts that passing footer-font-size 10 sets the footer font size to 10px.
        /// </summary>
        [TestMethod]
        public void FooterFontSize_With10_SetsTheFooterFontSizeTo10Pixels()
        {
            HtmlToPdfRunner runner = new HtmlToPdfRunner();

            string html = @"
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
                    string commandLine = $"--footer-font-size 10 --footer-right [page] \"{htmlFile.FilePath}\" \"{pdfFile.FilePath}\"";
                    HtmlToPdfRunResult result = runner.Run(commandLine);
                    Assert.AreEqual(0, result.ExitCode, result.Output);

                    using (var pdfDocument = UglyToad.PdfPig.PdfDocument.Open(pdfFile.FilePath))
                    {
                        Assert.AreEqual(1, pdfDocument.NumberOfPages);
                        Page page1 = pdfDocument.GetPage(1);
                        IEnumerable<Word> words = page1.GetWords();
                        Assert.AreEqual(3, words.Count());
                        Assert.AreEqual("Page 1", $"{words.ElementAt(0)} {words.ElementAt(1)}");

                        Assert.AreEqual("10", $"{words.ElementAt(2).Letters[0].FontSize}");
                    }
                }
            }
        }
    }
}
