// <copyright file="PrintToPdfException.cs" company="HtmlToPdf">
// Copyright (c) HtmlToPdf. All rights reserved.
// </copyright>

namespace HtmlToPdf.Exceptions
{
    using System;

    /// <summary>
    /// Print To PDF Exception.
    /// </summary>
    /// <seealso cref="System.Exception" />
    public class PrintToPdfException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PrintToPdfException" /> class.
        /// </summary>
        /// <param name="inputFilePath">The input file path.</param>
        /// <param name="innerException">The exception that is the cause of the current exception, or a null reference (<see langword="Nothing" /> in Visual Basic) if no inner exception is specified.</param>
        public PrintToPdfException(string inputFilePath, Exception innerException)
            : base($"Failed to print HTML file to PDF.{Environment.NewLine}File: \"{inputFilePath}\"", innerException)
        {
        }
    }
}
