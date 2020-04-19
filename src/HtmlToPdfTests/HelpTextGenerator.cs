// <copyright file="HelpTextGenerator.cs" company="HtmlToPdf">
// Copyright (c) HtmlToPdf. All rights reserved.
// </copyright>

namespace HtmlToPdfTests
{
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
        /// <returns>
        /// The help text.
        /// </returns>
        public static string Generate(string errorMessage)
        {
            Assembly currentAssembly = Assembly.GetExecutingAssembly();
            string version = FileVersionInfo.GetVersionInfo(currentAssembly.Location).FileVersion;
            string usage = File.ReadAllText(@"..\..\..\..\USAGE.md");

            string output = $@"HtmlToPdf {version}
Copyright © 2020

ERROR(S):
  A required value not bound to option name is missing.

{usage}


{errorMessage}";

            return output;
        }
    }
}
