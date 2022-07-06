// <copyright file="HeaderFontNameTests.cs" company="HtmlToPdf">
// Copyright (c) HtmlToPdf. All rights reserved.
// </copyright>

namespace HtmlToPdfTests
{
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using UglyToad.PdfPig.Content;

    /// <summary>
    /// Header Font Name Tests.
    /// </summary>
    [TestClass]
    public class HeaderFontNameTests
    {
        /// <summary>
        /// Gets or sets the test context.
        /// </summary>
        public TestContext TestContext { get; set; }

        /// <summary>
        /// Asserts that the default header-font-name is Arial.
        /// </summary>
        /// <param name="exeFileName">Name of the executable file.</param>
        /// <param name="expectedFontName">Expected name of the font.</param>
        [TestMethod]
        [DataRow(HtmlToPdfRunner.HtmlToPdfExe, "AAAAAA+ArialMT", DisplayName = "HtmlToPdf.exe")]
        [DataRow(HtmlToPdfRunner.WkhtmltopdfExe, "ArialRegular", DisplayName = "wkhtmltopdf.exe")]
        public void HeaderFontName_DefaultValue_IsArial(string exeFileName, string expectedFontName)
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
                    string commandLine = $"--header-right [page] \"{htmlFile.FilePath}\" \"{pdfFile.FilePath}\"";
                    HtmlToPdfRunResult result = runner.Run(commandLine);
                    Assert.AreEqual(0, result.ExitCode, result.Output);

                    using (var pdfDocument = UglyToad.PdfPig.PdfDocument.Open(pdfFile.FilePath))
                    {
                        Assert.AreEqual(1, pdfDocument.NumberOfPages);
                        Page page1 = pdfDocument.GetPage(1);
                        IEnumerable<Word> words = page1.GetWords();
                        Assert.AreEqual(3, words.Count());
                        Assert.AreEqual("1 Page 1", string.Join(" ", words));

                        Assert.AreEqual(expectedFontName, $"{words.ElementAt(0).FontName}");
                    }
                }
            }
        }

        /// <summary>
        /// Asserts that passing header-font-name "Times New Roman" sets the header font family to "Times New Roman".
        /// </summary>
        /// <param name="exeFileName">Name of the executable file.</param>
        /// <param name="expectedFontName">Expected name of the font.</param>
        [TestMethod]
        [DataRow(HtmlToPdfRunner.HtmlToPdfExe, "AAAAAA+TimesNewRomanPSMT", DisplayName = "HtmlToPdf.exe")]
        [DataRow(HtmlToPdfRunner.WkhtmltopdfExe, "TimesNewRomanRegular", DisplayName = "wkhtmltopdf.exe")]
        public void HeaderFontName_WithTimesNewRoman_SetsTheHeaderFontFamilyToTimesNewRoman(string exeFileName, string expectedFontName)
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
                    string commandLine = $"--header-font-name \"Times New Roman\" --header-right [page] \"{htmlFile.FilePath}\" \"{pdfFile.FilePath}\"";
                    HtmlToPdfRunResult result = runner.Run(commandLine);
                    Assert.AreEqual(0, result.ExitCode, result.Output);

                    using (var pdfDocument = UglyToad.PdfPig.PdfDocument.Open(pdfFile.FilePath))
                    {
                        Assert.AreEqual(1, pdfDocument.NumberOfPages);
                        Page page1 = pdfDocument.GetPage(1);
                        IEnumerable<Word> words = page1.GetWords();
                        Assert.AreEqual(3, words.Count());
                        Assert.AreEqual("1 Page 1", string.Join(" ", words));

                        Assert.AreEqual(expectedFontName, $"{words.ElementAt(0).FontName}");
                    }
                }
            }
        }
    }
}
