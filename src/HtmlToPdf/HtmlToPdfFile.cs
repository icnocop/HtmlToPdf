// <copyright file="HtmlToPdfFile.cs" company="HtmlToPdf">
// Copyright (c) HtmlToPdf. All rights reserved.
// </copyright>

namespace HtmlToPdf
{
    /// <summary>
    /// HTML to PDF file
    /// </summary>
    internal class HtmlToPdfFile
    {
        /// <summary>
        /// Gets or sets the input.
        /// </summary>
        public string Input { get; set; }

        /// <summary>
        /// Gets or sets the index.
        /// </summary>
        public int Index { get; set; }

        /// <summary>
        /// Gets or sets the PDF file path.
        /// </summary>
        public string PdfFilePath { get; set; }

        /// <summary>
        /// Gets or sets the number of pages.
        /// </summary>
        public int NumberOfPages { get; set; }

        /// <summary>
        /// Gets or sets the output PDF file page number.
        /// </summary>
        public int OutputPdfFilePageNumber { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to print the footer.
        /// </summary>
        public bool PrintFooter { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to skip printing this file.
        /// </summary>
        public bool Skip { get; set; }
    }
}
