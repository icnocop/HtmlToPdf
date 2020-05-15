// <copyright file="TempPdfFile.cs" company="HtmlToPdf">
// Copyright (c) HtmlToPdf. All rights reserved.
// </copyright>

namespace HtmlToPdf
{
    using System.IO;

    /// <summary>
    /// Temporary PDF file
    /// </summary>
    internal class TempPdfFile
    {
        /// <summary>
        /// Creates a temporary PDF file.
        /// </summary>
        /// <returns>The file path.</returns>
        internal static string Create()
        {
            string tempFilePath = Path.GetTempFileName();
            string newFilePath = tempFilePath.Replace(".tmp", ".pdf");

            // make sure file is unique
            while (File.Exists(newFilePath))
            {
                // create a new file
                File.Delete(tempFilePath);
                tempFilePath = Path.GetTempFileName();
                newFilePath = tempFilePath.Replace(".tmp", ".pdf");
            }

            File.Move(tempFilePath, newFilePath);
            return newFilePath;
        }
    }
}