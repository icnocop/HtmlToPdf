// <copyright file="Program.cs" company="HtmlToPdf">
// Copyright (c) HtmlToPdf. All rights reserved.
// </copyright>

namespace HtmlToPdf.Console
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Xml;
    using CommandLine;
    using CommandLine.Text;

    /// <summary>
    /// The main program.
    /// </summary>
    public class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        /// <param name="args">The arguments.</param>
        /// <returns>The exit code.</returns>
        public static int Main(string[] args)
        {
            Logger logger = null;

            try
            {
                // System.Diagnostics.Debugger.Launch();
                Console.OutputEncoding = Encoding.Default;

                var parser = new Parser(config =>
                {
                    config.AutoHelp = false;
                    config.AutoVersion = false;
                    config.HelpWriter = null;
                    config.CaseInsensitiveEnumValues = true;
                });

                // create logger
                var parserResult = parser.ParseArguments<CommandLineOptions>(args);
                LogLevel logLevel = parserResult.MapResult(
                    (CommandLineOptions options) =>
                    {
                        return options.LoggerLevel;
                    },
                    error =>
                    {
                        return LogLevel.Info;
                    });

                logger = new Logger(logLevel);

                // do work
                var task = ParseAndRunOptions(parser, args, logger);
                return task.Result;
            }
            catch (Exception ex)
            {
                logger?.LogError(ex);
                return 1;
            }
        }

        private static async Task<int> ParseAndRunOptions(Parser parser, string[] args, Logger logger)
        {
            var parserResult = parser.ParseArguments<CommandLineOptions>(args);
            var task = parserResult.MapResult(
                (CommandLineOptions options) =>
                {
                    if (options.DisplayHelp)
                    {
                        DisplayHelpText(logger);
                        return Error();
                    }

                    if (options.DisplayVersion)
                    {
                        DisplayVersionText(logger);
                        return Error();
                    }

                    try
                    {
                        Task<int> runTask = RunAsync(parser, args, options, logger);
                        runTask.Wait();
                        return runTask;
                    }
                    catch (AggregateException ex)
                    {
                        List<string> errorMessages = new List<string>();

                        foreach (Exception innerException in ex.InnerExceptions)
                        {
                            errorMessages.Add(innerException.ToString());
                        }

                        logger.LogError(string.Join(Environment.NewLine, errorMessages));
                        return Error();
                    }
                },
                errs =>
                {
                    DisplayHelpText(parserResult, logger);
                    return Error();
                });

            return await task;
        }

        private static void DisplayHelpText(Logger logger)
        {
            DisplayText(logger, "--help");
        }

        private static void DisplayVersionText(Logger logger)
        {
            DisplayText(logger, "--version");
        }

        private static void DisplayText(Logger logger, string commandLine)
        {
            var parser = new Parser(config =>
            {
                config.HelpWriter = null;
            });
            var parserResult = parser.ParseArguments<CommandLineOptions>(new[] { commandLine });
            var helpText = parserResult.GetHelpText();
            logger.LogError(helpText.ToString());
        }

        private static void DisplayHelpText<T>(ParserResult<T> parserResult, Logger logger)
        {
            HelpText helpText = parserResult.GetHelpText();
            helpText.AddOptions(parserResult);
            logger.LogError(helpText);
        }

        private static Task<int> Error()
        {
            Task<int> exitTask = new Task<int>(() => 1);
            exitTask.Start();
            return exitTask;
        }

        private static async Task<int> RunAsync(Parser parser, string[] args, CommandLineOptions commandLineOptions, Logger logger)
        {
            if (commandLineOptions.ReadArgsFromStdin)
            {
                logger.LogDebug("Reading arguments from stdin...");

                string stdin = Console.In.ReadToEnd();

                logger.LogDebug($"Arguments from stdin: {stdin}");

                commandLineOptions.ReadArgsFromStdin = false;
                string commandLine = parser.FormatCommandLine(commandLineOptions);

                string newCommandLine = $"{commandLine} {stdin}";
                logger.LogDebug($"Running with arguments: {newCommandLine}");

                string[] newArgs = ParseArguments(newCommandLine);
                return await ParseAndRunOptions(parser, newArgs, logger);
            }

            List<string> inputs = commandLineOptions.Inputs
                .Select(x => x.ToLower().Trim('"').Replace('/', Path.DirectorySeparatorChar))
                .ToList();

            string cover = null;
            if (inputs.Contains("cover"))
            {
                int index = inputs.IndexOf("cover");
                cover = inputs[index + 1];
                inputs.RemoveAt(index); // remove "cover"
            }

            bool addTableOfContents = false;
            if (inputs.Contains("toc"))
            {
                addTableOfContents = true;

                // remove "toc"
                int index = inputs.IndexOf("toc");
                inputs.RemoveAt(index);
            }

            bool stdout = false;
            string outputFilePath = null;

            if ((args.Length > 0) && (args.Last() == "-"))
            {
                stdout = true;
                outputFilePath = Path.GetTempFileName();
            }
            else if (inputs.Count > 0)
            {
                if (inputs.Last() == "-")
                {
                    stdout = true;
                    inputs.RemoveAt(inputs.Count - 1);
                    outputFilePath = Path.GetTempFileName();
                }
                else
                {
                    outputFilePath = inputs.Last();
                    inputs.RemoveAt(inputs.Count - 1);
                }
            }

            ConcurrentBag<HtmlToPdfFile> htmlToPdfFiles = await HtmlToPdfProcessor.ProcessAsync(
                new Options
                {
                    BottomMargin = commandLineOptions.BottomMargin,
                    Cover = cover,
                    Encoding = commandLineOptions.Encoding,
                    FooterCenter = commandLineOptions.FooterCenter,
                    FooterFontName = commandLineOptions.FooterFontName,
                    FooterFontSize = commandLineOptions.FooterFontSize,
                    FooterHtml = commandLineOptions.FooterHtml,
                    FooterLeft = commandLineOptions.FooterLeft,
                    FooterRight = commandLineOptions.FooterRight,
                    Inputs = inputs,
                    JavascriptDelayInMilliseconds = commandLineOptions.JavascriptDelayInMilliseconds,
                    LeftMargin = commandLineOptions.LeftMargin,
                    Orientation = commandLineOptions.Orientation,
                    Outline = !commandLineOptions.NoOutline,
                    OutputFilePath = outputFilePath,
                    PageHeight = commandLineOptions.PageHeight,
                    PageOffset = commandLineOptions.PageOffset,
                    PageSize = commandLineOptions.PageSize,
                    PageWidth = commandLineOptions.PageWidth,
                    PrintBackground = commandLineOptions.PrintBackground,
                    RightMargin = commandLineOptions.RightMargin,
                    Title = commandLineOptions.Title,
                    TopMargin = commandLineOptions.TopMargin,
                    UserStyleSheet = commandLineOptions.UserStyleSheet,
                    DumpDefaultTocXsl = commandLineOptions.DumpDefaultTocXsl,
                    AddTableOfContents = addTableOfContents,
                    OutlineBuilder = new Action<XmlWriter, IReadOnlyCollection<HtmlToPdfFile>, bool>(PdfOutlineBuilder.BuildOutline),
                    DefaultTableOfContentsStyleSheetBuilder = TableOfContentsStyleSheetBuilder.Build,
                    OutputDottedLinesInTableOfContents = !commandLineOptions.DisableDottedLines,
                },
                logger);

            if (stdout)
            {
                byte[] outputPdfFileBytes = File.ReadAllBytes(outputFilePath);
                Console.Write(Encoding.Default.GetString(outputPdfFileBytes));
                File.Delete(outputFilePath);
            }

            if (!string.IsNullOrEmpty(commandLineOptions.DumpOutline))
            {
                // dump outline
                using (XmlTextWriter xmlWriter = new XmlTextWriter(commandLineOptions.DumpOutline, new CustomUTF8Encoding()))
                {
                    xmlWriter.Formatting = Formatting.Indented;

                    PdfOutlineBuilder.BuildOutline(xmlWriter, htmlToPdfFiles, addTableOfContents);
                }
            }

            return 0;
        }

        private static string[] ParseArguments(string commandLine)
        {
            char[] commandLineAsChars = commandLine.ToCharArray();
            bool inQuote = false;
            for (int index = 0; index < commandLineAsChars.Length; index++)
            {
                if (commandLineAsChars[index] == '"')
                {
                    inQuote = !inQuote;
                }

                if (!inQuote && commandLineAsChars[index] == ' ')
                {
                    commandLineAsChars[index] = '\n';
                }
            }

            return new string(commandLineAsChars).Split('\n');
        }
    }
}
