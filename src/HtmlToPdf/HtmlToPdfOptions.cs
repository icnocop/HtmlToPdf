// <copyright file="HtmlToPdfOptions.cs" company="HtmlToPdf">
// Copyright (c) HtmlToPdf. All rights reserved.
// </copyright>

namespace HtmlToPdf
{
    using PuppeteerSharp.Media;

    /// <summary>
    /// HTML to PDF options
    /// </summary>
    public class HtmlToPdfOptions
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="HtmlToPdfOptions"/> class.
        /// </summary>
        public HtmlToPdfOptions()
        {
            this.MarginOptions = new MarginOptions();
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
        /// Gets or sets the footer template.
        /// </summary>
        public string FooterTemplate { get; set; }

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
        /// Gets a value indicating whether to display the header and footer.
        /// </summary>
        public bool DisplayHeaderFooter
        {
            get
            {
                return !string.IsNullOrEmpty(this.FooterTemplate);
            }
        }
    }
}
