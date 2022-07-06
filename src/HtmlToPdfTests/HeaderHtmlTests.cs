// <copyright file="HeaderHtmlTests.cs" company="HtmlToPdf">
// Copyright (c) HtmlToPdf. All rights reserved.
// </copyright>

namespace HtmlToPdfTests
{
    using System.Collections.Generic;
    using System.IO;
    using HtmlToPdf;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using UglyToad.PdfPig.Content;

    /// <summary>
    /// Header HTTML Tests.
    /// </summary>
    [TestClass]
    public class HeaderHtmlTests
    {
        /// <summary>
        /// Gets or sets the test context.
        /// </summary>
        public TestContext TestContext { get; set; }

        /// <summary>
        /// Asserts that passing header-html with a local file sets the header with the contents of the file.
        /// </summary>
        /// <param name="exeFileName">Name of the executable file.</param>
        [TestMethod]
        [DataRow(HtmlToPdfRunner.HtmlToPdfExe, DisplayName = "HtmlToPdf.exe")]
        [DataRow(HtmlToPdfRunner.WkhtmltopdfExe, DisplayName = "wkhtmltopdf.exe")]
        public void HeaderHtml_WithLocalFilePath_SetsTheHeaderWithContentsOfFile(string exeFileName)
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

            string headerHtml = @"<div>New Header HTML</div>";

            using (TempHtmlFile htmlFile = new TempHtmlFile(html))
            {
                using (TempHtmlFile htmlHeaderFile = new TempHtmlFile(headerHtml))
                {
                    using (TempPdfFile pdfFile = new TempPdfFile(this.TestContext))
                    {
                        string commandLine = $"--header-html \"{htmlHeaderFile.FilePath}\" \"{htmlFile.FilePath}\" \"{pdfFile.FilePath}\"";
                        HtmlToPdfRunResult result = runner.Run(commandLine);
                        Assert.AreEqual(0, result.ExitCode, result.Output);

                        using (var pdfDocument = UglyToad.PdfPig.PdfDocument.Open(pdfFile.FilePath))
                        {
                            Assert.AreEqual(1, pdfDocument.NumberOfPages);
                            Page page1 = pdfDocument.GetPage(1);
                            IEnumerable<Word> words = page1.GetWords();
                            Assert.AreEqual("New Header HTML Page 1", string.Join(" ", words));
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Asserts that passing header-html with a URL sets the header with the contents of the URL.
        /// </summary>
        /// <param name="exeFileName">Name of the executable file.</param>
        [TestMethod]
        [DataRow(HtmlToPdfRunner.HtmlToPdfExe, DisplayName = "HtmlToPdf.exe")]
        [DataRow(HtmlToPdfRunner.WkhtmltopdfExe, DisplayName = "wkhtmltopdf.exe")]
        public void HeaderHtml_WithUrl_SetsTheHeaderWithContentsOfUrl(string exeFileName)
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

            string headerHtml = @"<div>New Header HTML</div>";

            using (TempHtmlFile htmlFile = new TempHtmlFile(html))
            {
                using (var tempDirectory = new TempDirectory())
                {
                    string headerHtmlFileName = "header.html";
                    string headerHtmlFilePath = Path.Combine(tempDirectory.DirectoryPath, headerHtmlFileName);
                    File.WriteAllText(headerHtmlFilePath, headerHtml);

                    using (var server = new TestWebServer("/", tempDirectory.DirectoryPath))
                    {
                        string headerHtmlUrl = $"{server.RootUrl}{headerHtmlFileName}";
                        using (TempPdfFile pdfFile = new TempPdfFile(this.TestContext))
                        {
                            string commandLine = $"--header-html \"{headerHtmlUrl}\" \"{htmlFile.FilePath}\" \"{pdfFile.FilePath}\"";
                            HtmlToPdfRunResult result = runner.Run(commandLine);
                            Assert.AreEqual(0, result.ExitCode, result.Output);

                            using (var pdfDocument = UglyToad.PdfPig.PdfDocument.Open(pdfFile.FilePath))
                            {
                                Assert.AreEqual(1, pdfDocument.NumberOfPages);
                                Page page1 = pdfDocument.GetPage(1);
                                IEnumerable<Word> words = page1.GetWords();
                                Assert.AreEqual("New Header HTML Page 1", string.Join(" ", words));
                            }
                        }
                    }
                }
            }
        }
    }
}
