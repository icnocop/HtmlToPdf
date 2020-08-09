// <copyright file="PdfOutlineBuilder.cs" company="HtmlToPdf">
// Copyright (c) HtmlToPdf. All rights reserved.
// </copyright>

namespace HtmlToPdf
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Xml;
    using System.Xml.Xsl;

    /// <summary>
    /// PDF Outline Builder
    /// </summary>
    internal class PdfOutlineBuilder
    {
        /// <summary>
        /// Builds the outline asynchronous.
        /// </summary>
        /// <param name="coverIncluded">if set to <c>true</c> indicates a cover is included in the PDF.</param>
        /// <param name="tableOfContentsIncluded">if set to <c>true</c> indicates a table of contents is included in the PDF.</param>
        /// <param name="htmlToPdfFiles">The HTML to PDF files.</param>
        /// <param name="outlineBuilder">The outline builder.</param>
        /// <param name="pdfPrinter">The PDF printer.</param>
        /// <param name="htmlToPdfOptions">The HTML to PDF options.</param>
        /// <param name="variables">The variables.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        internal static async Task BuildOutlineAsync(
            bool coverIncluded,
            bool tableOfContentsIncluded,
            ConcurrentBag<HtmlToPdfFile> htmlToPdfFiles,
            Action<XmlWriter, IReadOnlyCollection<HtmlToPdfFile>, bool> outlineBuilder,
            PdfPrinter pdfPrinter,
            HtmlToPdfOptions htmlToPdfOptions,
            Dictionary<string, string> variables)
        {
            int tocIndex = coverIncluded ? 1 : 0;
            int tocPageNumber = htmlToPdfFiles.Where(x => x.Index < tocIndex).Sum(x => x.NumberOfPages) + 1;

            foreach (HtmlToPdfFile htmlToPdfFile in htmlToPdfFiles.Where(x => x.Index >= tocIndex))
            {
                htmlToPdfFile.Index += 1;
            }

            HtmlToPdfFile tocHtmlToPdfFile = new HtmlToPdfFile
            {
                Index = tocIndex,

                // TODO: extract wkhtmltopdf specific details
                Input = Path.Combine(Path.GetTempPath(), "__WKANCHOR_2").ToLower(),

                // TODO: localization
                Title = "Table of Contents",
                TitleAndHeadings = new List<HtmlHeading>
                {
                    new HtmlHeading
                    {
                        Level = 0,
                        Page = 0,
                        Text = "Table of Contents"
                    },
                    new HtmlHeading
                    {
                        Level = 1,
                        Page = tocPageNumber,
                        Text = "Table of Contents"
                    }
                }
            };

            htmlToPdfFiles.Add(tocHtmlToPdfFile);

            using (TempHtmlFile tempHtmlFile = new TempHtmlFile())
            {
                string defaultTocXsl = EmbeddedResource.GetDefaultTableOfContentsXsl();
                using (StringReader stringReader = new StringReader(defaultTocXsl))
                {
                    using (XmlReader tocXslXmlReader = XmlReader.Create(stringReader))
                    {
                        XslCompiledTransform xslCompiledTransform = new XslCompiledTransform();
                        xslCompiledTransform.Load(tocXslXmlReader);

                        using (MemoryStream memoryStream = new MemoryStream())
                        {
                            using (XmlWriter xmlWriter = XmlWriter.Create(memoryStream))
                            {
                                outlineBuilder(xmlWriter, htmlToPdfFiles, tableOfContentsIncluded);
                            }

                            // Reset stream position to read from the beginning
                            memoryStream.Seek(0, SeekOrigin.Begin);

                            using (XmlReader xmlReader = XmlReader.Create(memoryStream))
                            {
                                using (XmlWriter xmlWriter = XmlWriter.Create(tempHtmlFile.FilePath))
                                {
                                    xslCompiledTransform.Transform(xmlReader, xmlWriter);
                                }
                            }
                        }
                    }
                }

                // print as pdf
                string pdfFile = await pdfPrinter.PrintAsPdfAsync(
                    tempHtmlFile.FilePath,
                    htmlToPdfOptions,
                    variables,
                    false);

                int numberOfPages = PdfDocument.CountNumberOfPages(pdfFile);

                tocHtmlToPdfFile.PdfFilePath = pdfFile;
                tocHtmlToPdfFile.NumberOfPages = numberOfPages;
            }
        }
    }
}
