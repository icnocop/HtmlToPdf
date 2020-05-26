// <copyright file="BackgroundTests.cs" company="HtmlToPdf">
// Copyright (c) HtmlToPdf. All rights reserved.
// </copyright>

namespace HtmlToPdfTests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using iText.Kernel.Colors;
    using iText.Kernel.Geom;
    using iText.Kernel.Pdf;
    using iText.Kernel.Pdf.Canvas.Parser;
    using iText.Kernel.Pdf.Canvas.Parser.Data;
    using iText.Kernel.Pdf.Canvas.Parser.Listener;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>
    /// Background Tests
    /// </summary>
    [TestClass]
    public class BackgroundTests
    {
        /// <summary>
        /// Gets or sets the test context.
        /// </summary>
        public TestContext TestContext { get; set; }

        /// <summary>
        /// Asserts that passing a background value determines whether or not to print the background.
        /// </summary>
        /// <param name="exeFileName">Name of the executable file.</param>
        /// <param name="commandLineParameter">The command line parameter.</param>
        /// <param name="expectedBackground">if set to <c>true</c> a background is expected.</param>
        [TestMethod]
        [DataRow(HtmlToPdfRunner.HtmlToPdfExe, null, true, DisplayName = "HtmlToPdf.exe Default")]
        [DataRow(HtmlToPdfRunner.WkhtmltopdfExe, null, true, DisplayName = "wkhtmltopdf.exe Default")]
        [DataRow(HtmlToPdfRunner.HtmlToPdfExe, "--background", true, DisplayName = "HtmlToPdf.exe Background")]
        [DataRow(HtmlToPdfRunner.WkhtmltopdfExe, "--background", true, DisplayName = "wkhtmltopdf.exe Background")]
        [DataRow(HtmlToPdfRunner.HtmlToPdfExe, "--no-background", false, DisplayName = "HtmlToPdf.exe No Background")]
        [DataRow(HtmlToPdfRunner.WkhtmltopdfExe, "--no-background", false, DisplayName = "wkhtmltopdf.exe No Background")]
        public void BackgroundTest(string exeFileName, string commandLineParameter, bool expectedBackground)
        {
            HtmlToPdfRunner runner = new HtmlToPdfRunner(exeFileName);

            string html = @"<!DOCTYPE html>
<html>
  <head>
  </head>
  <body style=""background-color:blue;"">
   Test Page
  </body>
</html>";

            using (TempHtmlFile htmlFile = new TempHtmlFile(html))
            {
                using (TempPdfFile pdfFile = new TempPdfFile(this.TestContext))
                {
                    string commandLine = string.Empty;

                    if (!string.IsNullOrEmpty(commandLineParameter))
                    {
                        commandLine += $"{commandLineParameter} ";
                    }

                    commandLine += $"\"{htmlFile.FilePath}\" \"{pdfFile.FilePath}\"";
                    HtmlToPdfRunResult result = runner.Run(commandLine);
                    Assert.AreEqual(0, result.ExitCode, result.Output);

                    using (PdfReader pdfReader = new PdfReader(pdfFile.FilePath))
                    {
                        using (PdfDocument pdfDocument = new PdfDocument(pdfReader))
                        {
                            int pageCount = pdfDocument.GetNumberOfPages();
                            Assert.AreEqual(1, pageCount);

                            PdfPage page = pdfDocument.GetPage(1);

                            RectangleFinder rectangleFinder = new RectangleFinder();

                            PdfCanvasProcessor processor = new PdfCanvasProcessor(rectangleFinder);
                            processor.ProcessPageContent(page);

                            ICollection<Rectangle> boxes = rectangleFinder.GetBoundingBoxes();

                            Assert.AreEqual(expectedBackground ? 1 : 0, boxes.Count());
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Rectangle Finder
        /// </summary>
        /// <seealso cref="iText.Kernel.Pdf.Canvas.Parser.Listener.IEventListener" />
        public class RectangleFinder : IEventListener
        {
            private readonly Dictionary<Line, int> knownLines = new Dictionary<Line, int>();
            private readonly Dictionary<int, int> clusters = new Dictionary<int, int>();

            /// <summary>
            /// Called when some event occurs during parsing a content stream.
            /// </summary>
            /// <param name="data">Combines the data required for processing corresponding event type.</param>
            /// <param name="type">Event type.</param>
            public void EventOccurred(IEventData data, EventType type)
            {
                if (data is PathRenderInfo pathRenderInfo)
                {
                    pathRenderInfo.PreserveGraphicsState();

                    if (pathRenderInfo.GetOperation() == PathRenderInfo.NO_OP)
                    {
                        return;
                    }

                    if (pathRenderInfo.GetOperation() != PathRenderInfo.FILL)
                    {
                        return;
                    }

                    if (!this.IsBlue(pathRenderInfo.GetFillColor()))
                    {
                        return;
                    }

                    Path path = pathRenderInfo.GetPath();
                    foreach (Subpath sPath in path.GetSubpaths())
                    {
                        foreach (IShape segment in sPath.GetSegments())
                        {
                            if (segment is Line)
                            {
                                this.LineOccurred((Line)segment);
                            }
                        }
                    }
                }
            }

            /// <summary>
            /// Provides the set of event types this listener supports.
            /// </summary>
            /// <returns>
            /// Set of event types supported by this listener or
            /// null if all possible event types are supported.
            /// </returns>
            /// <remarks>
            /// Provides the set of event types this listener supports.
            /// Returns null if all possible event types are supported.
            /// </remarks>
            public ICollection<EventType> GetSupportedEvents()
            {
                return null;
            }

            /// <summary>
            /// Gets the clusters.
            /// </summary>
            /// <returns>The clusters.</returns>
            public ICollection<ICollection<Line>> GetClusters()
            {
                Dictionary<int, ICollection<Line>> result = new Dictionary<int, ICollection<Line>>();

                foreach (int val in this.clusters.Values)
                {
                    if (!result.ContainsKey(val))
                    {
                        result.Add(val, new HashSet<Line>());
                    }
                    else
                    {
                        result[val] = new HashSet<Line>();
                    }
                }

                result.Add(-1, new HashSet<Line>());

                foreach (Line l in this.knownLines.Keys)
                {
                    int clusterID = this.clusters.ContainsKey(this.knownLines[l]) ? this.clusters[this.knownLines[l]] : -1;
                    result[clusterID].Add(l);
                }

                result.Remove(-1);

                return result.Values;
            }

            /// <summary>
            /// Gets the bounding boxes.
            /// </summary>
            /// <returns>The bounding boxes.</returns>
            public ICollection<Rectangle> GetBoundingBoxes()
            {
                ICollection<Rectangle> rectangles = new HashSet<Rectangle>();
                foreach (ICollection<Line> cluster in this.GetClusters())
                {
                    double minX = double.MaxValue;
                    double minY = double.MaxValue;
                    double maxX = -double.MaxValue;
                    double maxY = -double.MaxValue;
                    foreach (Line l in cluster)
                    {
                        foreach (Point p in l.GetBasePoints())
                        {
                            minX = Math.Min(minX, p.x);
                            minY = Math.Min(minY, p.y);
                            maxX = Math.Max(maxX, p.x);
                            maxY = Math.Max(maxY, p.y);
                        }
                    }

                    double w = maxX - minX;
                    double h = maxY - minY;
                    rectangles.Add(new Rectangle((float)minX, (float)minY, (float)w, (float)h));
                }

                return rectangles;
            }

            private bool IsBlue(Color c)
            {
                if (c is DeviceRgb col02)
                {
                    return col02.GetNumberOfComponents() == 3
                        && col02.GetColorValue()[0] == 0.0f
                        && col02.GetColorValue()[1] == 0.0f
                        && col02.GetColorValue()[2] == 1.0f;
                }

                return false;
            }

            private void LineOccurred(Line line)
            {
                int id;
                if (!this.knownLines.ContainsKey(line))
                {
                    id = this.knownLines.Count();
                    this.knownLines.Add(line, id);
                }
                else
                {
                    id = this.knownLines[line];
                }

                Point start = line.GetBasePoints()[0];
                Point end = line.GetBasePoints()[1];
                foreach (Line line2 in this.knownLines.Keys)
                {
                    if (line.Equals(line2))
                    {
                        continue;
                    }

                    if (line2.GetBasePoints()[0].Equals(start)
                        || line2.GetBasePoints()[1].Equals(end)
                        || line2.GetBasePoints()[0].Equals(end)
                        || line2.GetBasePoints()[1].Equals(start))
                    {
                        int id2 = this.Find(this.knownLines[line2]);
                        this.clusters.Add(id, id2);
                        break;
                    }
                }
            }

            private int Find(int id)
            {
                int result = id;
                while (this.clusters.ContainsKey(result))
                {
                    result = this.clusters[result];
                }

                return result;
            }
        }
    }
}
