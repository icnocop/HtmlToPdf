// <copyright file="Logger.cs" company="HtmlToPdf">
// Copyright (c) HtmlToPdf. All rights reserved.
// </copyright>

namespace HtmlToPdf
{
    using System;

    /// <summary>
    /// Logger
    /// </summary>
    internal static class Logger
    {
        /// <summary>
        /// Logs the error.
        /// </summary>
        /// <param name="ex">The exception.</param>
        internal static void LogError(Exception ex)
        {
            LogError(ex.ToString());
        }

        /// <summary>
        /// Logs the error.
        /// </summary>
        /// <param name="message">The message.</param>
        internal static void LogError(string message)
        {
            Console.Error.WriteLine(message);
        }
    }
}
