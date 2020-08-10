// <copyright file="HelpTextGenerator.cs" company="HtmlToPdf">
// Copyright (c) HtmlToPdf. All rights reserved.
// </copyright>

namespace HtmlToPdfTests
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Reflection;

    /// <summary>
    /// Help Text Generator
    /// </summary>
    internal class HelpTextGenerator
    {
        /// <summary>
        /// Generates the help text.
        /// </summary>
        /// <param name="detailedErrorMessage">The detailed error message.</param>
        /// <returns>
        /// The help text.
        /// </returns>
        public static string Generate(string detailedErrorMessage = null)
        {
            return GenerateParserError(null, detailedErrorMessage);
        }

        /// <summary>
        /// Gets the version information.
        /// </summary>
        /// <returns>The version information.</returns>
        public static string GetVersionInformation()
        {
            Assembly currentAssembly = Assembly.GetExecutingAssembly();
            string version = FileVersionInfo.GetVersionInfo(currentAssembly.Location).FileVersion;
            return $@"HtmlToPdf.Console {version}";
        }

        /// <summary>
        /// Generates the help text.
        /// </summary>
        /// <param name="errorMessage">The error message.</param>
        /// <param name="detailedErrorMessage">The detailed error message.</param>
        /// <returns>
        /// The help text.
        /// </returns>
        public static string GenerateParserError(string errorMessage = null, string detailedErrorMessage = null)
        {
            string usage = string.Join(Environment.NewLine, File.ReadLines(@"..\..\..\..\USAGE.md").Select(x => x.TrimEnd()));

            string output = $@"{GetVersionInformation()}
Copyright © 2020
{(errorMessage == null ? null : $"{Environment.NewLine}ERROR(S):{Environment.NewLine}  {errorMessage}{Environment.NewLine}{Environment.NewLine}")}{usage}{(detailedErrorMessage == null ? string.Empty : $"{Environment.NewLine}{detailedErrorMessage}")}";

            return output;
        }
    }
}
