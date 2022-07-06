// <copyright file="TemplateBuilder.cs" company="HtmlToPdf">
// Copyright (c) HtmlToPdf. All rights reserved.
// </copyright>

namespace HtmlToPdf
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Net;

    /// <summary>
    /// Template Builder.
    /// </summary>
    public abstract class TemplateBuilder
    {
        private readonly string name;
        private bool loadedExternalHtml;
        private string externalHtml;

        /// <summary>Initializes a new instance of the <see cref="TemplateBuilder" /> class.</summary>
        /// <param name="name">The name.</param>
        public TemplateBuilder(string name)
        {
            this.name = name;
        }

        /// <summary>
        /// Gets a value indicating whether to display the template.
        /// </summary>
        public bool DisplayTemplate
        {
            get
            {
                return !(string.IsNullOrEmpty(this.Left)
                    && string.IsNullOrEmpty(this.Center)
                    && string.IsNullOrEmpty(this.Right)
                    && string.IsNullOrEmpty(this.Html));
            }
        }

        /// <summary>
        /// Gets or sets the left text.
        /// </summary>
        public string Left { get; set; }

        /// <summary>
        /// Gets or sets the center text.
        /// </summary>
        public string Center { get; set; }

        /// <summary>
        /// Gets or sets the right text.
        /// </summary>
        public string Right { get; set; }

        /// <summary>
        /// Gets or sets the size of the font.
        /// </summary>
        public string FontSize { get; set; }

        /// <summary>
        /// Gets or sets the name of the font.
        /// </summary>
        public string FontName { get; set; }

        /// <summary>
        /// Gets or sets the HTML.
        /// </summary>
        public string Html { get; set; }

        /// <summary>
        /// Builds the template with the specified CSS.
        /// </summary>
        /// <param name="variables">The variables.</param>
        /// <returns>
        /// The template.
        /// </returns>
        internal string Build(Dictionary<string, string> variables)
        {
            string html = this.BuildHtml();
            if (string.IsNullOrEmpty(html))
            {
                return html;
            }

            foreach (KeyValuePair<string, string> variable in variables)
            {
                html = html.Replace($"[{variable.Key}]", variable.Value);
            }

            return html;
        }

        /// <summary>
        /// Determines whether the HTML contains the specified variable.
        /// </summary>
        /// <param name="variable">The variable.</param>
        /// <returns>
        ///   <c>true</c> if the HTML contains the specified variable; otherwise, <c>false</c>.
        /// </returns>
        internal bool ContainsVariable(string variable)
        {
            if (!this.DisplayTemplate)
            {
                return false;
            }

            return ((this.Left != null) && this.Left.Contains($"[{variable}]"))
                || ((this.Center != null) && this.Center.Contains($"[{variable}]"))
                || ((this.Right != null) && this.Right.Contains($"[{variable}]"))
                || this.BuildHtml().Contains($"[{variable}]");
        }

        private string BuildHtml()
        {
            if (!this.DisplayTemplate)
            {
                return string.Empty;
            }

            string html;

            if (string.IsNullOrEmpty(this.Html))
            {
                if (string.IsNullOrEmpty(this.Left)
                    && string.IsNullOrEmpty(this.Right))
                {
                    // in order for the text to be displayed in the center, there needs to be text in either the left or right
                    this.Left = "&nbsp;";
                }

                // https://source.chromium.org/chromium/chromium/src/+/master:components/printing/resources/print_header_footer_template_page.html
                // to print a background color in the header/footer add the following to the style:
                // -webkit-print-color-adjust: exact; background-color: red;
                html = $@"<style>#header, #footer {{ padding: 0 !important; }}</style><div id=""{this.name}-template"" style=""margin:0;font-family:{this.FontName};font-size:{this.FontSize};color:#808080;position:relative;padding-left:10px;padding-right:10px;width:100%;"">
    <div style=""float:left;"">{this.Left}</div>
    <div style=""text-align:center;position:absolute;top:0;right:0;bottom:0;left:0;"">{this.Center}</div>
    <div style=""float:right;"">{this.Right}</div>
</div>";
            }
            else
            {
                if (this.loadedExternalHtml)
                {
                    html = this.externalHtml;
                }
                else
                {
                    bool isFile;
                    try
                    {
                        Uri uri = new Uri(this.Html);
                        isFile = uri.IsFile;
                    }
                    catch
                    {
                        isFile = true;
                    }

                    if (isFile)
                    {
                        if (!Path.IsPathRooted(this.Html))
                        {
                            this.Html = Path.GetFullPath(this.Html);
                        }

                        html = File.ReadAllText(this.Html);
                    }
                    else
                    {
                        using (var webClient = new WebClient())
                        {
                            html = webClient.DownloadString(this.Html);
                        }
                    }

                    this.externalHtml = html;
                    this.loadedExternalHtml = true;
                }
            }

            return html;
        }
    }
}
