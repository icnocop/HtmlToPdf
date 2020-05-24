// <copyright file="PageOffsetTests.cs" company="HtmlToPdf">
// Copyright (c) HtmlToPdf. All rights reserved.
// </copyright>

namespace HtmlToPdfTests
{
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using UglyToad.PdfPig.Content;

    /// <summary>
    /// Page Offset Tests
    /// </summary>
    [TestClass]
    public class PageOffsetTests
    {
        /// <summary>
        /// Gets or sets the test context.
        /// </summary>
        public TestContext TestContext { get; set; }

        /// <summary>
        /// Asserts that passing a page offset value doesn't print the previous page(s).
        /// </summary>
        [TestMethod]
        public void PageOffsetTest()
        {
            HtmlToPdfRunner runner = new HtmlToPdfRunner();

            string html1 = @"
<html>
  <head>
  </head>
  <body>
   Page 1
  </body>
</html>";

            string html2 = @"
<html>
  <head>
  </head>
  <body>
   Page 2
  </body>
</html>";

            using (TempHtmlFile htmlFile1 = new TempHtmlFile(html1))
            {
                using (TempHtmlFile htmlFile2 = new TempHtmlFile(html2))
                {
                    using (TempPdfFile pdfFile = new TempPdfFile(this.TestContext))
                    {
                        string commandLine = $"--page-offset 1 \"{htmlFile1.FilePath}\" --footer-right [page] \"{htmlFile2.FilePath}\" \"{pdfFile.FilePath}\"";
                        HtmlToPdfRunResult result = runner.Run(commandLine);
                        Assert.AreEqual(0, result.ExitCode, result.Output);

                        using (var pdfDocument = UglyToad.PdfPig.PdfDocument.Open(pdfFile.FilePath))
                        {
                            Assert.AreEqual(1, pdfDocument.NumberOfPages);
                            Page page = pdfDocument.GetPage(1);
                            IEnumerable<Word> words = page.GetWords();
                            Assert.AreEqual(3, words.Count());
                            Assert.AreEqual("Page 2", $"{words.ElementAt(0)} {words.ElementAt(1)}");
                            Assert.AreEqual("1", words.Last().Text); // the page number
                        }
                    }
                }
            }
        }
    }
}
