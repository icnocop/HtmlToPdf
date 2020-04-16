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
        /// <param name="orientation">The orientation.</param>
        /// <param name="pageSize">Size of the page.</param>
        /// <param name="width">The width.</param>
        /// <param name="height">The height.</param>
        [TestMethod]
        [DataRow(null, PageSize.Custom, 594, 841, DisplayName = "Default")]
        [DataRow("Portrait", PageSize.Custom, 594, 841, DisplayName = "Portrait")]
        [DataRow("Landscape", PageSize.Custom, 841, 594, DisplayName = "Landscape")]
        public void OrientationTest(string orientation, PageSize pageSize, int width, int height)
        {
            HtmlToPdfRunner runner = new HtmlToPdfRunner();

            string html = @"
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
