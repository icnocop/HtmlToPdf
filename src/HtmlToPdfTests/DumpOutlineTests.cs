// <copyright file="DumpOutlineTests.cs" company="HtmlToPdf">
// Copyright (c) HtmlToPdf. All rights reserved.
// </copyright>

namespace HtmlToPdfTests
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>
    /// Dump Outline tests
    /// </summary>
    [TestClass]
    public class DumpOutlineTests
    {
        /// <summary>
        /// Gets or sets the test context.
        /// </summary>
        public TestContext TestContext { get; set; }

        /// <summary>
        /// Asserts that passing dump-outline outputs the outline to the specified file.
        /// </summary>
        /// <param name="exeFileName">Name of the executable file.</param>
        [TestMethod]
        [DataRow(HtmlToPdfRunner.HtmlToPdfExe, DisplayName = "HtmlToPdf.exe")]
        [DataRow(HtmlToPdfRunner.WkhtmltopdfExe, DisplayName = "wkhtmltopdf.exe")]
        public void DumpOutline_OutputsOutlineToFile(string exeFileName)
        {
            HtmlToPdfRunner runner = new HtmlToPdfRunner(exeFileName);

            foreach (string directory in Directory.EnumerateDirectories(Path.Combine(Directory.GetCurrentDirectory(), "TestData", "DumpOutline")))
            {
                string directoryName = new DirectoryInfo(directory).Name;

                IEnumerable<string> htmlFiles = Directory.EnumerateFiles(directory, "*.html");
                string expectedOutlineXmlFile = Path.Combine(directory, "Outline.xml");

                using (TempPdfFile pdfFile = new TempPdfFile(this.TestContext))
                {
                    using (TempFile tempOutlineFile = new TempFile(".xml", this.TestContext))
                    {
                        List<string> commandLineArguments = new List<string>
                        {
                            $"--dump-outline \"{tempOutlineFile.FilePath}\""
                        };

                        commandLineArguments.AddRange(htmlFiles.Select(x => $"\"{x}\""));
                        commandLineArguments.Add($"\"{pdfFile.FilePath}\"");

                        HtmlToPdfRunResult result = runner.Run(string.Join(" ", commandLineArguments));

                        string errorMessage = $"Test: {directoryName}";

                        Assert.AreEqual(0, result.ExitCode, errorMessage + Environment.NewLine + result.Output);

                        XmlAssert.AreEqual(expectedOutlineXmlFile, tempOutlineFile.FilePath, this.TestContext, errorMessage + $" Actual: {tempOutlineFile.FileName}");
                    }
                }
            }
        }
    }
}
