// <copyright file="TempFile.cs" company="HtmlToPdf">
// Copyright (c) HtmlToPdf. All rights reserved.
// </copyright>

namespace HtmlToPdf
{
    using System;
    using System.IO;

    /// <summary>
    /// Temporary file.
    /// </summary>
    public class TempFile : IDisposable
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TempFile" /> class.
        /// </summary>
        /// <param name="fileExtension">The file extension.</param>
        public TempFile(string fileExtension)
            : this()
        {
            string tempFilePath = Path.GetTempFileName();
            string newFilePath = tempFilePath.Replace(".tmp", fileExtension);

            // make sure file is unique
            while (File.Exists(newFilePath))
            {
                // create a new file
                File.Delete(tempFilePath);
                tempFilePath = Path.GetTempFileName();
                newFilePath = tempFilePath.Replace(".tmp", fileExtension);
            }

            File.Move(tempFilePath, newFilePath);
            this.FilePath = newFilePath;
        }

        private TempFile()
        {
            this.DeleteFileOnDispose = true;
        }

        /// <summary>
        /// Gets the name of the file.
        /// </summary>
        /// <value>
        /// The name of the file.
        /// </value>
        public string FileName => Path.GetFileName(this.FilePath);

        /// <summary>
        /// Gets the file path.
        /// </summary>
        /// <value>
        /// The file path.
        /// </value>
        public string FilePath { get; private set; }

        /// <summary>
        /// Gets or sets a value indicating whether the file is deleted on dispose.
        /// </summary>
        public bool DeleteFileOnDispose { get; set; }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public virtual void Dispose()
        {
            if (!this.DeleteFileOnDispose)
            {
                return;
            }

            if (File.Exists(this.FilePath))
            {
                File.Delete(this.FilePath);
            }
        }

        /// <summary>
        /// Moves the file to the specified path.
        /// </summary>
        /// <param name="path">The path.</param>
        public void MoveToDirectory(string path)
        {
            string newFilePath = Path.Combine(path, this.FileName);

            File.Move(this.FilePath, newFilePath);
            this.FilePath = newFilePath;
        }
    }
}