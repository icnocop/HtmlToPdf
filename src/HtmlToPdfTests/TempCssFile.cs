// <copyright file="TempCssFile.cs" company="HtmlToPdf">
// Copyright (c) HtmlToPdf. All rights reserved.
// </copyright>

namespace HtmlToPdfTests
{
    using System.IO;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>
    /// Temporary CSS file.
    /// </summary>
    /// <seealso cref="HtmlToPdfTests.TempFile" />
    public class TempCssFile : TempFile
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TempCssFile" /> class.
        /// </summary>
        /// <param name="css">The CSS.</param>
        /// <param name="testContext">The test context.</param>
        public TempCssFile(string css = null, TestContext testContext = null)
            : base(".css", testContext)
        {
            if (css != null)
            {
                File.WriteAllText(this.FilePath, css);
            }
        }
    }
}
