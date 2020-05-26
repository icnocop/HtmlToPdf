// <copyright file="HtmlFileParser.cs" company="HtmlToPdf">
// Copyright (c) HtmlToPdf. All rights reserved.
// </copyright>

namespace HtmlToPdf
{
    using HtmlAgilityPack;

    /// <summary>
    /// HTML File Parser
    /// </summary>
    internal class HtmlFileParser
    {
        private readonly string htmlFilePath;

        /// <summary>
        /// Initializes a new instance of the <see cref="HtmlFileParser"/> class.
        /// </summary>
        /// <param name="htmlFilePath">The HTML file path.</param>
        public HtmlFileParser(string htmlFilePath)
        {
            this.htmlFilePath = htmlFilePath;
        }

        /// <summary>
        /// Gets the title.
        /// </summary>
        /// <returns>The title.</returns>
        internal string GetTitle()
        {
            HtmlDocument doc = new HtmlDocument();
            doc.Load(this.htmlFilePath);

            HtmlNode titleNode = doc.DocumentNode.SelectSingleNode("//head/title");
            if (titleNode == null)
            {
                return null;
            }

            return titleNode.InnerText;
        }
    }
}
