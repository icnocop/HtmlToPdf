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
    public class TempFile : HtmlToPdf.TempFile, IDisposable
    {
        private readonly TestContext testContext;

        /// <summary>
        /// Initializes a new instance of the <see cref="TempFile" /> class.
        /// </summary>
        /// <param name="fileExtension">The file extension.</param>
        /// <param name="testContext">The test context.</param>
        internal TempFile(string fileExtension, TestContext testContext = null)
            : base(fileExtension)
        {
            this.testContext = testContext;
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public new virtual void Dispose()
        {
            if (this.testContext != null)
            {
                string pdfFilePath = Path.Combine(this.testContext.TestResultsDirectory, this.FileName);
                File.Copy(this.FilePath, pdfFilePath, true);
                this.testContext.AddResultFile(pdfFilePath);
            }

            base.Dispose();
        }
    }
}
