// <copyright file="FooterFontNameTests.cs" company="HtmlToPdf">
// Copyright (c) HtmlToPdf. All rights reserved.
// </copyright>

namespace HtmlToPdfTests
{
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using UglyToad.PdfPig.Content;

    /// <summary>
    /// Footer Font Name Tests
    /// </summary>
    [TestClass]
    public class FooterFontNameTests
    {
        /// <summary>
        /// Gets or sets the test context.
        /// </summary>
        public TestContext TestContext { get; set; }

        /// <summary>
        /// Asserts that the default footer-font-name is Arial MT.
        /// </summary>
        [TestMethod]
        public void FooterFontName_DefaultValue_IsArialMT()
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

                        Assert.AreEqual("ArialMT", $"{words.ElementAt(2).FontName}");
                    }
                }
            }
        }

        /// <summary>
        /// Asserts that passing footer-font-name "Times New Roman" sets the footer font family to "Times New Roman".
        /// </summary>
        [TestMethod]
        public void FooterFontName_WithTimesNewRoman_SetsTheFooterFontFamilyToTimesNewRoman()
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
                    string commandLine = $"--footer-font-name \"Times New Roman\" --footer-right [page] \"{htmlFile.FilePath}\" \"{pdfFile.FilePath}\"";
                    HtmlToPdfRunResult result = runner.Run(commandLine);
                    Assert.AreEqual(0, result.ExitCode, result.Output);

                    using (var pdfDocument = UglyToad.PdfPig.PdfDocument.Open(pdfFile.FilePath))
                    {
                        Assert.AreEqual(1, pdfDocument.NumberOfPages);
                        Page page1 = pdfDocument.GetPage(1);
                        IEnumerable<Word> words = page1.GetWords();
                        Assert.AreEqual(3, words.Count());
                        Assert.AreEqual("Page 1", $"{words.ElementAt(0)} {words.ElementAt(1)}");

                        Assert.AreEqual("TimesNewRomanPSMT", $"{words.ElementAt(2).FontName}");
                    }
                }
            }
        }
    }
}
