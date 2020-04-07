// <copyright file="TempPdfFile.cs" company="HtmlToPdf">
// Copyright (c) HtmlToPdf. All rights reserved.
// </copyright>

namespace HtmlToPdfTests
{
    using System;
    using System.IO;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>
    /// Temporary PDF file.
    /// </summary>
    /// <seealso cref="HtmlToPdfTests.TempFile" />
    /// <seealso cref="System.IDisposable" />
    internal class TempPdfFile : TempFile, IDisposable
    {
        private readonly TestContext testContext;

        /// <summary>
        /// Initializes a new instance of the <see cref="TempPdfFile"/> class.
        /// </summary>
        /// <param name="testContext">The test context.</param>
        public TempPdfFile(TestContext testContext)
            : base(".pdf")
        {
            this.testContext = testContext;
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public override void Dispose()
        {
            string pdfFilePath = Path.Combine(Directory.GetCurrentDirectory(), Path.GetFileName(this.FilePath));
            File.Copy(this.FilePath, pdfFilePath, true);
            this.testContext.AddResultFile(pdfFilePath);

            base.Dispose();
        }
    }
}