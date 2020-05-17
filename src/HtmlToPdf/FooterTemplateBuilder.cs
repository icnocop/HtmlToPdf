﻿// <copyright file="FooterTemplateBuilder.cs" company="HtmlToPdf">
// Copyright (c) HtmlToPdf. All rights reserved.
// </copyright>

namespace HtmlToPdf
{
    /// <summary>
    /// Footer Template Builder
    /// </summary>
    public class FooterTemplateBuilder
    {
        /// <summary>
        /// Gets a value indicating whether to display the header and footer.
        /// </summary>
        public bool DisplayHeaderFooter
        {
            get
            {
                return !string.IsNullOrEmpty(this.FooterRight);
            }
        }

        /// <summary>
        /// Gets or sets the footer style.
        /// </summary>
        public string FooterStyle { get; set; }

        /// <summary>
        /// Gets or sets the footer right.
        /// </summary>
        public string FooterRight { get; set; }

        /// <summary>
        /// Builds the footer template with the specified CSS.
        /// </summary>
        /// <param name="footerTemplateCss">The footer template CSS.</param>
        /// <returns>The footer template.</returns>
        internal string Build(string footerTemplateCss)
        {
            if (string.IsNullOrEmpty(this.FooterRight))
            {
                return string.Empty;
            }

            return $"{footerTemplateCss ?? $"<style>#footer-template{{{this.FooterStyle}}}</style>"}<div id=\"footer-template\"><div style=\"text-align:right;\">{this.FooterRight}</div></div>"
                .Replace("[page]", "<span class=\"pageNumber\"></span>");
        }
    }
}