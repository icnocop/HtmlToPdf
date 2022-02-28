// <copyright file="TempDirectory.cs" company="HtmlToPdf">
// Copyright (c) HtmlToPdf. All rights reserved.
// </copyright>

namespace HtmlToPdfTests
{
    using System;
    using System.IO;

    /// <summary>
    /// Temporary Directory.
    /// </summary>
    /// <seealso cref="System.IDisposable" />
    internal class TempDirectory : IDisposable
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TempDirectory"/> class.
        /// </summary>
        public TempDirectory()
        {
            string tempFileName = Path.GetTempFileName();
            string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(tempFileName);
            if (string.IsNullOrEmpty(fileNameWithoutExtension))
            {
                throw new NullReferenceException("Failed to get temporary file name without extension");
            }

            this.DirectoryPath = Path.Combine(Path.GetTempPath(), fileNameWithoutExtension);
            File.Delete(tempFileName);
            Directory.CreateDirectory(this.DirectoryPath);
        }

        /// <summary>
        /// Gets or sets the directory path.
        /// </summary>
        /// <value>
        /// The directory path.
        /// </value>
        public string DirectoryPath { get; internal set; }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            try
            {
                if (Directory.Exists(this.DirectoryPath))
                {
                    Directory.Delete(this.DirectoryPath, true);
                }
            }
            catch
            {
                // ignore
            }
        }
    }
}
