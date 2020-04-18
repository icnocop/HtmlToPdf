// <copyright file="RightMarginTests.cs" company="HtmlToPdf">
// Copyright (c) HtmlToPdf. All rights reserved.
// </copyright>

namespace HtmlToPdfTests
{
    using System;
    using System.Linq;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using UglyToad.PdfPig.Content;

    /// <summary>
    /// Right Margin Tests
    /// </summary>
    [TestClass]
    public class RightMarginTests
    {
        /// <summary>
        /// Gets or sets the test context.
        /// </summary>
        public TestContext TestContext { get; set; }

        /// <summary>
        /// Asserts that passing a right margin value sets the right margin.
        /// </summary>
        /// <param name="rightMargin">The right margin.</param>
        /// <param name="expectedMinX">The expected minimum x.</param>
        /// <param name="expectedMaxX">The expected maximum x.</param>
        /// <param name="expectedMinY">The expected minimum y.</param>
        /// <param name="expectedMaxY">The expected maximum y.</param>
        [TestMethod]
        [DataRow(null, 41.03, 568, 41.03, 841.92, DisplayName = "Default")]
        [DataRow("0in", 41.03, 595, 41.03, 841.92, DisplayName = "0in")]
        public void RightMarginTest(
            string rightMargin,
            double expectedMinX,
            double expectedMaxX,
            double expectedMinY,
            double expectedMaxY)
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

                    if (!string.IsNullOrEmpty(rightMargin))
                    {
                        commandLine += $"--margin-right {rightMargin} ";
                    }

                    commandLine += $"\"{htmlFile.FilePath}\" \"{pdfFile.FilePath}\"";
                    HtmlToPdfRunResult result = runner.Run(commandLine);
                    Assert.AreEqual(0, result.ExitCode, result.Output);

                    using (var pdfDocument = UglyToad.PdfPig.PdfDocument.Open(pdfFile.FilePath))
                    {
                        Assert.AreEqual(1, pdfDocument.NumberOfPages);
                        Page page = pdfDocument.GetPage(1);

                        var bboxes = page.Letters.Select(x => x.GlyphRectangle).ToList();
                        bboxes.AddRange(page.ExperimentalAccess.Paths.Select(x => x.GetBoundingRectangle()).Where(x => x.HasValue).Select(x => x.Value));

                        var minX = bboxes.Min(x => x.Right);
                        var maxX = bboxes.Max(x => x.Right);
                        var minY = bboxes.Min(x => x.Right);
                        var maxY = bboxes.Max(x => x.Top);

                        Assert.AreEqual(expectedMinX, Math.Round(minX, 2));
                        Assert.AreEqual(expectedMaxX, Math.Round(maxX, 2));
                        Assert.AreEqual(expectedMinY, Math.Round(minY, 2));
                        Assert.AreEqual(expectedMaxY, Math.Round(maxY, 2));
                    }
                }
            }
        }
    }
}
