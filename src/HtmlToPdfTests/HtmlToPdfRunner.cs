// <copyright file="HtmlToPdfRunner.cs" company="HtmlToPdf">
// Copyright (c) HtmlToPdf. All rights reserved.
// </copyright>

namespace HtmlToPdfTests
{
    using System;
    using System.Diagnostics;
    using System.Text;
    using System.Threading;

    /// <summary>
    /// HTML to PDF runner.
    /// </summary>
    public class HtmlToPdfRunner
    {
        /// <summary>
        /// The wkhtmltopdf executable.
        /// </summary>
        public const string WkhtmltopdfExe = "wkhtmltopdf.exe";

        /// <summary>
        /// The HTML to PDF executable.
        /// </summary>
        public const string HtmlToPdfExe = "HtmlToPdf.Console.exe";

        private readonly string exeFileName;

        /// <summary>
        /// Initializes a new instance of the <see cref="HtmlToPdfRunner"/> class.
        /// </summary>
        /// <param name="exeFileName">Name of the executable file.</param>
        public HtmlToPdfRunner(string exeFileName)
        {
            this.exeFileName = exeFileName;
        }

        /// <summary>
        /// Runs the specified command line.
        /// </summary>
        /// <param name="commandLine">The command line.</param>
        /// <param name="stdIn">The standard input.</param>
        /// <returns>The result of the run.</returns>
        public HtmlToPdfRunResult Run(string commandLine, string stdIn = null)
        {
            HtmlToPdfRunResult runResult = new HtmlToPdfRunResult();

            using (var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    UseShellExecute = false,
                    RedirectStandardInput = stdIn != null,
                    RedirectStandardOutput = true,
                    StandardOutputEncoding = Encoding.Default,
                    RedirectStandardError = true,
                    StandardErrorEncoding = Encoding.Default,
                    WindowStyle = ProcessWindowStyle.Hidden,
                    FileName = this.exeFileName,
                    Arguments = commandLine,
                },
            })
            {
                StringBuilder output = new StringBuilder();
                StringBuilder error = new StringBuilder();

                Action<string, AutoResetEvent, StringBuilder> dataReceivedHandler = (data, autoResetEvent, stringBuilder) =>
                {
                    if (data == null)
                    {
                        autoResetEvent.Set();
                    }
                    else
                    {
                        stringBuilder.AppendLine(data);
                    }
                };

                using (AutoResetEvent errorWaitHandle = new AutoResetEvent(false))
                {
                    process.ErrorDataReceived += (sender, e) => dataReceivedHandler(e.Data, errorWaitHandle, error);

                    process.Start();

                    if (stdIn != null)
                    {
                        using (var standardInput = process.StandardInput)
                        {
                            standardInput.AutoFlush = true;
                            standardInput.Write(stdIn);
                        }
                    }

                    process.BeginErrorReadLine();

                    runResult.StandardOutput = process.StandardOutput.ReadToEnd();
                    process.WaitForExit();

                    // process completed
                    runResult.ExitCode = process.ExitCode;
                    runResult.StandardError = error.ToString();
                }
            }

            Console.WriteLine(runResult.Output);
            return runResult;
        }
    }
}
