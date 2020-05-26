// <copyright file="JavascriptDelayInMillisecondsTests.cs" company="HtmlToPdf">
// Copyright (c) HtmlToPdf. All rights reserved.
// </copyright>

namespace HtmlToPdfTests
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using UglyToad.PdfPig.Content;

    /// <summary>
    /// JavaScript delay in milliseconds tests
    /// </summary>
    [TestClass]
    public class JavascriptDelayInMillisecondsTests
    {
        /// <summary>
        /// Gets or sets the test context.
        /// </summary>
        public TestContext TestContext { get; set; }

        /// <summary>
        /// Asserts that not passing a javascript delay returns an error.
        /// </summary>
        [TestMethod]
        public void NoJavascriptDelay_ReturnsError()
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
                        "--javascript-delay",
                        $"\"{htmlFile.FilePath}\"",
                        $"\"{pdfFile.FilePath}\""
                    };

                    HtmlToPdfRunResult result = runner.Run(string.Join(" ", commandLine));
                    Assert.AreEqual(1, result.ExitCode);

                    string expectedOutput = HelpTextGenerator.Generate(
                        "Option 'javascript-delay' is defined with a bad format.");
                    Assert.IsTrue(string.IsNullOrEmpty(result.StandardOutput), result.StandardOutput);
                    Assert.IsTrue(result.StandardError.Trim().StartsWith(expectedOutput.Trim()), result.StandardError);
                }
            }
        }

        /// <summary>
        /// Asserts that passing a Javascript delay delays the specified number of milliseconds.
        /// </summary>
        /// <param name="exeFileName">Name of the executable file.</param>
        /// <param name="javascriptDelayInMilliseconds">The javascript delay in milliseconds.</param>
        /// <param name="expectedMinJavascriptDelayInMilliseconds">The expected minimum javascript delay in milliseconds.</param>
        [TestMethod]
        [DataRow(HtmlToPdfRunner.HtmlToPdfExe, null, 100, DisplayName = "HtmlToPdf.exe Default")]
        [DataRow(HtmlToPdfRunner.WkhtmltopdfExe, null, 21, DisplayName = "wkhtmltopdf.exe Default")]
        [DataRow(HtmlToPdfRunner.HtmlToPdfExe, "3000", 500, DisplayName = "HtmlToPdf.exe 3000")]
        [DataRow(HtmlToPdfRunner.WkhtmltopdfExe, "3000", 298, DisplayName = "wkhtmltopdf.exe 3000")]
        public void JavascriptDelayTest(
            string exeFileName,
            string javascriptDelayInMilliseconds,
            double expectedMinJavascriptDelayInMilliseconds)
        {
            string htmlFilePath = "JavascriptCounter.html";
            HtmlToPdfRunner runner = new HtmlToPdfRunner(exeFileName);

            using (TempPdfFile pdfFile = new TempPdfFile(this.TestContext))
            {
                string commandLine = string.Empty;

                if (!string.IsNullOrEmpty(javascriptDelayInMilliseconds))
                {
                    commandLine = $"--javascript-delay {javascriptDelayInMilliseconds} ";
                }

                commandLine += $"\"{htmlFilePath}\" \"{pdfFile.FilePath}\"";
                HtmlToPdfRunResult result = runner.Run(commandLine);
                Assert.AreEqual(0, result.ExitCode, result.Output);

                using (var pdfDocument = UglyToad.PdfPig.PdfDocument.Open(pdfFile.FilePath))
                {
                    Assert.AreEqual(1, pdfDocument.NumberOfPages);
                    Page page = pdfDocument.GetPage(1);

                    int milliseconds = int.Parse(page.Text);
                    Assert.IsTrue(milliseconds > expectedMinJavascriptDelayInMilliseconds, page.Text);
                }
            }
        }
    }
}
