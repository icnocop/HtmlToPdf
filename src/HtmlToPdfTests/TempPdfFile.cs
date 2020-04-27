// <copyright file="TempPdfFile.cs" company="HtmlToPdf">
// Copyright (c) HtmlToPdf. All rights reserved.
// </copyright>

namespace HtmlToPdfTests
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>
    /// Temporary PDF file.
    /// </summary>
    /// <seealso cref="HtmlToPdfTests.TempFile" />
    internal class TempPdfFile : TempFile
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TempPdfFile"/> class.
        /// </summary>
        /// <param name="testContext">The test context.</param>
        public TempPdfFile(TestContext testContext)
            : base(".pdf", testContext)
        {
        }
    }
}