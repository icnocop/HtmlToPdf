// <copyright file="Program.cs" company="HtmlToPdf">
// Copyright (c) HtmlToPdf. All rights reserved.
// </copyright>

namespace HtmlToPdf
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using CommandLine;
    using CommandLine.Text;
    using PuppeteerSharp;
    using PuppeteerSharp.Media;

    /// <summary>
    /// The main program
    /// </summary>
    public class Program
    {
        private static bool coverAdded = false;

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

        private static void DisplayHelpText<T>(ParserResult<T> parserResult, Logger logger)
        {
            HelpText helpText = HelpText.AutoBuild(parserResult);
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

            Encoding encoding = Encoding.Default;
            if (!string.IsNullOrEmpty(commandLineOptions.Encoding))
            {
                encoding = Encoding.GetEncoding(commandLineOptions.Encoding);
            }

            List<string> inputs = commandLineOptions.Inputs
                .Select(x => x.ToLower().Trim('"').Replace('/', Path.DirectorySeparatorChar))
                .ToList();

            if ((args.Length > 0) && (args.Last() == "-"))
            {
                inputs.Add("-");
            }

            if (inputs.Count < 2)
            {
                throw new ApplicationException($"At least one input and one output must be specified.");
            }

            ConcurrentBag<HtmlToPdfFile> htmlToPdfFiles = new ConcurrentBag<HtmlToPdfFile>();

            foreach (string input in inputs)
            {
                logger.LogDebug(input);
            }

            bool stdout = false;
            string outputFilePath = null;

            if (inputs.Last() == "-")
            {
                stdout = true;
            }
            else
            {
                outputFilePath = inputs.Last();
            }

            inputs.RemoveAt(inputs.Count - 1);

            commandLineOptions.UserStyleSheet = commandLineOptions.UserStyleSheet?.Trim('"');

            BrowserDownloader.DownloadBrowser(logger);

            using (Browser browser = await Puppeteer.LaunchAsync(new LaunchOptions
            {
                SlowMo = 0,
                Headless = true,
                Timeout = 0,
                LogProcess = false,
                EnqueueTransportMessages = true,
                Devtools = false
            }))
            {
                MarginOptions marginOptions = new MarginOptions
                {
                    Bottom = commandLineOptions.BottomMargin,
                    Left = commandLineOptions.LeftMargin,
                    Right = commandLineOptions.RightMargin,
                    Top = commandLineOptions.TopMargin
                };

                try
                {
                    PdfPrinter pdfPrinter = new PdfPrinter(browser);

                    // cover options
                    HtmlToPdfOptions htmlToPdfOptions = new HtmlToPdfOptions
                    {
                        StyleSheet = commandLineOptions.UserStyleSheet,
                        JavascriptDelayInMilliseconds = commandLineOptions.JavascriptDelayInMilliseconds,
                        Landscape = commandLineOptions.Landscape,
                        PaperFormat = commandLineOptions.PaperFormat,
                        Height = commandLineOptions.PageHeight,
                        Width = commandLineOptions.PageWidth,
                        PrintBackground = commandLineOptions.PrintBackground
                    };

                    if (inputs.Contains("cover") && (!coverAdded))
                    {
                        int index = inputs.IndexOf("cover");
                        string cover = inputs[index + 1];
                        inputs.RemoveAt(index); // remove "cover"

                        string pdfFile = await pdfPrinter.PrintAsPdfAsync(
                            cover,
                            htmlToPdfOptions,
                            logger);

                        int numberOfPages = PdfDocument.CountNumberOfPages(pdfFile);

                        logger.LogDebug($"Cover file \"{cover}\" contains number of PDF pages: {numberOfPages}.");

                        HtmlToPdfFile htmlToPdfFile = new HtmlToPdfFile
                        {
                            Input = cover,
                            Index = inputs.IndexOf(cover),
                            PdfFilePath = pdfFile,
                            PrintFooter = false,
                            NumberOfPages = numberOfPages
                        };

                        htmlToPdfFiles.Add(htmlToPdfFile);

                        coverAdded = true;
                    }

                    // page options
                    htmlToPdfOptions.MarginOptions = marginOptions;
                    htmlToPdfOptions.FooterTemplateBuilder.FooterRight = commandLineOptions.FooterRight;
                    htmlToPdfOptions.FooterTemplateBuilder.FooterStyle = commandLineOptions.FooterStyle;

                    var tasks = inputs.Where(x => !htmlToPdfFiles.Any(y => y.Input == x)).Select(async input =>
                    {
                        // print as pdf
                        string pdfFile = await pdfPrinter.PrintAsPdfAsync(
                            input,
                            htmlToPdfOptions,
                            logger);

                        int numberOfPages = PdfDocument.CountNumberOfPages(pdfFile);

                        logger.LogDebug($"\"{input}\" contains number of PDF pages: {numberOfPages}.");

                        HtmlToPdfFile htmlToPdfFile = new HtmlToPdfFile
                        {
                            Input = input,
                            Index = inputs.IndexOf(input),
                            PdfFilePath = pdfFile,
                            PrintFooter = true,
                            NumberOfPages = numberOfPages
                        };

                        htmlToPdfFiles.Add(htmlToPdfFile);
                    });

                    await Task.WhenAll(tasks);

                    // set page numbers for each HTML file
                    int currentPageNumber = 1;
                    foreach (HtmlToPdfFile htmlToPdfFile in htmlToPdfFiles.OrderBy(x => x.Index))
                    {
                        htmlToPdfFile.OutputPdfFilePageNumber = currentPageNumber;
                        htmlToPdfOptions.PageOffset = currentPageNumber;
                        htmlToPdfOptions.NumberOfPages = htmlToPdfFile.NumberOfPages;

                        if (htmlToPdfFile.PrintFooter)
                        {
                            // delete previously created PDF file
                            File.Delete(htmlToPdfFile.PdfFilePath);

                            // print as pdf
                            string pdfFile = await pdfPrinter.PrintAsPdfAsync(
                                htmlToPdfFile.Input,
                                htmlToPdfOptions,
                                logger);

                            htmlToPdfFile.PdfFilePath = pdfFile;
                        }

                        currentPageNumber += htmlToPdfOptions.NumberOfPages;
                    }
                }
                finally
                {
                    await browser.CloseAsync();
                }
            }

            // merge pdf files
            byte[] mergedBytes = PdfMerger.Merge(htmlToPdfFiles
                .OrderBy(x => x.OutputPdfFilePageNumber)
                .Select(x => x.PdfFilePath));

            string tempOutputFilePath = outputFilePath;
            if (stdout)
            {
                tempOutputFilePath = Path.GetTempFileName();
            }

            File.WriteAllBytes(tempOutputFilePath, mergedBytes);

            // update external file links to internal document links
            PdfDocument.UpdateLinks(tempOutputFilePath, htmlToPdfFiles, logger);

            if (stdout)
            {
                byte[] outputPdfFileBytes = File.ReadAllBytes(tempOutputFilePath);
                Console.Write(Encoding.Default.GetString(outputPdfFileBytes));
                File.Delete(tempOutputFilePath);
            }

            // delete temporary PDF files
            var deleteTempFileTasks = htmlToPdfFiles.Where(x => !string.IsNullOrEmpty(x.PdfFilePath)).Select(async input =>
            {
                await Task.Factory.StartNew(() =>
                {
                    if (File.Exists(input.PdfFilePath))
                    {
                        File.Delete(input.PdfFilePath);
                    }
                });
            });

            await Task.WhenAll(deleteTempFileTasks);

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
