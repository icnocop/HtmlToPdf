// <copyright file="TempPdfFile.cs" company="HtmlToPdf">
// Copyright (c) HtmlToPdf. All rights reserved.
// </copyright>

namespace HtmlToPdf
{
    /// <summary>
    /// Temporary PDF file.
    /// </summary>
    internal class TempPdfFile
    {
        /// <summary>
        /// Creates a temporary PDF file.
        /// </summary>
        /// <returns>The file path.</returns>
        internal static string Create()
        {
            TempFile tempFile = new TempFile(".pdf")
            {
                DeleteFileOnDispose = false,
            };
            return tempFile.FilePath;
        }
    }
}