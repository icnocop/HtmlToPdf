// <copyright file="Logger.cs" company="HtmlToPdf">
// Copyright (c) HtmlToPdf. All rights reserved.
// </copyright>

namespace HtmlToPdf.Console
{
    using System;

    /// <summary>
    /// Logger
    /// </summary>
    internal class Logger : ILogger
    {
        private readonly LogLevel logLevel;

        /// <summary>
        /// Initializes a new instance of the <see cref="Logger"/> class.
        /// </summary>
        /// <param name="logLevel">The log level.</param>
        public Logger(LogLevel logLevel)
        {
            this.logLevel = logLevel;
        }

        /// <summary>
        /// Logs the message.
        /// </summary>
        /// <param name="message">The message.</param>
        public void LogOutput(string message)
        {
            Console.Out.WriteLine(message);
        }

        /// <summary>
        /// Logs the exception.
        /// </summary>
        /// <param name="ex">The exception.</param>
        public void LogError(Exception ex)
        {
            this.LogError(ex.ToString());
        }

        /// <summary>
        /// Logs the error message.
        /// </summary>
        /// <param name="message">The message.</param>
        public void LogError(string message)
        {
            if (this.logLevel == LogLevel.None)
            {
                return;
            }

            Console.Error.WriteLine(message);
        }

        /// <summary>
        /// Logs the warning message.
        /// </summary>
        /// <param name="message">The message.</param>
        public void LogWarning(string message)
        {
            if ((this.logLevel == LogLevel.None)
                || (this.logLevel == LogLevel.Error))
            {
                return;
            }

            Console.Error.WriteLine($"Warning: {message}");
        }

        /// <summary>
        /// Logs the informational message.
        /// </summary>
        /// <param name="message">The message.</param>
        public void LogInfo(string message)
        {
            if ((this.logLevel != LogLevel.Info)
                && (this.logLevel != LogLevel.Debug))
            {
                return;
            }

            Console.Error.WriteLine(message);
        }

        /// <summary>
        /// Logs the message useful for debugging.
        /// </summary>
        /// <param name="message">The message.</param>
        public void LogDebug(string message)
        {
            if (this.logLevel != LogLevel.Debug)
            {
                return;
            }

            Console.Error.WriteLine(message);
        }
    }
}
