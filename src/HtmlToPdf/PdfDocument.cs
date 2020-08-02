// <copyright file="PdfDocument.cs" company="HtmlToPdf">
// Copyright (c) HtmlToPdf. All rights reserved.
// </copyright>

namespace HtmlToPdf
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.IO;
    using System.Linq;
    using System.Web;
    using iText.Kernel.Pdf;
    using iText.Kernel.Pdf.Action;
    using iText.Kernel.Pdf.Annot;
    using iText.Kernel.Pdf.Navigation;

    /// <summary>
    /// PDF Document
    /// </summary>
    internal static class PdfDocument
    {
        /// <summary>
        /// Counts the number of pages.
        /// </summary>
        /// <param name="pdfFilePath">The PDF file path.</param>
        /// <returns>The number of pages.</returns>
        internal static int CountNumberOfPages(string pdfFilePath)
        {
            using (PdfReader pdfReader = new PdfReader(pdfFilePath))
            {
                using (iText.Kernel.Pdf.PdfDocument pdfDocument = new iText.Kernel.Pdf.PdfDocument(pdfReader))
                {
                    return pdfDocument.GetNumberOfPages();
                }
            }
        }

        /// <summary>
        /// Sets the title.
        /// </summary>
        /// <param name="pdfFilePath">The PDF file path.</param>
        /// <param name="title">The title.</param>
        internal static void SetTitle(string pdfFilePath, string title)
        {
            if (string.IsNullOrEmpty(title))
            {
                return;
            }

            string tempFilePath = Path.GetTempFileName();

            using (PdfReader pdfReader = new PdfReader(pdfFilePath))
            {
                using (PdfWriter pdfWriter = new PdfWriter(tempFilePath))
                {
                    using (iText.Kernel.Pdf.PdfDocument pdfDocument = new iText.Kernel.Pdf.PdfDocument(pdfReader, pdfWriter))
                    {
                        PdfDocumentInfo pdfDocumentInfo = pdfDocument.GetDocumentInfo();
                        pdfDocumentInfo.SetTitle(title);
                    }
                }
            }

            File.Delete(pdfFilePath);
            File.Move(tempFilePath, pdfFilePath);
        }

        /// <summary>
        /// Updates the links.
        /// </summary>
        /// <param name="pdfFilePath">The PDF file path.</param>
        /// <param name="htmlToPdfFiles">The HTML to PDF files.</param>
        /// <param name="logger">The logger.</param>
        internal static void UpdateLinks(
            string pdfFilePath,
            IReadOnlyCollection<HtmlToPdfFile> htmlToPdfFiles,
            ILogger logger)
        {
            string tempFilePath = Path.GetTempFileName();

            using (PdfReader pdfReader = new PdfReader(pdfFilePath))
            {
                using (PdfWriter pdfWriter = new PdfWriter(tempFilePath))
                {
                    using (iText.Kernel.Pdf.PdfDocument pdfDocument = new iText.Kernel.Pdf.PdfDocument(pdfReader, pdfWriter))
                    {
                        int pageCount = pdfDocument.GetNumberOfPages();
                        for (int i = 1; i <= pageCount; i++)
                        {
                            // get page
                            PdfPage pdfPage = pdfDocument.GetPage(i);

                            // get link annotations
                            IEnumerable<PdfLinkAnnotation> linkAnnotations = pdfPage.GetAnnotations().OfType<PdfLinkAnnotation>();
                            foreach (PdfLinkAnnotation linkAnnotation in linkAnnotations)
                            {
                                // get action
                                PdfDictionary action = linkAnnotation.GetAction();
                                if (action == null)
                                {
                                    continue;
                                }

                                PdfName s = action.GetAsName(PdfName.S);
                                if (s != PdfName.URI)
                                {
                                    continue;
                                }

                                PdfString uriPdfString = action.GetAsString(PdfName.URI);
                                if (!Uri.TryCreate(uriPdfString.GetValue(), UriKind.RelativeOrAbsolute, out Uri uri))
                                {
                                    continue;
                                }

                                if (!uri.IsFile)
                                {
                                    continue;
                                }

                                string htmlFilePath = uri.LocalPath.ToLower();

                                if (!htmlToPdfFiles.Any(x => string.Compare(x.Input, htmlFilePath, StringComparison.OrdinalIgnoreCase) == 0))
                                {
                                    logger.LogDebug($"Could not find '{htmlFilePath}'. Referenced in '{pdfFilePath}' on page {i}.");
                                    continue;
                                }

                                HtmlToPdfFile linkedHtmlToPdfFile = htmlToPdfFiles.Single(x => x.Input == htmlFilePath);
                                int linkedPageNumber = linkedHtmlToPdfFile.OutputPdfFilePageNumber;

                                // http://api.itextpdf.com/itext/com/itextpdf/text/pdf/PdfDestination.html
                                PdfPage linkedPage = pdfDocument.GetPage(linkedPageNumber);
                                float top = linkedPage.GetPageSize().GetTop();
                                PdfExplicitDestination destination = PdfExplicitDestination.CreateFitH(linkedPage, top);
                                PdfAction newAction = PdfAction.CreateGoTo(destination);

                                linkAnnotation.SetAction(newAction);
                            }
                        }
                    }
                }
            }

            File.Delete(pdfFilePath);
            File.Move(tempFilePath, pdfFilePath);
        }

        /// <summary>
        /// Finds and sets the page numbers of links mapped to HTML headings in the specified PDF file.
        /// </summary>
        /// <param name="htmlToPdfFile">The HTML to PDF file.</param>
        internal static void SetHeadingPageNumbers(HtmlToPdfFile htmlToPdfFile)
        {
            using (PdfReader pdfReader = new PdfReader(htmlToPdfFile.PdfFilePath))
            {
                using (iText.Kernel.Pdf.PdfDocument pdfDocument = new iText.Kernel.Pdf.PdfDocument(pdfReader))
                {
                    int pageCount = pdfDocument.GetNumberOfPages();
                    for (int i = 1; i <= pageCount; i++)
                    {
                        // get page
                        PdfPage pdfPage = pdfDocument.GetPage(i);

                        // get link annotations
                        IEnumerable<PdfLinkAnnotation> linkAnnotations = pdfPage.GetAnnotations().OfType<PdfLinkAnnotation>();
                        foreach (PdfLinkAnnotation linkAnnotation in linkAnnotations)
                        {
                            // get action
                            PdfDictionary action = linkAnnotation.GetAction();
                            if (action == null)
                            {
                                continue;
                            }

                            PdfName s = action.GetAsName(PdfName.S);
                            if (s != PdfName.URI)
                            {
                                continue;
                            }

                            PdfString uriPdfString = action.GetAsString(PdfName.URI);
                            if (!Uri.TryCreate(uriPdfString.GetValue(), UriKind.RelativeOrAbsolute, out Uri uri))
                            {
                                continue;
                            }

                            if (!uri.IsFile)
                            {
                                continue;
                            }

                            // get query string
                            NameValueCollection queryString = HttpUtility.ParseQueryString(uri.Query);

                            // ex. ?headingLevel={level}&headingText
                            string headingLevel = queryString["headingLevel"];
                            if (headingLevel == null)
                            {
                                continue;
                            }

                            if (!int.TryParse(headingLevel, out int level))
                            {
                                continue;
                            }

                            string headingText = queryString["headingText"];
                            if (headingText == null)
                            {
                                continue;
                            }

                            HtmlHeading htmlHeading = htmlToPdfFile.TitleAndHeadings.SingleOrDefault(x => (x.Level == level) && (x.Text == headingText));
                            if (htmlHeading == null)
                            {
                                continue;
                            }

                            htmlHeading.Page = i;
                        }
                    }
                }
            }
        }
    }
}
