﻿// <copyright file="FooterTemplateBuilder.cs" company="HtmlToPdf">
// Copyright (c) HtmlToPdf. All rights reserved.
// </copyright>

namespace HtmlToPdf
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Net;

    /// <summary>
    /// Footer Template Builder
    /// </summary>
    public class FooterTemplateBuilder
    {
        private bool loadedExternalFooterHtml;
        private string externalFooterHtml;

        /// <summary>
        /// Gets a value indicating whether to display the header and footer.
        /// </summary>
        public bool DisplayHeaderFooter
        {
            get
            {
                return !(string.IsNullOrEmpty(this.FooterLeft)
                    && string.IsNullOrEmpty(this.FooterCenter)
                    && string.IsNullOrEmpty(this.FooterRight)
                    && string.IsNullOrEmpty(this.FooterHtml));
            }
        }

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
        /// Builds the footer template with the specified CSS.
        /// </summary>
        /// <param name="variables">The variables.</param>
        /// <returns>
        /// The footer template.
        /// </returns>
        internal string Build(Dictionary<string, string> variables)
        {
            string footerHtml = this.BuildFooterHtml();
            if (string.IsNullOrEmpty(footerHtml))
            {
                return footerHtml;
            }

            foreach (KeyValuePair<string, string> variable in variables)
            {
                footerHtml = footerHtml.Replace($"[{variable.Key}]", variable.Value);
            }

            return footerHtml;
        }

        /// <summary>
        /// Determines whether the footer HTML contains the specified variable.
        /// </summary>
        /// <param name="variable">The variable.</param>
        /// <returns>
        ///   <c>true</c> if the footer HTML contains the specified variable; otherwise, <c>false</c>.
        /// </returns>
        internal bool ContainsVariable(string variable)
        {
            if (!this.DisplayHeaderFooter)
            {
                return false;
            }

            return ((this.FooterLeft != null) && this.FooterLeft.Contains($"[{variable}]"))
                || ((this.FooterCenter != null) && this.FooterCenter.Contains($"[{variable}]"))
                || ((this.FooterRight != null) && this.FooterRight.Contains($"[{variable}]"))
                || this.BuildFooterHtml().Contains($"[{variable}]");
        }

        private string BuildFooterHtml()
        {
            if (!this.DisplayHeaderFooter)
            {
                return string.Empty;
            }

            string footerHtml;

            if (string.IsNullOrEmpty(this.FooterHtml))
            {
                if (string.IsNullOrEmpty(this.FooterLeft)
                    && string.IsNullOrEmpty(this.FooterRight))
                {
                    // in order for the text to be displayed in the center, there needs to be text in either the left or right
                    this.FooterLeft = "&nbsp;";
                }

                // https://source.chromium.org/chromium/chromium/src/+/master:components/printing/resources/print_header_footer_template_page.html
                // to print a background color in the header/footer add the following to the style:
                // -webkit-print-color-adjust: exact; background-color: red;
                footerHtml = $@"<style>#header, #footer {{ padding: 0 !important; }}</style><div id=""footer-template"" style=""margin:0;font-family:{this.FooterFontName};font-size:{this.FooterFontSize};color:#808080;position:relative;padding-left:10px;padding-right:10px;width:100%;"">
    <div style=""float:left;"">{this.FooterLeft}</div>
    <div style=""text-align:center;position:absolute;top:0;right:0;bottom:0;left:0;"">{this.FooterCenter}</div>
    <div style=""float:right;"">{this.FooterRight}</div>
</div>";
            }
            else
            {
                if (this.loadedExternalFooterHtml)
                {
                    footerHtml = this.externalFooterHtml;
                }
                else
                {
                    bool isFile;
                    try
                    {
                        Uri uri = new Uri(this.FooterHtml);
                        isFile = uri.IsFile;
                    }
                    catch
                    {
                        isFile = true;
                    }

                    if (isFile)
                    {
                        if (!Path.IsPathRooted(this.FooterHtml))
                        {
                            this.FooterHtml = Path.GetFullPath(this.FooterHtml);
                        }

                        footerHtml = File.ReadAllText(this.FooterHtml);
                    }
                    else
                    {
                        using (var webClient = new WebClient())
                        {
                            footerHtml = webClient.DownloadString(this.FooterHtml);
                        }
                    }

                    this.externalFooterHtml = footerHtml;
                    this.loadedExternalFooterHtml = true;
                }
            }

            return footerHtml;
        }
    }
}
