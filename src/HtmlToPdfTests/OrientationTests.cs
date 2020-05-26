// <copyright file="OrientationTests.cs" company="HtmlToPdf">
// Copyright (c) HtmlToPdf. All rights reserved.
// </copyright>

namespace HtmlToPdfTests
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using UglyToad.PdfPig.Content;

    /// <summary>
    /// Orientation Tests
    /// </summary>
    [TestClass]
    public class OrientationTests
    {
        /// <summary>
        /// Gets or sets the test context.
        /// </summary>
        public TestContext TestContext { get; set; }

        /// <summary>
        /// Asserts that passing a orientation value sets the page orientation.
        /// </summary>
        /// <param name="exeFileName">Name of the executable file.</param>
        /// <param name="orientation">The orientation.</param>
        /// <param name="pageSize">Size of the page.</param>
        /// <param name="width">The width.</param>
        /// <param name="height">The height.</param>
        [TestMethod]
        [DataRow(HtmlToPdfRunner.HtmlToPdfExe, null, PageSize.Custom, 594, 841, DisplayName = "HtmlToPdf.exe Default")]
        [DataRow(HtmlToPdfRunner.WkhtmltopdfExe, null, PageSize.A4, 595, 842, DisplayName = "wkhtmltopdf.exe Default")]
        [DataRow(HtmlToPdfRunner.HtmlToPdfExe, "Portrait", PageSize.Custom, 594, 841, DisplayName = "HtmlToPdf.exe Portrait")]
        [DataRow(HtmlToPdfRunner.WkhtmltopdfExe, "Portrait", PageSize.A4, 595, 842, DisplayName = "wkhtmltopdf.exe Portrait")]
        [DataRow(HtmlToPdfRunner.HtmlToPdfExe, "Landscape", PageSize.Custom, 841, 594, DisplayName = "HtmlToPdf.exe Landscape")]
        [DataRow(HtmlToPdfRunner.WkhtmltopdfExe, "Landscape", PageSize.Custom, 842, 595, DisplayName = "wkhtmltopdf.exe Landscape")]
        public void OrientationTest(
            string exeFileName,
            string orientation,
            PageSize pageSize,
            int width,
            int height)
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

            using (TempHtmlFile htmlFile = new TempHtmlFile(html))
            {
                using (TempPdfFile pdfFile = new TempPdfFile(this.TestContext))
                {
                    string commandLine = string.Empty;

                    if (!string.IsNullOrEmpty(orientation))
                    {
                        commandLine += $"--orientation {orientation} ";
                    }

                    commandLine += $"\"{htmlFile.FilePath}\" \"{pdfFile.FilePath}\"";
                    HtmlToPdfRunResult result = runner.Run(commandLine);
                    Assert.AreEqual(0, result.ExitCode, result.Output);

                    using (var pdfDocument = UglyToad.PdfPig.PdfDocument.Open(pdfFile.FilePath))
                    {
                        Assert.AreEqual(1, pdfDocument.NumberOfPages);
                        Page page = pdfDocument.GetPage(1);
                        Assert.AreEqual(pageSize, page.Size);
                        Assert.AreEqual(width, page.Width);
                        Assert.AreEqual(height, page.Height);
                        Assert.AreEqual(0, page.Rotation.Radians);
                        Assert.AreEqual(0, page.Rotation.Value);
                    }
                }
            }
        }
    }
}
