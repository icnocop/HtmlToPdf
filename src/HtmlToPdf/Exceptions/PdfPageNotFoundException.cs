// <copyright file="PdfPageNotFoundException.cs" company="HtmlToPdf">
// Copyright (c) HtmlToPdf. All rights reserved.
// </copyright>

namespace HtmlToPdf.Exceptions
{
    using System;

    /// <summary>
    /// PDF page not found exception.
    /// </summary>
    /// <seealso cref="System.Exception" />
    public class PdfPageNotFoundException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PdfPageNotFoundException"/> class.
        /// </summary>
        /// <param name="pageNumber">The page number.</param>
        /// <param name="htmlFile">The HTML file.</param>
        /// <param name="innerException">The inner exception.</param>
        public PdfPageNotFoundException(int pageNumber, string htmlFile, Exception innerException)
            : base($"Failed to get page {pageNumber} for HTML file '{htmlFile}'.", innerException)
        {
        }
    }
}
