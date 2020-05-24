// <copyright file="CommandLineOptions.cs" company="HtmlToPdf">
// Copyright (c) HtmlToPdf. All rights reserved.
// </copyright>

namespace HtmlToPdf
{
    using System;
    using System.Collections.Generic;
    using CommandLine;
    using PuppeteerSharp.Media;

    /// <summary>
    /// Command Line Options
    /// </summary>
    public class CommandLineOptions
    {
        /// <summary>
        /// Gets or sets the inputs.
        /// </summary>
        [Value(0, Required = false, MetaName = "<input file> [<input file> ...] <output file>")]
        public IEnumerable<string> Inputs { get; set; }

        /// <summary>
        /// Gets or sets the JavaScript delay in milliseconds.
        /// </summary>
        [Option("javascript-delay", Required = false, Default = 200, HelpText = "The number of milliseconds to wait for javascript to finish")]
        public int JavascriptDelayInMilliseconds { get; set; }

        /// <summary>
        /// Gets or sets the log level.
        /// </summary>
        [Option("log-level", Required = false, Default = LogLevel.Info, HelpText = "Set log level to: none, error, warn, info, or debug")]
        public LogLevel LogLevel { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether or not to output verbose information.
        /// </summary>
        [Option('q', "quiet", Required = false, Default = false, Hidden = true, HelpText = "Be less verbose, maintained for backwards compatibility; Same as using --log-level none")]
        public bool Quiet { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to not put an outline into the PDF.
        /// </summary>
        [Option("no-outline", Required = false, Default = false, Hidden = true, HelpText = "Do not put an outline into the pdf")]
        public bool NoOutline { get; set; }

        /// <summary>
        /// Gets or sets the encoding.
        /// </summary>
        [Option("encoding", Required = false, Hidden = true, HelpText = "Set the default text encoding, for input")]
        public string Encoding { get; set; }

        /// <summary>
        /// Gets or sets the user style sheet.
        /// </summary>
        [Option("user-style-sheet", Required = false, HelpText = "Specify a user style sheet, to load with every page")]
        public string UserStyleSheet { get; set; }

        /// <summary>
        /// Gets or sets the footer left.
        /// </summary>
        [Option("footer-left", MetaValue = "<text>", HelpText = "Left aligned footer text")]
        public string FooterLeft { get; set; }

        /// <summary>
        /// Gets or sets the footer center.
        /// </summary>
        [Option("footer-center", MetaValue = "<text>", HelpText = "Centered footer text")]
        public string FooterCenter { get; set; }

        /// <summary>
        /// Gets or sets the footer right.
        /// </summary>
        [Option("footer-right", MetaValue = "<text>", HelpText = "Right aligned footer text")]
        public string FooterRight { get; set; }

        /// <summary>
        /// Gets or sets the size of the footer font.
        /// </summary>
        [Option("footer-font-size", Default = "12", MetaValue = "<size>", HelpText = "Footer font size")]
        public string FooterFontSize { get; set; }

        /// <summary>
        /// Gets or sets the name of the footer font.
        /// </summary>
        [Option("footer-font-name", Default = "Arial", MetaValue = "<name>", HelpText = "Footer font name")]
        public string FooterFontName { get; set; }

        /// <summary>
        /// Gets or sets the footer HTML.
        /// </summary>
        [Option("footer-html", MetaValue = "<url>", HelpText = "HTML footer")]
        public string FooterHtml { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether or not to read arguments from standard input.
        /// </summary>
        [Option("read-args-from-stdin", HelpText = "Read command line arguments from stdin")]
        public bool ReadArgsFromStdin { get; set; }

        /// <summary>
        /// Gets or sets the bottom margin.
        /// </summary>
        [Option('B', "margin-bottom", MetaValue = "<unitreal>", HelpText = "Set the page bottom margin")]
        public string BottomMargin { get; set; }

        /// <summary>
        /// Gets or sets the left margin.
        /// </summary>
        [Option('L', "margin-left", Default = "10mm", MetaValue = "<unitreal>", HelpText = "Set the page left margin")]
        public string LeftMargin { get; set; }

        /// <summary>
        /// Gets or sets the right margin.
        /// </summary>
        [Option('R', "margin-right", Default = "10mm", MetaValue = "<unitreal>", HelpText = "Set the page right margin")]
        public string RightMargin { get; set; }

        /// <summary>
        /// Gets or sets the top margin.
        /// </summary>
        [Option('T', "margin-top", MetaValue = "<unitreal>", HelpText = "Set the page top margin")]
        public string TopMargin { get; set; }

        /// <summary>
        /// Gets or sets the orientation.
        /// </summary>
        [Option('O', "orientation", Default = "Portrait", MetaValue = "<orientation>", HelpText = "Set orientation to Landscape or Portrait")]
        public string Orientation { get; set; }

        /// <summary>
        /// Gets or sets the size of the page.
        /// </summary>
        [Option('s', "page-size", Default = "A4", MetaValue = "<Size>", HelpText = "Set paper size to: A4, Letter, etc.")]
        public string PageSize { get; set; }

        /// <summary>
        /// Gets or sets the height of the page.
        /// </summary>
        [Option("page-height", MetaValue = "<unitreal>", HelpText = "Page height")]
        public string PageHeight { get; set; }

        /// <summary>
        /// Gets or sets the width of the page.
        /// </summary>
        [Option("page-width", MetaValue = "<unitreal>", HelpText = "Page width")]
        public string PageWidth { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to print the background.
        /// </summary>
        [Option("background", Default = true, HelpText = "Do print background")]
        public bool Background { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to not print the background.
        /// </summary>
        [Option("no-background", HelpText = "Do not print background")]
        public bool NoBackground { get; set; }

        /// <summary>
        /// Gets or sets the page offset.
        /// </summary>
        [Option("page-offset", Default = 0, MetaValue = "<offset>", HelpText = "The starting page number")]
        public int PageOffset { get; set; }

        /// <summary>
        /// Gets the logger level.
        /// </summary>
        internal LogLevel LoggerLevel
        {
            get
            {
                LogLevel logLevel = this.LogLevel;
                if (this.Quiet)
                {
                    logLevel = LogLevel.Info;
                }

                return logLevel;
            }
        }

        /// <summary>
        /// Gets a value indicating whether or not to print the background.
        /// </summary>
        internal bool PrintBackground
        {
            get
            {
                return !this.NoBackground;
            }
        }

        /// <summary>
        /// Gets a value indicating whether to print in landscape orientation.
        /// </summary>
        internal bool Landscape
        {
            get
            {
                return this.Orientation == "Landscape";
            }
        }

        /// <summary>
        /// Gets the paper format.
        /// </summary>
        internal PaperFormat PaperFormat
        {
            get
            {
                switch (this.PageSize)
                {
                    case "A4":
                        return PaperFormat.A4;
                    case "A3":
                        return PaperFormat.A3;
                    case "A2":
                        return PaperFormat.A2;
                    case "A1":
                        return PaperFormat.A1;
                    case "A0":
                        return PaperFormat.A0;
                    case "Ledger":
                        return PaperFormat.Ledger;
                    case "Tabloid":
                        return PaperFormat.Tabloid;
                    case "A5":
                        return PaperFormat.A5;
                    case "Legal":
                        return PaperFormat.Legal;
                    case "Letter":
                        return PaperFormat.Letter;
                    case "A6":
                        return PaperFormat.A6;
                    default:
                        throw new ArgumentOutOfRangeException(this.PageSize);
                }
            }
        }
    }
}
