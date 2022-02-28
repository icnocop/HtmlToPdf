// <copyright file="ReadArgumentsFromStandardInputTests.cs" company="HtmlToPdf">
// Copyright (c) HtmlToPdf. All rights reserved.
// </copyright>

namespace HtmlToPdfTests
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using UglyToad.PdfPig.Content;

    /// <summary>
    /// Read arguments from standard input tests.
    /// </summary>
    [TestClass]
    public class ReadArgumentsFromStandardInputTests
    {
        /// <summary>
        /// Gets or sets the test context.
        /// </summary>
        public TestContext TestContext { get; set; }

        /// <summary>
        /// Asserts that passing inputs into standard input reads arguments from standard input.
        /// </summary>
        /// <param name="exeFileName">Name of the executable file.</param>
        [TestMethod]
        [DataRow(HtmlToPdfRunner.HtmlToPdfExe, DisplayName = "HtmlToPdf.exe")]
        [DataRow(HtmlToPdfRunner.WkhtmltopdfExe, DisplayName = "wkhtmltopdf.exe")]
        public void PassInputsToStandardInput_ReadsArgumentsFromStandardInput(string exeFileName)
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
                    string commandLine = $"--read-args-from-stdin";
                    string stdIn = $"\"{htmlFile.FilePath.Replace("\\", "\\\\")}\" \"{pdfFile.FilePath.Replace("\\", "\\\\")}\"";
                    HtmlToPdfRunResult result = runner.Run(commandLine, stdIn);
                    Assert.AreEqual(0, result.ExitCode, result.Output);

                    using (var pdfDocument = UglyToad.PdfPig.PdfDocument.Open(pdfFile.FilePath))
                    {
                        Assert.AreEqual(1, pdfDocument.NumberOfPages);
                        Page page = pdfDocument.GetPage(1);
                        Assert.AreEqual("Test Page", page.Text);
                    }
                }
            }
        }
    }
}
