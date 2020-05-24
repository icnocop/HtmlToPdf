// <copyright file="FooterTemplateBuilder.cs" company="HtmlToPdf">
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
                return !(string.IsNullOrEmpty(this.FooterLeft)
                    && string.IsNullOrEmpty(this.FooterCenter)
                    && string.IsNullOrEmpty(this.FooterRight));
            }
        }

        /// <summary>
        /// Gets or sets the footer style.
        /// </summary>
        public string FooterStyle { get; set; }

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
        /// Builds the footer template with the specified CSS.
        /// </summary>
        /// <returns>The footer template.</returns>
        internal string Build()
        {
            if (!this.DisplayHeaderFooter)
            {
                return string.Empty;
            }

            // https://chromedevtools.github.io/devtools-protocol/tot/Page/#method-printToPDF
            return $@"
<div id=""footer-template"" style=""{this.FooterStyle}"">
    <div style=""float:left;"">{this.FooterLeft}</div>
    <div style=""text-align:center;position:absolute;top:0;right:0;bottom:0;left:0;"">{this.FooterCenter}</div>
    <div style=""float:right;"">{this.FooterRight}</div>
</div>"
                .Replace("[page]", "<span class=\"pageNumber\"></span>")
                .Replace("[date]", "<span class=\"date\"></span>")
                .Replace("[title]", "<span class=\"title\"></span>")
                .Replace("[webpage]", "<span class=\"url\"></span>");
        }
    }
}
