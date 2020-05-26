// <copyright file="UserStyleSheetTests.cs" company="HtmlToPdf">
// Copyright (c) HtmlToPdf. All rights reserved.
// </copyright>

namespace HtmlToPdfTests
{
    using System.Linq;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using UglyToad.PdfPig.Content;
    using UglyToad.PdfPig.Graphics.Colors;

    /// <summary>
    /// User Style Sheet Tests
    /// </summary>
    [TestClass]
    public class UserStyleSheetTests
    {
        /// <summary>
        /// Gets or sets the test context.
        /// </summary>
        public TestContext TestContext { get; set; }

        /// <summary>
        /// Asserts passing user-style-sheet with CSS setting the body color to blue makes the letters in the PDF blue.
        /// </summary>
        /// <param name="exeFileName">Name of the executable file.</param>
        [TestMethod]
        [DataRow(HtmlToPdfRunner.HtmlToPdfExe, DisplayName = "HtmlToPdf.exe")]
        [DataRow(HtmlToPdfRunner.WkhtmltopdfExe, DisplayName = "wkhtmltopdf.exe")]
        public void UserStyleSheet_WithBodyColorBlue_MakesLettersBlue(string exeFileName)
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

            string css = @"body { color: blue; }";

            using (TempHtmlFile htmlFile = new TempHtmlFile(html))
            {
                using (TempCssFile cssFile = new TempCssFile(css))
                {
                    using (TempPdfFile pdfFile = new TempPdfFile(this.TestContext))
                    {
                        string commandLine = $"--user-style-sheet \"{cssFile.FilePath}\" \"{htmlFile.FilePath}\" \"{pdfFile.FilePath}\"";
                        HtmlToPdfRunResult result = runner.Run(commandLine);
                        Assert.AreEqual(0, result.ExitCode, result.Output);

                        using (var pdfDocument = UglyToad.PdfPig.PdfDocument.Open(pdfFile.FilePath))
                        {
                            Assert.AreEqual(1, pdfDocument.NumberOfPages);
                            Page page = pdfDocument.GetPage(1);
                            Assert.IsTrue(page.Letters.All(x => (RGBColor)x.Color == new RGBColor(0, 0, 1)));
                        }
                    }
                }
            }
        }
    }
}
