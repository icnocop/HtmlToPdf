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
        /// Asserts the default value causes JavaScript to count more than 100 milliseconds since the page has loaded.
        /// </summary>
        [TestMethod]
        public void DefaultValue_JavascriptCountedMillisecondsOnPageLoad_IsGreaterThan100()
        {
            RunResult runResult = this.Run();
            int milliseconds = runResult.Milliseconds;
            Assert.IsTrue(milliseconds > 100, runResult.PageText);
        }

        /// <summary>
        /// Asserts that passing a JavaScript delay of 3000 milliseconds causes JavaScript to count more than 500 milliseconds since the page has loaded.
        /// </summary>
        [TestMethod]
        public void ThreeThousand_JavascriptCountedMillisecondsOnPageLoad_IsGreaterThan500()
        {
            RunResult runResult = this.Run(3000);
            int milliseconds = runResult.Milliseconds;
            Assert.IsTrue(milliseconds > 500, runResult.PageText);
        }

        private RunResult Run(int? javascriptDelay = null)
        {
            string htmlFilePath = "JavascriptCounter.html";
            HtmlToPdfRunner runner = new HtmlToPdfRunner();

            using (TempPdfFile pdfFile = new TempPdfFile(this.TestContext))
            {
                string commandLine = string.Empty;

                if (javascriptDelay.HasValue)
                {
                    commandLine = $"--javascript-delay {javascriptDelay} ";
                }

                commandLine += $"\"{htmlFilePath}\" \"{pdfFile.FilePath}\"";
                HtmlToPdfRunResult result = runner.Run(commandLine);
                Assert.AreEqual(0, result.ExitCode, result.Output);

                using (var pdfDocument = UglyToad.PdfPig.PdfDocument.Open(pdfFile.FilePath))
                {
                    Assert.AreEqual(1, pdfDocument.NumberOfPages);
                    Page page = pdfDocument.GetPage(1);

                    return new RunResult
                    {
                        PageText = page.Text,
                        Milliseconds = int.Parse(page.Text)
                    };
                }
            }
        }

        private class RunResult
        {
            public string PageText { get; set; }

            public int Milliseconds { get; set; }
        }
    }
}
