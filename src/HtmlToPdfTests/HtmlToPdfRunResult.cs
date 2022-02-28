// <copyright file="HtmlToPdfRunResult.cs" company="HtmlToPdf">
// Copyright (c) HtmlToPdf. All rights reserved.
// </copyright>

namespace HtmlToPdfTests
{
    using System;

    /// <summary>
    /// HTML to PDF run result.
    /// </summary>
    public class HtmlToPdfRunResult
    {
        /// <summary>
        /// Gets or sets the exit code.
        /// </summary>
        public int ExitCode { get; set; }

        /// <summary>
        /// Gets or sets the standard output.
        /// </summary>
        public string StandardOutput { get; set; }

        /// <summary>
        /// Gets or sets the standard error.
        /// </summary>
        public string StandardError { get; set; }

        /// <summary>
        /// Gets the output.
        /// </summary>
        public string Output
        {
            get
            {
                string[] output = new[]
                {
                    "Output:",
                    this.StandardOutput,
                    "Error:",
                    this.StandardError,
                };

                return string.Join(Environment.NewLine, output);
            }
        }
    }
}