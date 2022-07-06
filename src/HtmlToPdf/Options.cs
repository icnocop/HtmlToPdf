// <copyright file="Options.cs" company="HtmlToPdf">
// Copyright (c) HtmlToPdf. All rights reserved.
// </copyright>

namespace HtmlToPdf
{
    using System;
    using System.Collections.Generic;
    using System.Xml;
    using PuppeteerSharp.Media;

    /// <summary>
    /// Options.
    /// </summary>
    public class Options
    {
        /// <summary>
        /// Gets or sets the inputs.
        /// </summary>
        public List<string> Inputs { get; set; }

        /// <summary>
        /// Gets or sets the cover.
        /// </summary>
        public string Cover { get; set; }

        /// <summary>
        /// Gets or sets the JavaScript delay in milliseconds.
        /// </summary>
        public int JavascriptDelayInMilliseconds { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to not put an outline into the PDF.
        /// </summary>
        public bool Outline { get; set; }

        /// <summary>
        /// Gets or sets the encoding.
        /// </summary>
        public string Encoding { get; set; }

        /// <summary>
        /// Gets or sets the user style sheet.
        /// </summary>
        public string UserStyleSheet { get; set; }

        /// <summary>
        /// Gets or sets the header left.
        /// </summary>
        public string HeaderLeft { get; set; }

        /// <summary>
        /// Gets or sets the header center.
        /// </summary>
        public string HeaderCenter { get; set; }

        /// <summary>
        /// Gets or sets the header right.
        /// </summary>
        public string HeaderRight { get; set; }

        /// <summary>
        /// Gets or sets the size of the header font.
        /// </summary>
        public string HeaderFontSize { get; set; }

        /// <summary>
        /// Gets or sets the name of the header font.
        /// </summary>
        public string HeaderFontName { get; set; }

        /// <summary>
        /// Gets or sets the header HTML.
        /// </summary>
        public string HeaderHtml { get; set; }

        /// <summary>
        /// Gets or sets the footer left.
        /// </summary>
        public string FooterLeft { get; set; }

        /// <summary>
        /// Gets or sets the footer center.
        /// </summary>
        public string FooterCenter { get; set; }

        /// <summary>
        /// Gets or sets the footer right.
        /// </summary>
        public string FooterRight { get; set; }

        /// <summary>
        /// Gets or sets the size of the footer font.
        /// </summary>
        public string FooterFontSize { get; set; }

        /// <summary>
        /// Gets or sets the name of the footer font.
        /// </summary>
        public string FooterFontName { get; set; }

        /// <summary>
        /// Gets or sets the footer HTML.
        /// </summary>
        public string FooterHtml { get; set; }

        /// <summary>
        /// Gets or sets the bottom margin.
        /// </summary>
        public string BottomMargin { get; set; }

        /// <summary>
        /// Gets or sets the left margin.
        /// </summary>
        public string LeftMargin { get; set; }

        /// <summary>
        /// Gets or sets the right margin.
        /// </summary>
        public string RightMargin { get; set; }

        /// <summary>
        /// Gets or sets the top margin.
        /// </summary>
        public string TopMargin { get; set; }

        /// <summary>
        /// Gets or sets the orientation.
        /// </summary>
        public string Orientation { get; set; }

        /// <summary>
        /// Gets or sets the size of the page.
        /// </summary>
        public string PageSize { get; set; }

        /// <summary>
        /// Gets or sets the height of the page.
        /// </summary>
        public string PageHeight { get; set; }

        /// <summary>
        /// Gets or sets the width of the page.
        /// </summary>
        public string PageWidth { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to print the background.
        /// </summary>
        public bool PrintBackground { get; set; }

        /// <summary>
        /// Gets or sets the page offset.
        /// </summary>
        public int PageOffset { get; set; }

        /// <summary>
        /// Gets or sets the title.
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Gets or sets the output file path.
        /// </summary>
        public string OutputFilePath { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to dump the default TOC XSL style sheet to the standard output (STDOUT) stream.
        /// </summary>
        public bool DumpDefaultTocXsl { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to add a table of contents.
        /// </summary>
        public bool AddTableOfContents { get; set; }

        /// <summary>
        /// Gets or sets the outline builder.
        /// </summary>
        public Action<XmlWriter, IReadOnlyCollection<HtmlToPdfFile>, bool> OutlineBuilder { get; set; }

        /// <summary>
        /// Gets or sets the default table of contents style sheet builder.
        /// </summary>
        public Func<bool, string> DefaultTableOfContentsStyleSheetBuilder { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to output dotted lines in the table of contents.
        /// </summary>
        public bool OutputDottedLinesInTableOfContents { get; set; }

        /// <summary>
        /// Gets or sets the additional arguments to pass to the browser instance.
        /// </summary>
        public IEnumerable<string> AdditionalArguments { get; set; }

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
