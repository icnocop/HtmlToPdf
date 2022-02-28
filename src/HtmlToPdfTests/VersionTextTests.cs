// <copyright file="VersionTextTests.cs" company="HtmlToPdf">
// Copyright (c) HtmlToPdf. All rights reserved.
// </copyright>

namespace HtmlToPdfTests
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>
    /// Version text tests.
    /// </summary>
    [TestClass]
    public class VersionTextTests
    {
        /// <summary>
        /// Asserts that passing the version command line option displays the version information.
        /// </summary>
        /// <param name="commandLine">The command line.</param>
        [TestMethod]
        [DataRow("-V", DisplayName = "-V")]
        [DataRow("--version", DisplayName = "--version")]
        public void VersionArgument_DisplaysVersionInformation(string commandLine)
        {
            string expectedOutput = HelpTextGenerator.GetVersionInformation();

            HtmlToPdfRunner runner = new HtmlToPdfRunner(HtmlToPdfRunner.HtmlToPdfExe);

            HtmlToPdfRunResult result = runner.Run(commandLine);
            Assert.AreEqual(1, result.ExitCode, result.Output);
            Assert.IsTrue(string.IsNullOrEmpty(result.StandardOutput), result.StandardOutput);
            Assert.IsTrue(result.StandardError.Trim().StartsWith(expectedOutput.Trim()), result.StandardError);
        }
    }
}
