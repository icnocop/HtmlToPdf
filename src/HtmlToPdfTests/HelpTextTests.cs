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
                "System.ApplicationException: At least one input and one output must be specified.");

            HtmlToPdfRunner runner = new HtmlToPdfRunner();

            HtmlToPdfRunResult result = runner.Run(string.Empty);
            Assert.AreEqual(1, result.ExitCode, result.Output);
            Assert.IsTrue(string.IsNullOrEmpty(result.StandardOutput), result.StandardOutput);
            Assert.IsTrue(result.StandardError.Trim().StartsWith(expectedOutput), result.StandardError);
        }
    }
}
