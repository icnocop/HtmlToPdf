// <copyright file="UpdatePdfLinksException.cs" company="HtmlToPdf">
// Copyright (c) HtmlToPdf. All rights reserved.
// </copyright>

namespace HtmlToPdf.Exceptions
{
    using System;
    using System.Collections.Concurrent;
    using System.IO;
    using System.Linq;
    using System.Text;
    using TheCodingMonkey.Serialization;

    /// <summary>
    /// Update PDF links exception
    /// </summary>
    /// <seealso cref="System.Exception" />
    public class UpdatePdfLinksException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UpdatePdfLinksException"/> class.
        /// </summary>
        /// <param name="outputFilePath">The output file path.</param>
        /// <param name="htmlToPdfFiles">The HTML to PDF files.</param>
        /// <param name="innerException">The inner exception.</param>
        public UpdatePdfLinksException(string outputFilePath, ConcurrentBag<HtmlToPdfFile> htmlToPdfFiles, Exception innerException)
            : base(BuildErrorMessage(outputFilePath, htmlToPdfFiles), innerException)
        {
        }

        private static string BuildErrorMessage(string outputFilePath, ConcurrentBag<HtmlToPdfFile> htmlToPdfFiles)
        {
            StringBuilder errorMessage = new StringBuilder($"Failed to update links in output file '{outputFilePath}'.");
            var serializer = new CsvSerializer<HtmlToPdfFile>(config => config.ByConvention());

            using (TextWriter textWriter = new StringWriter(errorMessage))
            {
                serializer.SerializeArray(
                    textWriter,
                    htmlToPdfFiles.OrderBy(x => x.OutputPdfFilePageNumber).ToArray(),
                    true);
            }

            return errorMessage.ToString();
        }
    }
}
