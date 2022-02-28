// <copyright file="PageSizeTests.cs" company="HtmlToPdf">
// Copyright (c) HtmlToPdf. All rights reserved.
// </copyright>

namespace HtmlToPdfTests
{
    using System;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using UglyToad.PdfPig.Content;

    /// <summary>
    /// Page Size Tests.
    /// </summary>
    [TestClass]
    public class PageSizeTests
    {
        /// <summary>
        /// Gets or sets the test context.
        /// </summary>
        public TestContext TestContext { get; set; }

        /// <summary>
        /// Asserts that passing an invalid page size returns an error.
        /// </summary>
        [TestMethod]
        public void InvalidPageSize_ReturnsError()
        {
            HtmlToPdfRunner runner = new HtmlToPdfRunner(HtmlToPdfRunner.HtmlToPdfExe);

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
                    string[] commandLine = new[]
                    {
                        "--log-level Error",
                        "--page-size invalid",
                        $"\"{htmlFile.FilePath}\"",
                        $"\"{pdfFile.FilePath}\"",
                    };

                    HtmlToPdfRunResult result = runner.Run(string.Join(" ", commandLine));
                    Assert.AreEqual(1, result.ExitCode);

                    string[] expectedErrorMessage = new[]
                    {
                        "System.ArgumentOutOfRangeException: Specified argument was out of the range of valid values.",
                        "Parameter name: invalid",
                    };

                    string expectedOutput = string.Join(Environment.NewLine, expectedErrorMessage);
                    Assert.IsTrue(string.IsNullOrEmpty(result.StandardOutput), result.StandardOutput);
                    Assert.IsTrue(result.StandardError.Trim().StartsWith(expectedOutput.Trim()), result.StandardError);
                }
            }
        }

        /// <summary>
        /// Asserts that passing a page size value sets the page size.
        /// </summary>
        /// <param name="exeFileName">Name of the executable file.</param>
        /// <param name="pageSizeCommandLineParameter">The page size command line parameter.</param>
        /// <param name="pageSize">Size of the page.</param>
        /// <param name="width">The width.</param>
        /// <param name="height">The height.</param>
        [TestMethod]
        [DataRow(HtmlToPdfRunner.HtmlToPdfExe, null, PageSize.Custom, 594, 841, DisplayName = "HtmlToPdf.exe Default")]
        [DataRow(HtmlToPdfRunner.WkhtmltopdfExe, null, PageSize.A4, 595, 842, DisplayName = "wkhtmltopdf.exe Default")]
        [DataRow(HtmlToPdfRunner.HtmlToPdfExe, "A4", PageSize.Custom, 594, 841, DisplayName = "HtmlToPdf.exe A4")]
        [DataRow(HtmlToPdfRunner.WkhtmltopdfExe, "A4", PageSize.A4, 595, 842, DisplayName = "wkhtmltopdf.exe A4")]
        [DataRow(HtmlToPdfRunner.HtmlToPdfExe, "A3", PageSize.Custom, 841, 1189, DisplayName = "HtmlToPdf.exe A3")]
        [DataRow(HtmlToPdfRunner.WkhtmltopdfExe, "A3", PageSize.A3, 842, 1191, DisplayName = "wkhtmltopdf.exe A3")]
        [DataRow(HtmlToPdfRunner.HtmlToPdfExe, "A2", PageSize.Custom, 1189, 1684, DisplayName = "HtmlToPdf.exe A2")]
        [DataRow(HtmlToPdfRunner.WkhtmltopdfExe, "A2", PageSize.A2, 1191, 1684, DisplayName = "wkhtmltopdf.exe A2")]
        [DataRow(HtmlToPdfRunner.HtmlToPdfExe, "A1", PageSize.Custom, 1684, 2382, DisplayName = "HtmlToPdf.exe A1")]
        [DataRow(HtmlToPdfRunner.WkhtmltopdfExe, "A1", PageSize.A1, 1684, 2384, DisplayName = "wkhtmltopdf.exe A1")]
        [DataRow(HtmlToPdfRunner.HtmlToPdfExe, "A0", PageSize.Custom, 2382, 3368, DisplayName = "HtmlToPdf.exe A0")]
        [DataRow(HtmlToPdfRunner.WkhtmltopdfExe, "A0", PageSize.A0, 2384, 3370, DisplayName = "wkhtmltopdf.exe A0")]
        [DataRow(HtmlToPdfRunner.HtmlToPdfExe, "Ledger", PageSize.Ledger, 1224, 792, DisplayName = "HtmlToPdf.exe Ledger")]
        [DataRow(HtmlToPdfRunner.WkhtmltopdfExe, "Ledger", PageSize.Ledger, 1224, 792, DisplayName = "wkhtmltopdf.exe Ledger")]
        [DataRow(HtmlToPdfRunner.HtmlToPdfExe, "Tabloid", PageSize.Tabloid, 792, 1224, DisplayName = "HtmlToPdf.exe Tabloid")]
        [DataRow(HtmlToPdfRunner.WkhtmltopdfExe, "Tabloid", PageSize.Tabloid, 792, 1224, DisplayName = "wkhtmltopdf.exe Tabloid")]
        [DataRow(HtmlToPdfRunner.HtmlToPdfExe, "A5", PageSize.Custom, 419, 594, DisplayName = "HtmlToPdf.exe A5")]
        [DataRow(HtmlToPdfRunner.WkhtmltopdfExe, "A5", PageSize.A5, 420, 595, DisplayName = "wkhtmltopdf.exe A5")]
        [DataRow(HtmlToPdfRunner.HtmlToPdfExe, "Legal", PageSize.Legal, 612, 1008, DisplayName = "HtmlToPdf.exe Legal")]
        [DataRow(HtmlToPdfRunner.WkhtmltopdfExe, "Legal", PageSize.Legal, 612, 1008, DisplayName = "wkhtmltopdf.exe Legal")]
        [DataRow(HtmlToPdfRunner.HtmlToPdfExe, "Letter", PageSize.Letter, 612, 792, DisplayName = "HtmlToPdf.exe Letter")]
        [DataRow(HtmlToPdfRunner.WkhtmltopdfExe, "Letter", PageSize.Letter, 612, 792, DisplayName = "wkhtmltopdf.exe Letter")]
        [DataRow(HtmlToPdfRunner.HtmlToPdfExe, "A6", PageSize.Custom, 297, 419, DisplayName = "HtmlToPdf.exe A6")]
        [DataRow(HtmlToPdfRunner.WkhtmltopdfExe, "A6", PageSize.A6, 298, 420, DisplayName = "wkhtmltopdf.exe A6")]
        public void PageSizeTest(
            string exeFileName,
            string pageSizeCommandLineParameter,
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

                    if (!string.IsNullOrEmpty(pageSizeCommandLineParameter))
                    {
                        commandLine += $"--page-size {pageSizeCommandLineParameter} ";
                    }

                    commandLine += $"\"{htmlFile.FilePath}\" \"{pdfFile.FilePath}\"";
                    HtmlToPdfRunResult result = runner.Run(commandLine);
                    Assert.AreEqual(0, result.ExitCode, result.Output);

                    using (var pdfDocument = UglyToad.PdfPig.PdfDocument.Open(pdfFile.FilePath))
                    {
                        Assert.AreEqual(1, pdfDocument.NumberOfPages);
                        Page page = pdfDocument.GetPage(1);
                        string message = $"Size: {page.Size} Height: {page.Height} Width: {page.Width}";
                        Assert.AreEqual(pageSize, page.Size, message);
                        Assert.AreEqual(width, page.Width, message);
                        Assert.AreEqual(height, page.Height, message);
                    }
                }
            }
        }
    }
}
