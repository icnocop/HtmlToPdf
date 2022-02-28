// <copyright file="TableOfContentsTests.cs" company="HtmlToPdf">
// Copyright (c) HtmlToPdf. All rights reserved.
// </copyright>

namespace HtmlToPdfTests
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using UglyToad.PdfPig.Annotations;
    using UglyToad.PdfPig.Content;
    using UglyToad.PdfPig.Graphics.Operations;
    using UglyToad.PdfPig.Graphics.Operations.ClippingPaths;
    using UglyToad.PdfPig.Graphics.Operations.PathConstruction;
    using UglyToad.PdfPig.Graphics.Operations.PathPainting;
    using UglyToad.PdfPig.Tokens;

    /// <summary>
    /// Table Of Contents Tests.
    /// </summary>
    [TestClass]
    public class TableOfContentsTests
    {
        /// <summary>
        /// Gets or sets the test context.
        /// </summary>
        public TestContext TestContext { get; set; }

        /// <summary>
        /// Asserts that passing toc inserts a table of contents as the first page.
        /// </summary>
        /// <param name="exeFileName">Name of the executable file.</param>
        /// <param name="wkhtmltopdf">if set to <c>true</c> indicates the test is with wkhtmltopdf.</param>
        /// <param name="disableDottedLines">if set to <c>true</c> disables dotted lines.</param>
        [TestMethod]
        [DataRow(HtmlToPdfRunner.HtmlToPdfExe, false, false, DisplayName = "HtmlToPdf.exe with dotted lines")]
        [DataRow(HtmlToPdfRunner.HtmlToPdfExe, false, true, DisplayName = "HtmlToPdf.exe without dotted lines")]
        [DataRow(HtmlToPdfRunner.WkhtmltopdfExe, true, false, DisplayName = "wkhtmltopdf.exe with dotted lines")]
        [DataRow(HtmlToPdfRunner.WkhtmltopdfExe, true, true, DisplayName = "wkhtmltopdf.exe without dotted lines")]
        public void TableOfContents_InsertsTableOfContentsAsFirstPage(string exeFileName, bool wkhtmltopdf, bool disableDottedLines)
        {
            HtmlToPdfRunner runner = new HtmlToPdfRunner(exeFileName);

            string html = @"<!DOCTYPE html>
<html>
  <head>
  </head>
  <body>
   Page 2
  </body>
</html>";

            using (TempHtmlFile htmlFile = new TempHtmlFile(html, this.TestContext))
            {
                using (TempPdfFile pdfFile = new TempPdfFile(this.TestContext))
                {
                    using (TempFile tempOutlineFile = new TempFile(".xml", this.TestContext))
                    {
                        string commandLine = $"--dump-outline \"{tempOutlineFile.FilePath}\" toc ";

                        if (disableDottedLines)
                        {
                            commandLine += "--disable-dotted-lines ";
                        }

                        commandLine += $"\"{htmlFile.FilePath}\" \"{pdfFile.FilePath}\"";

                        HtmlToPdfRunResult result = runner.Run(commandLine);
                        Assert.AreEqual(0, result.ExitCode, result.Output);

                        // assert outline
                        using (TempFile expectedOutlineFile = new TempFile(".xml", this.TestContext))
                        {
                            string[] contents = new[]
                            {
                                @"<?xml version=""1.0"" encoding=""UTF-8""?>",
                                @"<outline xmlns=""http://wkhtmltopdf.org/outline"">",
                                @"  <item title=""Table of Contents"" page=""0"" link=""__WKANCHOR_0"" backLink=""__WKANCHOR_0"">",
                                @"    <item title=""Table of Contents"" page=""1"" link=""__WKANCHOR_2"" backLink=""__WKANCHOR_2""/>",
                                @"  </item>",
                                @"  <item title="""" page=""1"" link="""" backLink=""""/>",
                                @"</outline>",
                            };

                            File.WriteAllLines(expectedOutlineFile.FilePath, contents);
                            XmlAssert.AreEqual(expectedOutlineFile.FilePath, tempOutlineFile.FilePath, this.TestContext, $"Expected: {expectedOutlineFile.FileName} Actual: {tempOutlineFile.FileName}");
                        }

                        // assert PDF
                        using (var pdfDocument = UglyToad.PdfPig.PdfDocument.Open(pdfFile.FilePath))
                        {
                            Assert.AreEqual(2, pdfDocument.NumberOfPages);
                            Page tocPage = pdfDocument.GetPage(1);

                            // assert text
                            IEnumerable<Word> words = tocPage.GetWords();
                            Assert.AreEqual(7, words.Count());
                            Assert.AreEqual("Table", words.ElementAt(0).Text);
                            Assert.AreEqual("of", words.ElementAt(1).Text);
                            Assert.AreEqual("Contents", words.ElementAt(2).Text);
                            Assert.AreEqual("Table", words.ElementAt(3).Text);
                            Assert.AreEqual("of", words.ElementAt(4).Text);
                            Assert.AreEqual("Contents", words.ElementAt(5).Text);
                            Assert.AreEqual("1", words.ElementAt(6).Text);

                            // assert link
                            IEnumerable<Annotation> annotations = tocPage.ExperimentalAccess.GetAnnotations();
                            Assert.AreEqual(1, annotations.Count());
                            Annotation annotation = annotations.ElementAt(0);
                            Assert.AreEqual(AnnotationType.Link, annotation.Type);

                            string errorMessage = "Annotation dictionary:" + Environment.NewLine + string.Join(
                                Environment.NewLine,
                                annotation.AnnotationDictionary.Data.Select(x => $"{x.Key} {x.Value.GetType()} {x.Value}"));

                            Assert.IsTrue(annotation.AnnotationDictionary.TryGet(NameToken.Type, out NameToken typeToken), errorMessage);
                            Assert.AreEqual("Annot", typeToken.Data);

                            Assert.IsTrue(annotation.AnnotationDictionary.TryGet(NameToken.Subtype, out NameToken subtypeToken), errorMessage);
                            Assert.AreEqual("Link", subtypeToken.Data);

                            if (wkhtmltopdf)
                            {
                                Assert.IsTrue(annotation.AnnotationDictionary.TryGet(NameToken.Dest, out NameToken destinationToken), errorMessage);
                                Assert.AreEqual("__WKANCHOR_2", destinationToken.Data);
                            }
                            else
                            {
                                Assert.IsTrue(annotation.AnnotationDictionary.TryGet(NameToken.A, out IndirectReferenceToken aToken), errorMessage);
                                Assert.AreEqual(27, aToken.Data.ObjectNumber);

                                Assert.IsTrue(annotation.AnnotationDictionary.TryGet(NameToken.P, out IndirectReferenceToken pToken), errorMessage);
                                Assert.AreEqual(16, pToken.Data.ObjectNumber);
                            }

                            // assert graphic operations
                            IReadOnlyList<IGraphicsStateOperation> graphicsStateOperations = tocPage.Operations;

                            errorMessage = "Graphics State Operations:" + Environment.NewLine + string.Join(
                                Environment.NewLine,
                                graphicsStateOperations.Select(x => $"{x.Operator} {x.GetType()} {x}"));

                            if (wkhtmltopdf)
                            {
                                Assert.AreEqual(!disableDottedLines, graphicsStateOperations.Any(x => x is ModifyClippingByNonZeroWindingIntersect), errorMessage);
                                Assert.AreEqual(!disableDottedLines, graphicsStateOperations.Any(x => x is AppendRectangle), errorMessage);
                                Assert.AreEqual(!disableDottedLines, graphicsStateOperations.Any(x => x is FillPathNonZeroWinding), errorMessage);
                            }
                            else
                            {
                                Assert.AreEqual(!disableDottedLines, graphicsStateOperations.Any(x => x is AppendStraightLineSegment), errorMessage);
                                Assert.AreEqual(!disableDottedLines, graphicsStateOperations.Any(x => x is BeginNewSubpath), errorMessage);
                            }

                            // assert page 2
                            Page page2 = pdfDocument.GetPage(2);
                            Assert.AreEqual("Page 2", page2.Text);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Asserts that not passing toc doesn't insert a table of contents.
        /// </summary>
        /// <param name="exeFileName">Name of the executable file.</param>
        [TestMethod]
        [DataRow(HtmlToPdfRunner.HtmlToPdfExe, DisplayName = "HtmlToPdf.exe")]
        [DataRow(HtmlToPdfRunner.WkhtmltopdfExe, DisplayName = "wkhtmltopdf.exe")]
        public void WithoutTableOfContents_DoesNotInsertTableOfContentsPage(string exeFileName)
        {
            HtmlToPdfRunner runner = new HtmlToPdfRunner(exeFileName);

            string html = @"<!DOCTYPE html>
<html>
  <head>
  </head>
  <body>
   Page 1
  </body>
</html>";

            using (TempHtmlFile htmlFile = new TempHtmlFile(html))
            {
                using (TempPdfFile pdfFile = new TempPdfFile(this.TestContext))
                {
                    string commandLine = $"\"{htmlFile.FilePath}\" \"{pdfFile.FilePath}\"";
                    HtmlToPdfRunResult result = runner.Run(commandLine);
                    Assert.AreEqual(0, result.ExitCode, result.Output);

                    using (var pdfDocument = UglyToad.PdfPig.PdfDocument.Open(pdfFile.FilePath))
                    {
                        Assert.AreEqual(1, pdfDocument.NumberOfPages);
                        Page page1 = pdfDocument.GetPage(1);
                        Assert.AreEqual("Page 1", page1.Text);
                    }
                }
            }
        }
    }
}
