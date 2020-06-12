// <copyright file="TempCopyHtmlFile.cs" company="HtmlToPdf">
// Copyright (c) HtmlToPdf. All rights reserved.
// </copyright>

namespace HtmlToPdf
{
    using System;
    using System.IO;

    /// <summary>
    /// Temporary Copy HTML File
    /// </summary>
    /// <seealso cref="System.IDisposable" />
    internal class TempCopyHtmlFile : IDisposable
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TempCopyHtmlFile"/> class.
        /// </summary>
        /// <param name="originalFilePath">The original file path.</param>
        public TempCopyHtmlFile(string originalFilePath)
        {
            // create temporary file in the same path as the original file so relative paths are resolved correctly
            string originalPath = Path.GetDirectoryName(originalFilePath);
            if (string.IsNullOrEmpty(originalPath))
            {
                throw new Exception($"Failed to get directory name for path '{originalFilePath}'.");
            }

            string tempFilePath = Path.GetTempFileName();
            string newFilePath = Path.Combine(originalPath, $"{Path.GetFileNameWithoutExtension(tempFilePath)}.html");

            // make sure file is unique
            while (File.Exists(newFilePath))
            {
                File.Delete(tempFilePath);
                tempFilePath = Path.GetTempFileName();
                newFilePath = Path.Combine(originalPath, $"{Path.GetFileNameWithoutExtension(tempFilePath)}.html");
            }

            File.Move(tempFilePath, newFilePath);
            File.Copy(originalFilePath, newFilePath, true);
            this.FilePath = newFilePath;
        }

        /// <summary>
        /// Gets the file path.
        /// </summary>
        /// <value>
        /// The file path.
        /// </value>
        public string FilePath { get; }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            if (File.Exists(this.FilePath))
            {
                File.Delete(this.FilePath);
            }
        }
    }
}
