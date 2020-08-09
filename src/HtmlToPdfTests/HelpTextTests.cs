// <copyright file="HelpTextTests.cs" company="HtmlToPdf">
// Copyright (c) HtmlToPdf. All rights reserved.
// </copyright>

namespace HtmlToPdfTests
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>
    /// Help text tests
    /// </summary>
    [TestClass]
    public class HelpTextTests
    {
        /// <summary>
        /// Asserts that not passing any arguments displays an error and help text.
        /// </summary>
        [TestMethod]
        public void NoArguments_DisplaysErrorAndHelpText()
        {
            string expectedOutput = HelpTextGenerator.GenerateParserError(
                "A required value not bound to option name is missing.",
                "System.ApplicationException: At least one input must be specified.");

            HtmlToPdfRunner runner = new HtmlToPdfRunner(HtmlToPdfRunner.HtmlToPdfExe);

            HtmlToPdfRunResult result = runner.Run("--log-level Error");
            Assert.AreEqual(1, result.ExitCode, result.Output);
            Assert.IsTrue(string.IsNullOrEmpty(result.StandardOutput), result.StandardOutput);
            int length = expectedOutput.Trim().Length;
            StringAssertWithDiff.AreEqual(expectedOutput.Trim(), result.StandardError.Trim().Substring(0, length));
        }

        /// <summary>
        /// Asserts that passing the help command line option displays the help text.
        /// </summary>
        /// <param name="commandLine">The command line.</param>
        [TestMethod]
        [DataRow("-h", DisplayName = "-h")]
        [DataRow("--help", DisplayName = "--help")]
        public void HelpArgument_DisplaysHelpText(string commandLine)
        {
            string expectedOutput = HelpTextGenerator.Generate();

            HtmlToPdfRunner runner = new HtmlToPdfRunner(HtmlToPdfRunner.HtmlToPdfExe);

            HtmlToPdfRunResult result = runner.Run(commandLine);
            Assert.AreEqual(1, result.ExitCode, result.Output);
            Assert.IsTrue(string.IsNullOrEmpty(result.StandardOutput), result.StandardOutput);
            StringAssertWithDiff.AreEqual(expectedOutput.Trim(), result.StandardError.Trim());
        }
    }
}
