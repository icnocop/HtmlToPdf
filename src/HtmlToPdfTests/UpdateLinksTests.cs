// <copyright file="UpdateLinksTests.cs" company="HtmlToPdf">
// Copyright (c) HtmlToPdf. All rights reserved.
// </copyright>

namespace HtmlToPdfTests
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Web;
    using iText.Kernel.Pdf;
    using iText.Kernel.Pdf.Annot;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>
    /// Update Links Tests
    /// </summary>
    [TestClass]
    public class UpdateLinksTests
    {
        /// <summary>
        /// Gets or sets the test context.
        /// </summary>
        public TestContext TestContext { get; set; }

        /// <summary>
        /// Asserts that the link in one file which links to the next file is updated.
        /// </summary>
        /// <param name="exeFileName">Name of the executable file.</param>
        [TestMethod]
        [DataRow(HtmlToPdfRunner.HtmlToPdfExe, DisplayName = "HtmlToPdf.exe")]
        [DataRow(HtmlToPdfRunner.WkhtmltopdfExe, DisplayName = "wkhtmltopdf.exe")]
        public void OneFile_LinksToTheNextFile_UpdatesLink(string exeFileName)
        {
            HtmlToPdfRunner runner = new HtmlToPdfRunner(exeFileName);

            string htmlFile2Contents = @"
<html>
  <head>
  </head>
  <body>
   Page 2
  </body>
</html>";

            using (TempHtmlFile htmlFile2 = new TempHtmlFile(htmlFile2Contents))
            {
                string htmlFile1Contents = $@"
<html>
  <head>
  </head>
  <body>
   Page 1
   <br/>
   <a href=""{htmlFile2.FilePath}"">Page 2</a>
  </body>
</html>";
                using (TempHtmlFile htmlFile1 = new TempHtmlFile(htmlFile1Contents))
                {
                    using (TempPdfFile pdfFile = new TempPdfFile(this.TestContext))
                    {
                        string commandLine = $"\"{htmlFile1.FilePath}\" \"{htmlFile2.FilePath}\" \"{pdfFile.FilePath}\"";
                        HtmlToPdfRunResult result = runner.Run(commandLine);
                        Assert.AreEqual(0, result.ExitCode, result.Output);

                        using (PdfReader pdfReader = new PdfReader(pdfFile.FilePath))
                        {
                            using (PdfDocument pdfDocument = new PdfDocument(pdfReader))
                            {
                                Assert.AreEqual(2, pdfDocument.GetNumberOfPages());

                                // get the first page
                                PdfPage pdfPage = pdfDocument.GetPage(1);

                                // get link annotations
                                List<PdfLinkAnnotation> linkAnnotations = pdfPage.GetAnnotations().OfType<PdfLinkAnnotation>().ToList();
                                Assert.AreEqual(1, linkAnnotations.Count);

                                // get the first link annotation
                                PdfLinkAnnotation linkAnnotation = linkAnnotations.ElementAt(0);
                                Assert.IsNotNull(linkAnnotation);

                                // get action
                                PdfDictionary action = linkAnnotation.GetAction();
                                Assert.IsNotNull(action);

                                // get GoTo sub-type
                                PdfName s = action.GetAsName(PdfName.S);

                                if (exeFileName == HtmlToPdfRunner.HtmlToPdfExe)
                                {
                                    Assert.AreEqual(PdfName.GoTo, s);

                                    // get destination
                                    PdfArray destination = action.GetAsArray(PdfName.D);
                                    PdfIndirectReference destinationPageReference = destination.GetAsDictionary(0).GetIndirectReference();
                                    PdfName zoom = destination.GetAsName(1);
                                    PdfNumber pageOffset = destination.GetAsNumber(2);

                                    // get expected values
                                    PdfPage pdfPage2 = pdfDocument.GetPage(2);
                                    PdfDictionary page2Dictionary = pdfPage2.GetPdfObject();
                                    PdfIndirectReference expectedPageReference = page2Dictionary.GetIndirectReference();
                                    PdfName expectedZoom = PdfName.FitH;
                                    float expectedPageOffset = pdfPage2.GetPageSize().GetTop();

                                    // assert
                                    Assert.AreEqual(expectedPageReference, destinationPageReference);
                                    Assert.AreEqual(expectedZoom, zoom);
                                    Assert.AreEqual(expectedPageOffset, pageOffset.FloatValue());
                                }
                                else if (exeFileName == HtmlToPdfRunner.WkhtmltopdfExe)
                                {
                                    Assert.AreEqual(PdfName.URI, s);

                                    PdfString uri = action.GetAsString(PdfName.URI);
                                    Assert.AreEqual(htmlFile2.FilePath, HttpUtility.UrlDecode(uri.ToString()));
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}
