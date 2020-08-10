// <copyright file="OutlineTests.cs" company="HtmlToPdf">
// Copyright (c) HtmlToPdf. All rights reserved.
// </copyright>

namespace HtmlToPdfTests
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using UglyToad.PdfPig.Content;
    using UglyToad.PdfPig.Outline;

    /// <summary>
    /// Outline tests
    /// </summary>
    [TestClass]
    public class OutlineTests
    {
        /// <summary>
        /// Gets or sets the test context.
        /// </summary>
        public TestContext TestContext { get; set; }

        /// <summary>
        /// Asserts that passing outline doesn't produce an error.
        /// </summary>
        /// <param name="exeFileName">Name of the executable file.</param>
        [TestMethod]
        [DataRow(HtmlToPdfRunner.HtmlToPdfExe, DisplayName = "HtmlToPdf.exe")]
        [DataRow(HtmlToPdfRunner.WkhtmltopdfExe, DisplayName = "wkhtmltopdf.exe")]
        public void Outline_DoesNotError(string exeFileName)
        {
            HtmlToPdfRunner runner = new HtmlToPdfRunner(exeFileName);

            string html = @"<!DOCTYPE html>
<html>
  <head>
  </head>
  <body>
   <h1>Test Page</h1>
  </body>
</html>";

            using (TempHtmlFile htmlFile = new TempHtmlFile(html))
            {
                using (TempPdfFile pdfFile = new TempPdfFile(this.TestContext))
                {
                    string commandLine = $"--outline \"{htmlFile.FilePath}\" \"{pdfFile.FilePath}\"";
                    HtmlToPdfRunResult result = runner.Run(commandLine);
                    Assert.AreEqual(0, result.ExitCode, result.Output);

                    using (var pdfDocument = UglyToad.PdfPig.PdfDocument.Open(pdfFile.FilePath))
                    {
                        Assert.AreEqual(1, pdfDocument.NumberOfPages);
                        Page page = pdfDocument.GetPage(1);

                        bool containsBookmarks = pdfDocument.TryGetBookmarks(out Bookmarks bookmarks);
                        Assert.IsTrue(containsBookmarks);

                        Assert.AreEqual(1, bookmarks.Roots.Count);
                        BookmarkNode rootBookmark = bookmarks.Roots[0];
                        Assert.AreEqual("Test Page", rootBookmark.Title);
                        Assert.AreEqual(0, rootBookmark.Level);
                        Assert.AreEqual(true, rootBookmark.IsLeaf);
                        Assert.AreEqual(0, rootBookmark.Children.Count);

                        Assert.AreEqual("Test Page", page.Text);
                    }
                }
            }
        }
    }
}
