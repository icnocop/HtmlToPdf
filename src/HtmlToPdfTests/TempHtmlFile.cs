// <copyright file="TempHtmlFile.cs" company="HtmlToPdf">
// Copyright (c) HtmlToPdf. All rights reserved.
// </copyright>

namespace HtmlToPdfTests
{
    using System.IO;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>
    /// Temporary HTML file
    /// </summary>
    /// <seealso cref="HtmlToPdfTests.TempFile" />
    public class TempHtmlFile : TempFile
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TempHtmlFile" /> class.
        /// </summary>
        /// <param name="html">The HTML.</param>
        /// <param name="testContext">The test context.</param>
        public TempHtmlFile(string html = null, TestContext testContext = null)
            : base(".html", testContext)
        {
            if (html != null)
            {
                File.WriteAllText(this.FilePath, html);
            }
        }
    }
}
