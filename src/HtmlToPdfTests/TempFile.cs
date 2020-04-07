// <copyright file="TempFile.cs" company="HtmlToPdf">
// Copyright (c) HtmlToPdf. All rights reserved.
// </copyright>

namespace HtmlToPdfTests
{
    using System;
    using System.IO;

    /// <summary>
    /// Temporary file
    /// </summary>
    /// <seealso cref="System.IDisposable" />
    public class TempFile : IDisposable
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TempFile"/> class.
        /// </summary>
        /// <param name="fileExtension">The file extension.</param>
        public TempFile(string fileExtension)
        {
            string tempFilePath = Path.GetTempFileName();
            string newFilePath = tempFilePath.Replace(".tmp", fileExtension);
            File.Move(tempFilePath, newFilePath);
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
        public virtual void Dispose()
        {
            if (File.Exists(this.FilePath))
            {
                File.Delete(this.FilePath);
            }
        }
    }
}
