// <copyright file="HtmlFileParser.cs" company="HtmlToPdf">
// Copyright (c) HtmlToPdf. All rights reserved.
// </copyright>

namespace HtmlToPdf
{
    using System.Collections.Generic;
    using HtmlAgilityPack;

    /// <summary>
    /// HTML File Parser.
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
        /// Gets the title and headings.
        /// </summary>
        /// <param name="tableOfContentsIncluded">if set to <c>true</c> indicates a table of contents is included in the PDF.</param>
        /// <returns>The headings.</returns>
        internal List<HtmlHeading> GetTitleAndHeadings(bool tableOfContentsIncluded)
        {
            List<HtmlHeading> headings = new List<HtmlHeading>();

            HtmlDocument doc = new HtmlDocument();
            doc.Load(this.htmlFilePath);

            // get title
            HtmlNode titleNode = doc.DocumentNode.SelectSingleNode("//head/title");
            headings.Add(new HtmlHeading
            {
                Text = titleNode?.InnerText ?? string.Empty,
                Level = 0,
                Page = tableOfContentsIncluded ? 1 : 0,
            });

            string xpathQuery = "//*[starts-with(name(),'h') and string-length(name()) = 2 and number(substring(name(), 2)) <= 6]";
            var nodes = doc.DocumentNode.SelectNodes(xpathQuery);

            if (nodes == null)
            {
                return headings;
            }

            foreach (var node in nodes)
            {
                int level = int.Parse(node.Name.Substring(1));

                headings.Add(new HtmlHeading
                {
                    Text = node.InnerHtml,
                    Level = level,
                    Page = 1,
                });
            }

            return headings;
        }
    }
}
