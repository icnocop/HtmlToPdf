// <copyright file="MarginTests.cs" company="HtmlToPdf">
// Copyright (c) HtmlToPdf. All rights reserved.
// </copyright>

namespace HtmlToPdfTests
{
    using System;
    using System.Linq;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using UglyToad.PdfPig.Content;

    /// <summary>
    /// Margin Tests
    /// </summary>
    [TestClass]
    public class MarginTests
    {
        /// <summary>
        /// Gets or sets the test context.
        /// </summary>
        public TestContext TestContext { get; set; }

        /// <summary>
        /// Asserts that passing left margin and right margin values set the left and right margins.
        /// </summary>
        /// <param name="exeFileName">Name of the executable file.</param>
        /// <param name="leftMargin">The left margin.</param>
        /// <param name="rightMargin">The right margin.</param>
        /// <param name="topMargin">The top margin.</param>
        /// <param name="bottomMargin">The bottom margin.</param>
        /// <param name="expectedMinX">The expected minimum x.</param>
        /// <param name="expectedMaxX">The expected maximum x.</param>
        /// <param name="expectedMinY">The expected minimum y.</param>
        /// <param name="expectedMaxY">The expected maximum y.</param>
        [TestMethod]
        [DataRow(HtmlToPdfRunner.HtmlToPdfExe, null, null, null, null, 28, 568, 28, 841.92, DisplayName = "HtmlToPdf.exe Default")]
        [DataRow(HtmlToPdfRunner.WkhtmltopdfExe, null, null, null, null, 33.61, 69.39, 33.61, 811.31, DisplayName = "wkhtmltopdf.exe Default")]
        [DataRow(HtmlToPdfRunner.HtmlToPdfExe, "0mm", "0mm", "0mm", "0mm", 0, 595, 0, 841.92, DisplayName = "HtmlToPdf.exe 0mm")]
        [DataRow(HtmlToPdfRunner.WkhtmltopdfExe, "0mm", "0mm", "0mm", "0mm", 5.11, 40.89, 5.11, 839.81, DisplayName = "wkhtmltopdf.exe 0mm")]
        [DataRow(HtmlToPdfRunner.HtmlToPdfExe, "5mm", "10mm", "15mm", "20mm", 14, 567, 14, 799.92, DisplayName = "HtmlToPdf.exe")]
        [DataRow(HtmlToPdfRunner.WkhtmltopdfExe, "5mm", "10mm", "15mm", "20mm", 19.36, 55.13, 19.36, 797.06, DisplayName = "wkhtmltopdf.exe")]
        public void MarginTest(
            string exeFileName,
            string leftMargin,
            string rightMargin,
            string topMargin,
            string bottomMargin,
            double expectedMinX,
            double expectedMaxX,
            double expectedMinY,
            double expectedMaxY)
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

                    if (!string.IsNullOrEmpty(leftMargin))
                    {
                        commandLine += $"--margin-left {leftMargin} ";
                    }

                    if (!string.IsNullOrEmpty(rightMargin))
                    {
                        commandLine += $"--margin-right {rightMargin} ";
                    }

                    if (!string.IsNullOrEmpty(topMargin))
                    {
                        commandLine += $"--margin-top {topMargin} ";
                    }

                    if (!string.IsNullOrEmpty(bottomMargin))
                    {
                        commandLine += $"--margin-bottom {bottomMargin} ";
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

                        var minX = bboxes.Min(x => x.Left);
                        var maxX = bboxes.Max(x => x.Right);
                        var minY = bboxes.Min(x => x.Left);
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
