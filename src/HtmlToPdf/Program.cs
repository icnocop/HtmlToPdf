// <copyright file="Program.cs" company="HtmlToPdf">
// Copyright (c) HtmlToPdf. All rights reserved.
// </copyright>

namespace HtmlToPdf
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Diagnostics;
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
            try
            {
                // System.Diagnostics.Debugger.Launch();
                Console.OutputEncoding = Encoding.Default;

                var parser = new Parser(config =>
                {
                    config.HelpWriter = null;
                });

                var task = ParseAndRunOptions(parser, args);
                return task.Result;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex);
                return 1;
            }
        }

        private static async Task<int> ParseAndRunOptions(Parser parser, string[] args)
        {
            var parserResult = parser.ParseArguments<CommandLineOptions>(args);
            var task = parserResult.MapResult(
                (CommandLineOptions options) =>
                {
                    try
                    {
                        Task<int> runTask = RunAsync(parser, args, options);
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

                        Logger.LogError(parserResult.GetAutoBuildHelpText());
                        Logger.LogError(string.Join(Environment.NewLine, errorMessages));
                        return Error();
                    }
                },
                errs =>
                {
                    DisplayHelpText(parserResult);
                    return Error();
                });

            return await task;
        }

        private static void DisplayHelpText<T>(ParserResult<T> parserResult)
        {
            HelpText helpText = HelpText.AutoBuild(parserResult);
            helpText.AddOptions(parserResult);
            Logger.LogError(helpText);
        }

        private static Task<int> Error()
        {
            Task<int> exitTask = new Task<int>(() => 1);
            exitTask.Start();
            return exitTask;
        }

        private static async Task<int> RunAsync(Parser parser, string[] args, CommandLineOptions options)
        {
            if (options.ReadArgsFromStdin)
            {
                Trace.WriteLine("Reading arguments from stdin...");

                string stdin = Console.In.ReadToEnd();

                Trace.WriteLine($"Arguments from stdin: {stdin}");

                options.ReadArgsFromStdin = false;
                string commandLine = parser.FormatCommandLine(options);

                string newCommandLine = $"{commandLine} {stdin}";
                Trace.WriteLine($"Running with arguments: {newCommandLine}");

                string[] newArgs = ParseArguments(newCommandLine);
                return await ParseAndRunOptions(parser, newArgs);
            }

            Encoding encoding = Encoding.Default;
            if (!string.IsNullOrEmpty(options.Encoding))
            {
                encoding = Encoding.GetEncoding(options.Encoding);
            }

            List<string> inputs = options.Inputs
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

            ConcurrentDictionary<string, HtmlToPdfFile> htmlToPdfFiles = new ConcurrentDictionary<string, HtmlToPdfFile>();

            foreach (string input in inputs)
            {
                Trace.WriteLine(input);
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

            options.UserStyleSheet = options.UserStyleSheet?.Trim('"');

            BrowserDownloader.DownloadBrowser();

            using (Browser browser = await Puppeteer.LaunchAsync(new LaunchOptions
            {
                Headless = true
            }))
            {
                MarginOptions marginOptions = new MarginOptions
                {
                    Bottom = options.BottomMargin,
                    Left = options.LeftMargin,
                    Right = options.RightMargin,
                    Top = options.TopMargin
                };

                try
                {
                    PdfPrinter pdfPrinter = new PdfPrinter(browser);

                    // cover options
                    HtmlToPdfOptions htmlToPdfOptions = new HtmlToPdfOptions
                    {
                        StyleSheet = options.UserStyleSheet,
                        JavascriptDelayInMilliseconds = options.JavascriptDelayInMilliseconds,
                        Landscape = options.Landscape,
                        PaperFormat = options.PaperFormat,
                        Height = options.PageHeight,
                        Width = options.PageWidth,
                        PrintBackground = options.PrintBackground
                    };

                    if (inputs.Contains("cover") && (!coverAdded))
                    {
                        int index = inputs.IndexOf("cover");
                        string cover = inputs[index + 1];
                        inputs.RemoveAt(index); // remove "cover"
                        inputs.RemoveAt(index); // remove cover

                        string pdfFile = await pdfPrinter.PrintAsPdfAsync(
                            cover,
                            htmlToPdfOptions);

                        HtmlToPdfFile htmlToPdfFile = new HtmlToPdfFile
                        {
                            PdfFilePath = pdfFile
                        };

                        if (!htmlToPdfFiles.TryAdd(cover, htmlToPdfFile))
                        {
                            throw new Exception($"Failed to add '{cover}'.");
                        }

                        coverAdded = true;
                    }

                    // page options
                    htmlToPdfOptions.MarginOptions = marginOptions;
                    htmlToPdfOptions.FooterTemplate = options.FooterTemplate;

                    var tasks = inputs.Select(async input =>
                    {
                        // print as pdf
                        string pdfFile = await pdfPrinter.PrintAsPdfAsync(
                            input,
                            htmlToPdfOptions);

                        int numberOfPages = PdfDocument.CountNumberOfPages(pdfFile);

                        HtmlToPdfFile htmlToPdfFile = new HtmlToPdfFile
                        {
                            PdfFilePath = pdfFile,
                            NumberOfPages = numberOfPages
                        };

                        // Logger.LogError($"Adding '{input}'");
                        if (!htmlToPdfFiles.TryAdd(input, htmlToPdfFile))
                        {
                            throw new Exception($"Failed to add '{input}'.");
                        }
                    });

                    await Task.WhenAll(tasks);

                    // set page numbers for each HTML file
                    int currentPageNumber = 1;
                    foreach (string input in inputs)
                    {
                        HtmlToPdfFile htmlToPdfFile = htmlToPdfFiles[input];

                        // delete previously created PDF file
                        File.Delete(htmlToPdfFile.PdfFilePath);

                        htmlToPdfFile.OutputPdfFilePageNumber = currentPageNumber;

                        htmlToPdfOptions.PageOffset = currentPageNumber;
                        htmlToPdfOptions.NumberOfPages = htmlToPdfFile.NumberOfPages;

                        // print as pdf
                        string pdfFile = await pdfPrinter.PrintAsPdfAsync(
                            input,
                            htmlToPdfOptions);

                        htmlToPdfFile.PdfFilePath = pdfFile;

                        currentPageNumber += htmlToPdfOptions.NumberOfPages;
                    }
                }
                finally
                {
                    await browser.CloseAsync();
                }
            }

            // merge pdf files
            byte[] mergedBytes = PdfMerger.Merge(htmlToPdfFiles.Values
                .OrderBy(x => x.OutputPdfFilePageNumber)
                .Select(x => x.PdfFilePath));

            string tempOutputFilePath = outputFilePath;
            if (stdout)
            {
                tempOutputFilePath = Path.GetTempFileName();
            }

            File.WriteAllBytes(tempOutputFilePath, mergedBytes);

            // update external file links to internal document links
            PdfDocument.UpdateLinks(tempOutputFilePath, htmlToPdfFiles);

            if (stdout)
            {
                byte[] outputPdfFileBytes = File.ReadAllBytes(tempOutputFilePath);
                Console.Write(Encoding.Default.GetString(outputPdfFileBytes));
                File.Delete(tempOutputFilePath);
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
