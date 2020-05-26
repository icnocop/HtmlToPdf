// <copyright file="FooterHtmlTests.cs" company="HtmlToPdf">
// Copyright (c) HtmlToPdf. All rights reserved.
// </copyright>

namespace HtmlToPdfTests
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Net.Sockets;
    using Huygens;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using UglyToad.PdfPig.Content;

    /// <summary>
    /// Footer HTTML Tests
    /// </summary>
    [TestClass]
    public class FooterHtmlTests
    {
        private static readonly Random Random = new Random();

        /// <summary>
        /// Gets or sets the test context.
        /// </summary>
        public TestContext TestContext { get; set; }

        /// <summary>
        /// Asserts that passing footer-html with a local file sets the footer with the contents of the file.
        /// </summary>
        /// <param name="exeFileName">Name of the executable file.</param>
        [TestMethod]
        [DataRow(HtmlToPdfRunner.HtmlToPdfExe, DisplayName = "HtmlToPdf.exe")]
        [DataRow(HtmlToPdfRunner.WkhtmltopdfExe, DisplayName = "wkhtmltopdf.exe")]
        public void FooterHtml_WithLocalFilePath_SetsTheFooterWithContentsOfFile(string exeFileName)
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

            string footerHtml = @"<div>New Footer HTML</div>";

            using (TempHtmlFile htmlFile = new TempHtmlFile(html))
            {
                using (TempHtmlFile htmlFooterFile = new TempHtmlFile(footerHtml))
                {
                    using (TempPdfFile pdfFile = new TempPdfFile(this.TestContext))
                    {
                        string commandLine = $"--footer-html \"{htmlFooterFile.FilePath}\" \"{htmlFile.FilePath}\" \"{pdfFile.FilePath}\"";
                        HtmlToPdfRunResult result = runner.Run(commandLine);
                        Assert.AreEqual(0, result.ExitCode, result.Output);

                        using (var pdfDocument = UglyToad.PdfPig.PdfDocument.Open(pdfFile.FilePath))
                        {
                            Assert.AreEqual(1, pdfDocument.NumberOfPages);
                            Page page1 = pdfDocument.GetPage(1);
                            IEnumerable<Word> words = page1.GetWords();
                            Assert.AreEqual("Page 1", $"{words.ElementAt(0)} {words.ElementAt(1)}");

                            Assert.AreEqual("New Footer HTML", string.Join(" ", words.Skip(2).Select(x => x.Text)));
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Asserts that passing footer-html with a URL sets the footer with the contents of the URL.
        /// </summary>
        /// <param name="exeFileName">Name of the executable file.</param>
        [TestMethod]
        [DataRow(HtmlToPdfRunner.HtmlToPdfExe, DisplayName = "HtmlToPdf.exe")]
        [DataRow(HtmlToPdfRunner.WkhtmltopdfExe, DisplayName = "wkhtmltopdf.exe")]
        public void FooterHtml_WithUrl_SetsTheFooterWithContentsOfUrl(string exeFileName)
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

            string footerHtml = @"<div>New Footer HTML</div>";

            using (TempHtmlFile htmlFile = new TempHtmlFile(html))
            {
                using (var tempDirectory = new TempDirectory())
                {
                    string footerHtmlFileName = "footer.html";
                    string footerHtmlFilePath = Path.Combine(tempDirectory.DirectoryPath, footerHtmlFileName);
                    File.WriteAllText(footerHtmlFilePath, footerHtml);

                    int port = GetAvailablePort();
                    using (var server = new SocketServer(port, "/", tempDirectory.DirectoryPath))
                    {
                        server.Start();

                        string footerHtmlUrl = $"{server.RootUrl}{footerHtmlFileName}";
                        using (TempPdfFile pdfFile = new TempPdfFile(this.TestContext))
                        {
                            string commandLine = $"--footer-html \"{footerHtmlUrl}\" \"{htmlFile.FilePath}\" \"{pdfFile.FilePath}\"";
                            HtmlToPdfRunResult result = runner.Run(commandLine);
                            Assert.AreEqual(0, result.ExitCode, result.Output);

                            using (var pdfDocument = UglyToad.PdfPig.PdfDocument.Open(pdfFile.FilePath))
                            {
                                Assert.AreEqual(1, pdfDocument.NumberOfPages);
                                Page page1 = pdfDocument.GetPage(1);
                                IEnumerable<Word> words = page1.GetWords();
                                Assert.AreEqual("Page 1", $"{words.ElementAt(0)} {words.ElementAt(1)}");

                                Assert.AreEqual("New Footer HTML", string.Join(" ", words.Skip(2).Select(x => x.Text)));
                            }
                        }

                        server.ShutDown();
                    }
                }
            }
        }

        private static int GetAvailablePort()
        {
            return GetAvailablePort(8000, 10000);
        }

        private static int GetAvailablePort(int minPort, int maxPort)
        {
            int port = Random.Next(minPort, maxPort + 1);
            while (!IsPortAvailable(port))
            {
                port = Random.Next(minPort, maxPort + 1);
            }

            return port;
        }

        private static bool IsPortAvailable(int port)
        {
            try
            {
                TcpListener listener = new TcpListener(IPAddress.Loopback, port);
                listener.Start();

                // port is available
                listener.Stop();
                return true;
            }
            catch (SocketException ex) when (ex.SocketErrorCode == SocketError.AddressAlreadyInUse)
            {
                return false;
            }
        }
    }
}
