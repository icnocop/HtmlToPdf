// <copyright file="TempHtmlFile.cs" company="HtmlToPdf">
// Copyright (c) HtmlToPdf. All rights reserved.
// </copyright>

namespace HtmlToPdf
{
    /// <summary>
    /// Temporary HTML file
    /// </summary>
    internal class TempHtmlFile : TempFile
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TempHtmlFile"/> class.
        /// </summary>
        public TempHtmlFile()
            : base(".html")
        {
        }
    }
}