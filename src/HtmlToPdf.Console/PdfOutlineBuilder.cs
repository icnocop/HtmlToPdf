// <copyright file="PdfOutlineBuilder.cs" company="HtmlToPdf">
// Copyright (c) HtmlToPdf. All rights reserved.
// </copyright>

namespace HtmlToPdf.Console
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Xml;
    using System.Xml.Serialization;
    using HtmlToPdf.Console.Outline.Xml;

    /// <summary>
    /// PDF Outline Builder
    /// </summary>
    internal static class PdfOutlineBuilder
    {
        /// <summary>
        /// Builds the outline.
        /// </summary>
        /// <param name="xmlWriter">The XML writer.</param>
        /// <param name="htmlToPdfFiles">The HTML to PDF files.</param>
        /// <param name="tableOfContentsIncluded">if set to <c>true</c> indicates a table of contents is included in the PDF.</param>
        internal static void BuildOutline(XmlWriter xmlWriter, IReadOnlyCollection<HtmlToPdfFile> htmlToPdfFiles, bool tableOfContentsIncluded)
        {
            outline outline = new outline
            {
                Items = BuildOutline(htmlToPdfFiles.SelectMany(x => x.TitleAndHeadings), tableOfContentsIncluded)
            };

            var serializer = new XmlSerializer(typeof(outline));

            XmlSerializerNamespaces namespaces = new XmlSerializerNamespaces();
            namespaces.Add(string.Empty, "http://wkhtmltopdf.org/outline");

            serializer.Serialize(xmlWriter, outline, namespaces);
        }

        private static List<item> BuildOutline(IEnumerable<HtmlHeading> headings, bool tableOfContentsIncluded)
        {
            int counter = 0;

            var items = new List<item>();
            var allItems = new List<item>();
            item parentNode = null;
            item currentItem = null;

            foreach (var heading in headings)
            {
                // if there is no previous item, add it to the current list
                // if previously added item is the same level, add it to current list
                // if previously added item is a smaller level, add it to the previous item's children
                // if previously added item is a larger level, add it to the previously added item's parent
                string link = string.Empty;
                string backLink = string.Empty;

                if ((!string.IsNullOrEmpty(heading.Text)) || !tableOfContentsIncluded)
                {
                    string index = IntToBase(counter++, 36);
                    link = $"__WKANCHOR_{index}";

                    if (!tableOfContentsIncluded)
                    {
                        index = IntToBase(counter++, 36);
                    }

                    backLink = $"__WKANCHOR_{index}";

                    if (tableOfContentsIncluded)
                    {
                        counter++;
                    }
                }

                currentItem = new item
                {
                    title = heading.Text,
                    level = heading.Level,
                    link = link,
                    backLink = backLink,
                    page = heading.Page.ToString(),
                    children = new List<item>()
                };

                allItems.Add(currentItem);

                if (!allItems.Any(n => n.level < currentItem.level))
                {
                    items.Add(currentItem);
                    parentNode = null;
                }

                if (parentNode != null)
                {
                    if (parentNode.level >= currentItem.level)
                    {
                        parentNode = allItems.Last(n => n.level < currentItem.level);
                    }

                    // add item as child of parent
                    parentNode.children.Add(currentItem);
                }

                parentNode = currentItem;
            }

            return items;
        }

        private static string IntToBase(int input, int @base)
        {
            var digits = "0123456789abcdefghijklmnopqrstuvwxyz";

            if (@base < 2 || @base > 36)
            {
                throw new ArgumentOutOfRangeException(nameof(@base), "Must specify a base between 2 and 36, inclusive.");
            }

            if (input < @base && input >= 0)
            {
                return digits[input].ToString();
            }
            else
            {
                return IntToBase(input / @base, @base) + digits[input % @base].ToString();
            }
        }
    }
}
