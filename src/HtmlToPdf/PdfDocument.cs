// <copyright file="PdfDocument.cs" company="HtmlToPdf">
// Copyright (c) HtmlToPdf. All rights reserved.
// </copyright>

namespace HtmlToPdf
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
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
        /// Updates the links.
        /// </summary>
        /// <param name="pdfFilePath">The PDF file path.</param>
        /// <param name="htmlToPdfFiles">The HTML to PDF files.</param>
        internal static void UpdateLinks(string pdfFilePath, ConcurrentDictionary<string, HtmlToPdfFile> htmlToPdfFiles)
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

                                string htmlFilePath = uri.LocalPath;

                                if (!htmlToPdfFiles.ContainsKey(htmlFilePath))
                                {
                                    Logger.LogError($"WARN: Could not find '{htmlFilePath}'.");
                                    continue;
                                }

                                HtmlToPdfFile linkedHtmlToPdfFile = htmlToPdfFiles[htmlFilePath];
                                int linkedPageNumber = linkedHtmlToPdfFile.OutputPdfFilePageNumber;

                                // please go to http://api.itextpdf.com/itext/com/itextpdf/text/pdf/PdfDestination.html to find the detail.
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
    }
}
