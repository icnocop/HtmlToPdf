// <copyright file="InputTests.cs" company="HtmlToPdf">
// Copyright (c) HtmlToPdf. All rights reserved.
// </copyright>

namespace HtmlToPdfTests
{
    using System.IO;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>
    /// Input Tests
    /// </summary>
    [TestClass]
    public class InputTests
    {
        /// <summary>
        /// Gets or sets the test context.
        /// </summary>
        public TestContext TestContext { get; set; }

        /// <summary>
        /// Asserts that passing an input file that doesn't exist returns an error.
        /// </summary>
        [TestMethod]
        public void Input_WithFileDoesNotExist_ReturnsAnError()
        {
            HtmlToPdfRunner runner = new HtmlToPdfRunner(HtmlToPdfRunner.HtmlToPdfExe);

            using (TempHtmlFile htmlFile = new TempHtmlFile())
            {
                File.Delete(htmlFile.FilePath);

                using (TempPdfFile pdfFile = new TempPdfFile(this.TestContext))
                {
                    string commandLine = $"--log-level Error \"{htmlFile.FilePath}\" \"{pdfFile.FilePath}\"";
                    HtmlToPdfRunResult result = runner.Run(commandLine);
                    Assert.AreEqual(1, result.ExitCode);

                    string expectedOutput = HelpTextGenerator.GenerateParserError(
                        "A required value not bound to option name is missing.",
                        "System.IO.FileNotFoundException: File not found: ");
                    Assert.IsTrue(string.IsNullOrEmpty(result.StandardOutput), result.StandardOutput);
                    Assert.IsTrue(result.StandardError.Trim().StartsWith(expectedOutput.Trim()), result.StandardError);
                }
            }
        }
    }
}
