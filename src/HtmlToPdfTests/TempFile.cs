// <copyright file="TempFile.cs" company="HtmlToPdf">
// Copyright (c) HtmlToPdf. All rights reserved.
// </copyright>

namespace HtmlToPdfTests
{
    using System;
    using System.IO;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>
    /// Temporary file
    /// </summary>
    /// <seealso cref="System.IDisposable" />
    public class TempFile : IDisposable
    {
        private readonly TestContext testContext;

        /// <summary>
        /// Initializes a new instance of the <see cref="TempFile" /> class.
        /// </summary>
        /// <param name="fileExtension">The file extension.</param>
        /// <param name="testContext">The test context.</param>
        internal TempFile(string fileExtension, TestContext testContext = null)
        {
            this.testContext = testContext;

            string tempFilePath = Path.GetTempFileName();
            string newFilePath = tempFilePath.Replace(".tmp", fileExtension);

            // make sure file is unique
            while (File.Exists(newFilePath))
            {
                File.Delete(tempFilePath);
                tempFilePath = Path.GetTempFileName();
                newFilePath = tempFilePath.Replace(".tmp", fileExtension);
            }

            File.Move(tempFilePath, newFilePath);
            this.FilePath = newFilePath;
        }

        /// <summary>
        /// Gets the file path.
        /// </summary>
        /// <value>
        /// The file path.
        /// </value>
        public string FilePath { get; private set; }

        /// <summary>
        /// Gets the name of the file.
        /// </summary>
        /// <value>
        /// The name of the file.
        /// </value>
        internal string FileName => Path.GetFileName(this.FilePath);

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public virtual void Dispose()
        {
            if (this.testContext != null)
            {
                string pdfFilePath = Path.Combine(this.testContext.TestResultsDirectory, this.FileName);
                File.Copy(this.FilePath, pdfFilePath, true);
                this.testContext.AddResultFile(pdfFilePath);
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
