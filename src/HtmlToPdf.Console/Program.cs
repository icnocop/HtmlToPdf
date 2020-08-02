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
    using System.Xml.Serialization;
    using CommandLine;
    using CommandLine.Text;
    using HtmlToPdf.Console.Outline.Xml;

    /// <summary>
    /// The main program
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
                System.Diagnostics.Debugger.Launch();
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

                        logger.LogError(parserResult.GetAutoBuildHelpText());
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
            var helpText = HelpText.AutoBuild(parserResult, h =>
            {
                h.AutoHelp = false;
                h.AutoVersion = false;
                return h;
            });
            logger.LogError(helpText.ToString());
        }

        private static void DisplayHelpText<T>(ParserResult<T> parserResult, Logger logger)
        {
            HelpText helpText = HelpText.AutoBuild(
                parserResult,
                h =>
                {
                    h.AutoHelp = false;
                    h.AutoVersion = false;
                    return h;
                });
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
                    DumpDefaultTocXsl = commandLineOptions.DumpDefaultTocXsl
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

                    outline outline = new outline
                    {
                        Items = BuildOutline(htmlToPdfFiles.SelectMany(x => x.TitleAndHeadings))
                    };

                    var serializer = new XmlSerializer(typeof(outline));

                    XmlSerializerNamespaces namespaces = new XmlSerializerNamespaces();
                    namespaces.Add(string.Empty, "http://wkhtmltopdf.org/outline");

                    serializer.Serialize(xmlWriter, outline, namespaces);
                }
            }

            return 0;
        }

        private static List<item> BuildOutline(IEnumerable<HtmlHeading> headings)
        {
            int counter = 0;

            var items = new List<item>();
            var allItems = new List<item>();
            item parentNode = null;
            item currentItem = null;

            foreach (var heading in headings)
            {
                // if there is no previous item, add it to the current list
                // if previously added item is the same level, add it to current list
                // if previously added item is a smaller level, add it to the previous item's children
                // if previously added item is a larger level, add it to the previously added item's parent
                currentItem = new item
                {
                    title = heading.Text,
                    level = heading.Level,
                    link = $"__WKANCHOR_{IntToBase(counter++, 36)}",
                    backLink = $"__WKANCHOR_{IntToBase(counter++, 36)}",
                    page = heading.Page.ToString(),
                    children = new List<item>()
                };

                allItems.Add(currentItem);

                if (!allItems.Any(n => n.level < currentItem.level))
                {
                    items.Add(currentItem);
                    parentNode = null;
                }

                if (parentNode != null)
                {
                    if (parentNode.level >= currentItem.level)
                    {
                        parentNode = allItems.Last(n => n.level < currentItem.level);
                    }

                    // add item as child of parent
                    parentNode.children.Add(currentItem);
                }

                parentNode = currentItem;
            }

            return items;
        }

        private static string IntToBase(int input, int @base)
        {
            var digits = "0123456789abcdefghijklmnopqrstuvwxyz";

            if (@base < 2 || @base > 36)
            {
                throw new ArgumentOutOfRangeException(nameof(@base), "Must specify a base between 2 and 36, inclusive.");
            }

            if (input < @base && input >= 0)
            {
                return digits[input].ToString();
            }
            else
            {
                return IntToBase(input / @base, @base) + digits[input % @base].ToString();
            }
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
