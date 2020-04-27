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
        public TempFile(string fileExtension, TestContext testContext)
        {
            this.testContext = testContext;

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
            if (this.testContext != null)
            {
                string pdfFilePath = Path.Combine(Directory.GetCurrentDirectory(), Path.GetFileName(this.FilePath));
                File.Copy(this.FilePath, pdfFilePath, true);
                this.testContext.AddResultFile(pdfFilePath);
            }

            if (File.Exists(this.FilePath))
            {
                File.Delete(this.FilePath);
            }
        }
    }
}
