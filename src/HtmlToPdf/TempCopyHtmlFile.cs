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
            string tempFilePath = Path.GetTempFileName();
            string newFilePath = tempFilePath.Replace(".tmp", ".html");

            // make sure file is unique
            while (File.Exists(newFilePath))
            {
                File.Delete(tempFilePath);
                tempFilePath = Path.GetTempFileName();
                newFilePath = tempFilePath.Replace(".tmp", ".html");
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
