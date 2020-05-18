// <copyright file="HelpTextGenerator.cs" company="HtmlToPdf">
// Copyright (c) HtmlToPdf. All rights reserved.
// </copyright>

namespace HtmlToPdfTests
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Reflection;

    /// <summary>
    /// Help Text Generator
    /// </summary>
    internal class HelpTextGenerator
    {
        /// <summary>
        /// Generates the help text.
        /// </summary>
        /// <param name="errorMessage">The error message.</param>
        /// <param name="detailedErrorMessage">The detailed error message.</param>
        /// <returns>
        /// The help text.
        /// </returns>
        public static string Generate(string errorMessage = null, string detailedErrorMessage = null)
        {
            Assembly currentAssembly = Assembly.GetExecutingAssembly();
            string version = FileVersionInfo.GetVersionInfo(currentAssembly.Location).FileVersion;
            string usage = File.ReadAllText(@"..\..\..\..\USAGE.md");

            string output = $@"HtmlToPdf {version}
Copyright © 2020
{(errorMessage == null ? string.Empty : $"{Environment.NewLine}ERROR(S):{Environment.NewLine}  {errorMessage}{Environment.NewLine}")}
{usage}{(detailedErrorMessage == null ? string.Empty : $"{Environment.NewLine}{Environment.NewLine}{Environment.NewLine}{detailedErrorMessage}")}";

            return output;
        }
    }
}
