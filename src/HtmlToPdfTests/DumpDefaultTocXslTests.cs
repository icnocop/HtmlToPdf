// <copyright file="DumpDefaultTocXslTests.cs" company="HtmlToPdf">
// Copyright (c) HtmlToPdf. All rights reserved.
// </copyright>

namespace HtmlToPdfTests
{
    using HtmlToPdf.Console;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>
    /// Dump Default TOC XSL tests.
    /// </summary>
    [TestClass]
    public class DumpDefaultTocXslTests
    {
        /// <summary>
        /// Gets or sets the test context.
        /// </summary>
        public TestContext TestContext { get; set; }

        /// <summary>
        /// Asserts that passing dump-default-toc-xsl outputs the default TOC XSL to the standard output stream.
        /// </summary>
        /// <param name="exeFileName">Name of the executable file.</param>
        /// <param name="removeEmptyLines">if set to <c>true</c> removes empty lines from the output.</param>
        [TestMethod]
        [DataRow(HtmlToPdfRunner.HtmlToPdfExe, false, DisplayName = "HtmlToPdf.exe")]
        [DataRow(HtmlToPdfRunner.WkhtmltopdfExe, true, DisplayName = "wkhtmltopdf.exe")]
        public void DumpDefaultTocXsl_OutputsDefaultTocXslToStandardOutput(string exeFileName, bool removeEmptyLines)
        {
            HtmlToPdfRunner runner = new HtmlToPdfRunner(exeFileName);

            string commandLine = "--dump-default-toc-xsl";
            HtmlToPdfRunResult result = runner.Run(commandLine);
            Assert.AreEqual(0, result.ExitCode, result.Output);

            // remove empty lines
            string output = result.StandardOutput;

            if (removeEmptyLines)
            {
                output = output
                    .Replace("\r\n", "\r")
                    .Replace("\r\r", "\r\n");
            }

            string expected = TableOfContentsStyleSheetBuilder.Build(true);
            StringAssertWithDiff.AreEqual(expected.Trim(), output.Trim());
        }
    }
}
