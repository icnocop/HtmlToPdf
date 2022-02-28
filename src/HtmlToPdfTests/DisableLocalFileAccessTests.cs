// <copyright file="DisableLocalFileAccessTests.cs" company="HtmlToPdf">
// Copyright (c) HtmlToPdf. All rights reserved.
// </copyright>

namespace HtmlToPdfTests
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using UglyToad.PdfPig.Content;

    /// <summary>
    /// Disable Local File Access Tests.
    /// </summary>
    [TestClass]
    public class DisableLocalFileAccessTests
    {
        /// <summary>
        /// Gets or sets the test context.
        /// </summary>
        public TestContext TestContext { get; set; }

        /// <summary>
        /// Asserts that passing disable-local-file-access doesn't produce an error.
        /// </summary>
        /// <param name="exeFileName">Name of the executable file.</param>
        [TestMethod]
        [DataRow(HtmlToPdfRunner.HtmlToPdfExe, DisplayName = "HtmlToPdf.exe")]
        [DataRow(HtmlToPdfRunner.WkhtmltopdfExe, DisplayName = "wkhtmltopdf.exe")]
        public void DisableLocalFileAccess_DoesNotError(string exeFileName)
        {
            HtmlToPdfRunner runner = new HtmlToPdfRunner(exeFileName);

            // create HTML file
            string html = $@"<!DOCTYPE html>
<html>
<head>
</head>
<body>
Page 1
</body>
</html>";

            using (TempHtmlFile htmlFile = new TempHtmlFile(html, this.TestContext))
            {
                using (TempPdfFile pdfFile = new TempPdfFile(this.TestContext))
                {
                    string commandLine = $"--disable-local-file-access \"{htmlFile.FilePath}\" \"{pdfFile.FilePath}\"";
                    HtmlToPdfRunResult result = runner.Run(commandLine);
                    Assert.AreEqual(0, result.ExitCode, result.Output);

                    using (var pdfDocument = UglyToad.PdfPig.PdfDocument.Open(pdfFile.FilePath))
                    {
                        Assert.AreEqual(1, pdfDocument.NumberOfPages);
                        Page page1 = pdfDocument.GetPage(1);
                        Assert.AreEqual("Page 1", page1.Text);
                    }
                }
            }
        }
    }
}