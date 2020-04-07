// <copyright file="TempHtmlFile.cs" company="HtmlToPdf">
// Copyright (c) HtmlToPdf. All rights reserved.
// </copyright>

namespace HtmlToPdfTests
{
    using System.IO;

    /// <summary>
    /// Temporary HTML file
    /// </summary>
    /// <seealso cref="HtmlToPdfTests.TempFile" />
    public class TempHtmlFile : TempFile
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TempHtmlFile"/> class.
        /// </summary>
        /// <param name="html">The HTML.</param>
        public TempHtmlFile(string html = null)
            : base(".html")
        {
            if (html != null)
            {
                File.WriteAllText(this.FilePath, html);
            }
        }
    }
}
