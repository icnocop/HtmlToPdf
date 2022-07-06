// <copyright file="AdditionalArgumentsTests.cs" company="HtmlToPdf">
// Copyright (c) HtmlToPdf. All rights reserved.
// </copyright>

namespace HtmlToPdfTests
{
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using UglyToad.PdfPig.Content;

    /// <summary>
    /// Additional arguments tests.
    /// </summary>
    [TestClass]
    public class AdditionalArgumentsTests
    {
        /// <summary>
        /// Gets or sets the test context.
        /// </summary>
        public TestContext TestContext { get; set; }

        /// <summary>
        /// Asserts that passing additional arguments are passed to the browser instance.
        /// </summary>
        [TestMethod]
        public void AdditionalArgumentsTest()
        {
            HtmlToPdfRunner runner = new HtmlToPdfRunner(HtmlToPdfRunner.HtmlToPdfExe);

            string html = @"<!DOCTYPE html>
<html>
  <head>
  </head>
  <body>
    <p id=""userAgent""></p>

    <script>
    let agent = navigator.userAgent;
    document.getElementById(""userAgent"").innerHTML = ""User-agent:<br>"" + agent;
    </script>
  </body>
</html>";

            using (TempHtmlFile htmlFile = new TempHtmlFile(html))
            {
                using (TempPdfFile pdfFile = new TempPdfFile(this.TestContext))
                {
                    string commandLine = $"--additional-arguments=\"--user-agent=MyCustomUserAgent\" \"{htmlFile.FilePath}\" \"{pdfFile.FilePath}\"";
                    HtmlToPdfRunResult result = runner.Run(commandLine);
                    Assert.AreEqual(0, result.ExitCode, result.Output);

                    using (var pdfDocument = UglyToad.PdfPig.PdfDocument.Open(pdfFile.FilePath))
                    {
                        Assert.AreEqual(1, pdfDocument.NumberOfPages);
                        Page page = pdfDocument.GetPage(1);
                        IEnumerable<Word> words = page.GetWords();
                        Assert.AreEqual("User-agent: MyCustomUserAgent", string.Join(" ", words));
                    }
                }
            }
        }
    }
}
