// <copyright file="PdfMerger.cs" company="HtmlToPdf">
// Copyright (c) HtmlToPdf. All rights reserved.
// </copyright>

namespace HtmlToPdf
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;

    /// <summary>
    /// PDF Merge
    /// </summary>
    internal class PdfMerger
    {
        /// <summary>
        /// Merges the specified PDF files.
        /// </summary>
        /// <param name="pdfFiles">The PDF files.</param>
        /// <returns>A byte array.</returns>
        internal static byte[] Merge(IEnumerable<string> pdfFiles)
        {
            if (pdfFiles.Count() == 1)
            {
                return File.ReadAllBytes(pdfFiles.Single());
            }
            else
            {
                List<byte[]> bytes = new List<byte[]>();

                foreach (string pdfFile in pdfFiles)
                {
                    byte[] fileBytes = File.ReadAllBytes(pdfFile);
                    if (fileBytes.Length == 0)
                    {
                        throw new Exception($"{pdfFile} contains 0bytes.");
                    }

                    bytes.Add(fileBytes);

                    // delete pdf file
                    File.Delete(pdfFile);
                }

                return UglyToad.PdfPig.Writer.PdfMerger.Merge(bytes);
            }
        }
    }
}
