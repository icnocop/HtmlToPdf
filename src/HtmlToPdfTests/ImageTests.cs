// <copyright file="ImageTests.cs" company="HtmlToPdf">
// Copyright (c) HtmlToPdf. All rights reserved.
// </copyright>

namespace HtmlToPdfTests
{
    using System.Collections.Generic;
    using System.Drawing;
    using System.IO;
    using System.Linq;
    using iText.IO.Image;
    using iText.Kernel.Pdf;
    using iText.Kernel.Pdf.Canvas.Parser;
    using iText.Kernel.Pdf.Canvas.Parser.Data;
    using iText.Kernel.Pdf.Canvas.Parser.Listener;
    using iText.Kernel.Pdf.Xobject;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Tesseract;

    /// <summary>
    /// Image Tests
    /// </summary>
    [TestClass]
    public class ImageTests
    {
        /// <summary>
        /// Gets or sets the test context.
        /// </summary>
        public TestContext TestContext { get; set; }

        /// <summary>
        /// Asserts that passing an input file referencing a relative image file path exists in the PDF.
        /// </summary>
        [TestMethod]
        public void Input_WithRelativeImageFilePath_ExistsInPdf()
        {
            HtmlToPdfRunner runner = new HtmlToPdfRunner(HtmlToPdfRunner.HtmlToPdfExe);

            using (TempDirectory tempDirectory = new TempDirectory())
            {
                // create image
                string expectedTextInImage = "test";

                using (TempImageFile tempImageFile = new TempImageFile(expectedTextInImage, this.TestContext))
                {
                    tempImageFile.MoveToDirectory(tempDirectory.DirectoryPath);

                    // create HTML file
                    string html = $@"<!DOCTYPE html>
<html>
  <head>
  </head>
  <body>
   <img src=""{tempImageFile.FileName}"" />
  </body>
</html>";

                    using (TempHtmlFile htmlFile = new TempHtmlFile(html))
                    {
                        htmlFile.MoveToDirectory(tempDirectory.DirectoryPath);

                        using (TempPdfFile pdfFile = new TempPdfFile(this.TestContext))
                        {
                            string commandLine = $"\"{htmlFile.FilePath}\" \"{pdfFile.FilePath}\"";
                            HtmlToPdfRunResult result = runner.Run(commandLine);
                            Assert.AreEqual(0, result.ExitCode, result.Output);

                            using (PdfReader pdfReader = new PdfReader(pdfFile.FilePath))
                            {
                                using (PdfDocument pdfDocument = new PdfDocument(pdfReader))
                                {
                                    int pageCount = pdfDocument.GetNumberOfPages();
                                    Assert.AreEqual(1, pageCount);

                                    PdfPage page = pdfDocument.GetPage(1);

                                    ImageFinder imageFinder = new ImageFinder();
                                    PdfCanvasProcessor processor = new PdfCanvasProcessor(imageFinder);
                                    processor.ProcessPageContent(page);

                                    PdfImage[] images = imageFinder.GetImages().ToArray();
                                    Assert.AreEqual(1, images.Length);
                                    PdfImage pdfImage = images[0];

                                    Assert.AreEqual(ImageType.PNG, pdfImage.ImageType);
                                    using (TempFile bitmapFile = new TempFile(pdfImage.FileExtension, this.TestContext))
                                    {
                                        File.WriteAllBytes(bitmapFile.FilePath, pdfImage.Bytes);

                                        using (Image image = Image.FromFile(bitmapFile.FilePath))
                                        {
                                            string tessDataPath = Path.Combine(
                                                Directory.GetCurrentDirectory(),
                                                "tessdata");
                                            using (var ocrEngine = new TesseractEngine(
                                                tessDataPath + "\\",
                                                "eng",
                                                EngineMode.Default))
                                            {
                                                using (Page ocrPage = ocrEngine.Process(image as Bitmap, PageSegMode.SingleWord))
                                                {
                                                    var actualTextInImage = ocrPage.GetText().Trim();

                                                    Assert.AreEqual(expectedTextInImage, actualTextInImage);
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        private class ImageFinder : IEventListener
        {
            private readonly List<PdfImage> images;

            public ImageFinder()
            {
                this.images = new List<PdfImage>();
            }

            public void EventOccurred(IEventData data, EventType type)
            {
                if (data is ImageRenderInfo imageRenderInfo)
                {
                    PdfImageXObject image = imageRenderInfo.GetImage();
                    if (image == null)
                    {
                        return;
                    }

                    ImageType imageType = image.IdentifyImageType();
                    string fileExtension = $".{image.IdentifyImageFileExtension()}";
                    byte[] imageBytes = image.GetImageBytes(true);

                    this.images.Add(new PdfImage
                    {
                        ImageType = imageType,
                        FileExtension = fileExtension,
                        Bytes = imageBytes
                    });
                }
            }

            public ICollection<EventType> GetSupportedEvents()
            {
                return null;
            }

            internal ICollection<PdfImage> GetImages()
            {
                return this.images;
            }
        }

        private class PdfImage
        {
            public ImageType ImageType { get; internal set; }

            public byte[] Bytes { get; internal set; }

            public string FileExtension { get; internal set; }
        }
    }
}
