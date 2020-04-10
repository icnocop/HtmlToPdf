// <copyright file="UpdateLinksTests.cs" company="HtmlToPdf">
// Copyright (c) HtmlToPdf. All rights reserved.
// </copyright>

namespace HtmlToPdfTests
{
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using UglyToad.PdfPig.Annotations;
    using UglyToad.PdfPig.Content;
    using UglyToad.PdfPig.Core;
    using UglyToad.PdfPig.Tokens;

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
        [TestMethod]
        public void OneFile_LinksToTheNextFile_UpdatesLink()
        {
            HtmlToPdfRunner runner = new HtmlToPdfRunner();

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

                        using (var pdfDocument = UglyToad.PdfPig.PdfDocument.Open(pdfFile.FilePath))
                        {
                            Assert.AreEqual(2, pdfDocument.NumberOfPages);
                            Page page1 = pdfDocument.GetPage(1);

                            // get link
                            IEnumerable<Annotation> annotations = page1.ExperimentalAccess.GetAnnotations();
                            Assert.AreEqual(1, annotations.Count());
                            Annotation annotation = annotations.ElementAt(0);
                            Assert.AreEqual(AnnotationType.Link, annotation.Type);

                            // get indirect reference
                            Assert.IsTrue(annotation.AnnotationDictionary.TryGet(NameToken.A, out IndirectReferenceToken indirectReferenceToken));
                            IndirectReference indirectReference = indirectReferenceToken.Data;

                            // get GoTo
                            long value = pdfDocument.Structure.CrossReferenceTable.ObjectOffsets[indirectReference];
                            Assert.AreEqual(1872, value);
                        }
                    }
                }
            }
        }
    }
}
