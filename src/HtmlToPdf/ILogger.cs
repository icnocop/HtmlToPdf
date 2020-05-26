// <copyright file="ILogger.cs" company="HtmlToPdf">
// Copyright (c) HtmlToPdf. All rights reserved.
// </copyright>

namespace HtmlToPdf
{
    using System;

    /// <summary>
    /// Logger
    /// </summary>
    public interface ILogger
    {
        /// <summary>
        /// Logs the exception.
        /// </summary>
        /// <param name="ex">The exception.</param>
        void LogError(Exception ex);

        /// <summary>
        /// Logs the error message.
        /// </summary>
        /// <param name="message">The message.</param>
        void LogError(string message);

        /// <summary>
        /// Logs the warning message.
        /// </summary>
        /// <param name="message">The message.</param>
        void LogWarning(string message);

        /// <summary>
        /// Logs the informational message.
        /// </summary>
        /// <param name="message">The message.</param>
        void LogInfo(string message);

        /// <summary>
        /// Logs the message useful for debugging.
        /// </summary>
        /// <param name="message">The message.</param>
        void LogDebug(string message);
    }
}
