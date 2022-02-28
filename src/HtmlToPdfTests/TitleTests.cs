// <copyright file="TitleTests.cs" company="HtmlToPdf">
// Copyright (c) HtmlToPdf. All rights reserved.
// </copyright>

namespace HtmlToPdfTests
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using UglyToad.PdfPig.Content;

    /// <summary>
    /// Title Tests.
    /// </summary>
    [TestClass]
    public class TitleTests
    {
        /// <summary>
        /// Gets or sets the test context.
        /// </summary>
        public TestContext TestContext { get; set; }

        /// <summary>
        /// Asserts passing a title value sets the PDF title.
        /// </summary>
        /// <param name="exeFileName">Name of the executable file.</param>
        /// <param name="title">The title.</param>
        /// <param name="expectedTitle">The expected title.</param>
        [TestMethod]
        [DataRow(HtmlToPdfRunner.HtmlToPdfExe, null, "Test Page Title", DisplayName = "HtmlToPdf.exe Default")]
        [DataRow(HtmlToPdfRunner.WkhtmltopdfExe, null, "Test Page Title", DisplayName = "wkhtmltopdf.exe Default")]
        [DataRow(HtmlToPdfRunner.HtmlToPdfExe, "My Title", "My Title", DisplayName = "HtmlToPdf.exe Title")]
        [DataRow(HtmlToPdfRunner.WkhtmltopdfExe, "My Title", "My Title", DisplayName = "wkhtmltopdf.exe Title")]
        public void Title_SetsThePdfTitle(string exeFileName, string title, string expectedTitle)
        {
            HtmlToPdfRunner runner = new HtmlToPdfRunner(exeFileName);

            string html = @"<!DOCTYPE html>
<html>
  <head>
    <title>Test Page Title</title>
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

                    if (!string.IsNullOrEmpty(title))
                    {
                        commandLine += $"--title \"{title}\" ";
                    }

                    commandLine += $"\"{htmlFile.FilePath}\" \"{pdfFile.FilePath}\"";
                    HtmlToPdfRunResult result = runner.Run(commandLine);
                    Assert.AreEqual(0, result.ExitCode, result.Output);

                    using (var pdfDocument = UglyToad.PdfPig.PdfDocument.Open(pdfFile.FilePath))
                    {
                        Assert.AreEqual(1, pdfDocument.NumberOfPages);
                        Page page = pdfDocument.GetPage(1);
                        Assert.AreEqual(expectedTitle, pdfDocument.Information.Title);
                    }
                }
            }
        }
    }
}
