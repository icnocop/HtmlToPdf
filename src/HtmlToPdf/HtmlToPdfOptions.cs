﻿// <copyright file="HtmlToPdfOptions.cs" company="HtmlToPdf">
// Copyright (c) HtmlToPdf. All rights reserved.
// </copyright>

namespace HtmlToPdf
{
    using PuppeteerSharp.Media;

    /// <summary>
    /// HTML to PDF options.
    /// </summary>
    public class HtmlToPdfOptions
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="HtmlToPdfOptions"/> class.
        /// </summary>
        public HtmlToPdfOptions()
        {
            this.MarginOptions = new MarginOptions();
            this.FooterTemplateBuilder = new FooterTemplateBuilder();
            this.HeaderTemplateBuilder = new HeaderTemplateBuilder();
        }

        /// <summary>
        /// Gets or sets the style sheet.
        /// </summary>
        public string StyleSheet { get; set; }

        /// <summary>
        /// Gets or sets the javascript delay in milliseconds.
        /// </summary>
        public int JavascriptDelayInMilliseconds { get; set; }

        /// <summary>
        /// Gets or sets the margin options.
        /// </summary>
        public MarginOptions MarginOptions { get; set; }

        /// <summary>
        /// Gets or sets the footer template builder.
        /// </summary>
        public FooterTemplateBuilder FooterTemplateBuilder { get; set; }

        /// <summary>
        /// Gets or sets the header template builder.
        /// </summary>
        public HeaderTemplateBuilder HeaderTemplateBuilder { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to print in landscape orientation.
        /// </summary>
        public bool Landscape { get; set; }

        /// <summary>
        /// Gets or sets the paper format.
        /// </summary>
        public PaperFormat PaperFormat { get; set; }

        /// <summary>
        /// Gets or sets the height.
        /// </summary>
        public string Height { get; set; }

        /// <summary>
        /// Gets or sets the width.
        /// </summary>
        public string Width { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to print the background.
        /// </summary>
        public bool PrintBackground { get; set; }

        /// <summary>
        /// Gets or sets the page offset.
        /// </summary>
        public int PageOffset { get; set; }

        /// <summary>
        /// Gets or sets the page number offset.
        /// </summary>
        public int PageNumberOffset { get; set; }

        /// <summary>
        /// Gets or sets the number of pages.
        /// </summary>
        public int NumberOfPages { get; set; }

        /// <summary>
        /// Copies this instance.
        /// </summary>
        /// <returns>A copy of this instance.</returns>
        public HtmlToPdfOptions Copy()
        {
            return (HtmlToPdfOptions)this.MemberwiseClone();
        }
    }
}
