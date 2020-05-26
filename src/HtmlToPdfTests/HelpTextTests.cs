// <copyright file="HelpTextTests.cs" company="HtmlToPdf">
// Copyright (c) HtmlToPdf. All rights reserved.
// </copyright>

namespace HtmlToPdfTests
{
    using System.Diagnostics;
    using System.IO;
    using System.Reflection;
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
            string expectedOutput = HelpTextGenerator.Generate(
                "A required value not bound to option name is missing.",
                "System.ApplicationException: At least one input must be specified.");

            HtmlToPdfRunner runner = new HtmlToPdfRunner(HtmlToPdfRunner.HtmlToPdfExe);

            HtmlToPdfRunResult result = runner.Run("--log-level Error");
            Assert.AreEqual(1, result.ExitCode, result.Output);
            Assert.IsTrue(string.IsNullOrEmpty(result.StandardOutput), result.StandardOutput);
            Assert.IsTrue(result.StandardError.Trim().StartsWith(expectedOutput.Trim()), result.StandardError);
        }

        /// <summary>
        /// Asserts that passing the "--help" argument displays the help text.
        /// </summary>
        [TestMethod]
        public void HelpArgument_DisplaysHelpText()
        {
            string expectedOutput = HelpTextGenerator.Generate();

            HtmlToPdfRunner runner = new HtmlToPdfRunner(HtmlToPdfRunner.HtmlToPdfExe);

            HtmlToPdfRunResult result = runner.Run("--help");
            Assert.AreEqual(1, result.ExitCode, result.Output);
            Assert.IsTrue(string.IsNullOrEmpty(result.StandardOutput), result.StandardOutput);
            Assert.IsTrue(result.StandardError.Trim().StartsWith(expectedOutput.Trim()), result.StandardError);
        }
    }
}
