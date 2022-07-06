// <copyright file="HeaderTests.cs" company="HtmlToPdf">
// Copyright (c) HtmlToPdf. All rights reserved.
// </copyright>

namespace HtmlToPdfTests
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using DotLiquid;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Pathoschild.NaturalTimeParser.Extensions.DotLiquid;
    using UglyToad.PdfPig.Content;

    /// <summary>
    /// Header Tests.
    /// </summary>
    [TestClass]
    public class HeaderTests
    {
        /// <summary>
        /// Gets or sets the test context.
        /// </summary>
        public TestContext TestContext { get; set; }

        /// <summary>
        /// Asserts that passing header-right with a variable placeholder replaces the variable in the header.
        /// </summary>
        /// <param name="exeFileName">Name of the executable file.</param>
        /// <param name="headerCommandLineArgument">The header command line argument.</param>
        /// <param name="expectedHeaderTextTemplate">The expected header text template.</param>
        [TestMethod]
        [DataRow(HtmlToPdfRunner.HtmlToPdfExe, "[page]", "1", DisplayName = "HtmlToPdf.exe [page]")]
        [DataRow(HtmlToPdfRunner.WkhtmltopdfExe, "[page]", "1", DisplayName = "wkhtmltopdf.exe [page]")]
        [DataRow(HtmlToPdfRunner.HtmlToPdfExe, "[date]", "{{ 'today' | as_date | date:'M/d/yyyy' }}", DisplayName = "HtmlToPdf.exe [date]")]
        [DataRow(HtmlToPdfRunner.WkhtmltopdfExe, "[date]", "{{ 'today' | as_date | date:'M/d/yyyy' }}", DisplayName = "wkhtmltopdf.exe [date]")]
        [DataRow(HtmlToPdfRunner.HtmlToPdfExe, "[title]", "The title of the test page", DisplayName = "HtmlToPdf.exe [title]")]
        [DataRow(HtmlToPdfRunner.WkhtmltopdfExe, "[title]", "The title of the test page", DisplayName = "wkhtmltopdf.exe [title]")]
        [DataRow(HtmlToPdfRunner.HtmlToPdfExe, "[webpage]", "{{url}}", DisplayName = "HtmlToPdf.exe [webpage]")]
        [DataRow(HtmlToPdfRunner.WkhtmltopdfExe, "[webpage]", "{{url}}", DisplayName = "wkhtmltopdf.exe [webpage]")]
        [DataRow(HtmlToPdfRunner.HtmlToPdfExe, "[frompage]", "1", DisplayName = "HtmlToPdf.exe [frompage]")]
        [DataRow(HtmlToPdfRunner.WkhtmltopdfExe, "[frompage]", "1", DisplayName = "wkhtmltopdf.exe [frompage]")]
        [DataRow(HtmlToPdfRunner.HtmlToPdfExe, "[topage]", "1", DisplayName = "HtmlToPdf.exe [topage]")]
        [DataRow(HtmlToPdfRunner.WkhtmltopdfExe, "[topage]", "1", DisplayName = "wkhtmltopdf.exe [topage]")]
        [DataRow(HtmlToPdfRunner.HtmlToPdfExe, "[isodate]", "{{ 'today' | as_date | date:'yyyy-MM-dd' }}", DisplayName = "HtmlToPdf.exe [isodate]")]
        [DataRow(HtmlToPdfRunner.WkhtmltopdfExe, "[isodate]", "{{ 'today' | as_date | date:'yyyy-MM-dd' }}", DisplayName = "wkhtmltopdf.exe [isodate]")]
        [DataRow(HtmlToPdfRunner.HtmlToPdfExe, "[time]", "{{ 'now' | as_date | date:'h:mm:ss tt' }}", DisplayName = "HtmlToPdf.exe [time]")]
        [DataRow(HtmlToPdfRunner.WkhtmltopdfExe, "[time]", "{{ 'now' | as_date | date:'h:mm:ss tt' }}", DisplayName = "wkhtmltopdf.exe [time]")]
        [DataRow(HtmlToPdfRunner.HtmlToPdfExe, "[doctitle]", "The title of the test page", DisplayName = "HtmlToPdf.exe [doctitle]")]
        [DataRow(HtmlToPdfRunner.WkhtmltopdfExe, "[doctitle]", "The title of the test page", DisplayName = "wkhtmltopdf.exe [doctitle]")]
        public void HeaderRight_WithVariable_ReplacesVariableInHeader(
            string exeFileName,
            string headerCommandLineArgument,
            string expectedHeaderTextTemplate)
        {
            HtmlToPdfRunner runner = new HtmlToPdfRunner(exeFileName);

            string html = @"<!DOCTYPE html>
<html>
  <head>
    <title>The title of the test page</title>
  </head>
  <body>
   Test Page
  </body>
</html>";

            using (TempHtmlFile htmlFile = new TempHtmlFile(html))
            {
                Dictionary<string, object> dictionary = new Dictionary<string, object>
                {
                    { "url", htmlFile.FilePath },
                };
                Hash hash = Hash.FromDictionary(dictionary);
                Template.RegisterFilter(typeof(NaturalDateFilter));
                Template template = Template.Parse(expectedHeaderTextTemplate);
                string expectedHeaderText = template.Render(hash);

                using (TempPdfFile pdfFile = new TempPdfFile(this.TestContext))
                {
                    string commandLine = $"--margin-top 5mm --header-right {headerCommandLineArgument} \"{htmlFile.FilePath}\" \"{pdfFile.FilePath}\"";
                    HtmlToPdfRunResult result = runner.Run(commandLine);
                    Assert.AreEqual(0, result.ExitCode, result.Output);

                    using (var pdfDocument = UglyToad.PdfPig.PdfDocument.Open(pdfFile.FilePath))
                    {
                        Assert.AreEqual(1, pdfDocument.NumberOfPages);
                        Page page = pdfDocument.GetPage(1);
                        IEnumerable<Word> words = page.GetWords();
                        Assert.IsTrue(words.Count() >= 3, $"{words.Count()}");
                        Assert.AreEqual("Test Page", $"{words.ElementAt(words.Count() - 2)} {words.ElementAt(words.Count() - 1)}");
                        string headerText = string.Join(" ", words.Take(words.Count() - 2).Select(x => x.Text.ToLower()));

                        if (headerCommandLineArgument == "[time]")
                        {
                            // assert times are within 5 seconds
                            DateTime expectedDateTime = DateTime.ParseExact(expectedHeaderText, "h:mm:ss tt", CultureInfo.InvariantCulture);
                            DateTime actualDateTime = DateTime.ParseExact(headerText, "h:mm:ss tt", CultureInfo.InvariantCulture);
                            DateAssert.AreWithinFiveSeconds(expectedDateTime, actualDateTime);
                        }
                        else
                        {
                            Assert.AreEqual(expectedHeaderText.ToLower(), headerText);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Asserts that passing header with a page placeholder inserts a page number in the header in multiple pages.
        /// </summary>
        /// <param name="exeFileName">Name of the executable file.</param>
        /// <param name="headerCommandLineArgument">The header command line argument.</param>
        [TestMethod]
        [DataRow(HtmlToPdfRunner.HtmlToPdfExe, "--header-left", DisplayName = "HtmlToPdf.exe --header-left")]
        [DataRow(HtmlToPdfRunner.WkhtmltopdfExe, "--header-left", DisplayName = "wkhtmltopdf.exe --header-left")]
        [DataRow(HtmlToPdfRunner.HtmlToPdfExe, "--header-center", DisplayName = "HtmlToPdf.exe --header-center")]
        [DataRow(HtmlToPdfRunner.WkhtmltopdfExe, "--header-center", DisplayName = "wkhtmltopdf.exe --header-center")]
        [DataRow(HtmlToPdfRunner.HtmlToPdfExe, "--header-right", DisplayName = "HtmlToPdf.exe --header-right")]
        [DataRow(HtmlToPdfRunner.WkhtmltopdfExe, "--header-right", DisplayName = "wkhtmltopdf.exe --header-right")]
        public void Header_WithPage_InsertsPageNumberInHeaderInMultiplePages(string exeFileName, string headerCommandLineArgument)
        {
            HtmlToPdfRunner runner = new HtmlToPdfRunner(exeFileName);

            string html1 = @"<!DOCTYPE html>
<html>
  <head>
  </head>
  <body>
   Page 1
  </body>
</html>";

            string html2 = @"<!DOCTYPE html>
<html>
  <head>
  </head>
  <body>
   Page 2
  </body>
</html>";

            using (TempHtmlFile htmlFile1 = new TempHtmlFile(html1))
            {
                using (TempHtmlFile htmlFile2 = new TempHtmlFile(html2))
                {
                    using (TempPdfFile pdfFile = new TempPdfFile(this.TestContext))
                    {
                        string commandLine = $"--margin-top 5mm {headerCommandLineArgument} [page] \"{htmlFile1.FilePath}\" \"{htmlFile2.FilePath}\" \"{pdfFile.FilePath}\"";
                        HtmlToPdfRunResult result = runner.Run(commandLine);
                        Assert.AreEqual(0, result.ExitCode, result.Output);

                        using (var pdfDocument = UglyToad.PdfPig.PdfDocument.Open(pdfFile.FilePath))
                        {
                            Assert.AreEqual(2, pdfDocument.NumberOfPages);
                            Page page1 = pdfDocument.GetPage(1);
                            IEnumerable<Word> words = page1.GetWords();
                            Assert.AreEqual(3, words.Count(), string.Join(" ", words));
                            Assert.AreEqual("1 Page 1", string.Join(" ", words));

                            Page page2 = pdfDocument.GetPage(2);
                            words = page2.GetWords();
                            Assert.AreEqual(3, words.Count());
                            Assert.AreEqual("2 Page 2", string.Join(" ", words));
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Asserts that passing header-right with CSS to hide the header on the first page does not show the header on the first page.
        /// </summary>
        [TestMethod]
        public void HeaderRight_WithCssToHideHeaderOnFirstPage_DoesNotShowHeaderOnFirstPage()
        {
            HtmlToPdfRunner runner = new HtmlToPdfRunner(HtmlToPdfRunner.HtmlToPdfExe);

            string html1 = @"<!DOCTYPE html>
<html>
  <head>
    <style>
        @page {margin-right: 0;}
    </style>
  </head>
  <body>
   Page 1
  </body>
</html>";

            string html2 = @"<!DOCTYPE html>
<html>
  <head>
  </head>
  <body>
   Page 2
  </body>
</html>";

            using (TempHtmlFile htmlFile1 = new TempHtmlFile(html1, this.TestContext))
            {
                using (TempHtmlFile htmlFile2 = new TempHtmlFile(html2, this.TestContext))
                {
                    using (TempPdfFile pdfFile = new TempPdfFile(this.TestContext))
                    {
                        string commandLine = $"--header-right [page] \"{htmlFile1.FilePath}\" \"{htmlFile2.FilePath}\" \"{pdfFile.FilePath}\"";
                        HtmlToPdfRunResult result = runner.Run(commandLine);
                        Assert.AreEqual(0, result.ExitCode, result.Output);

                        using (var pdfDocument = UglyToad.PdfPig.PdfDocument.Open(pdfFile.FilePath))
                        {
                            Assert.AreEqual(2, pdfDocument.NumberOfPages);
                            Page page1 = pdfDocument.GetPage(1);
                            IEnumerable<Word> words = page1.GetWords();
                            Assert.AreEqual(3, words.Count(), string.Join(" ", words));
                            Assert.AreEqual("1 Page 1", string.Join(" ", words));

                            Page page2 = pdfDocument.GetPage(2);
                            words = page2.GetWords();
                            Assert.AreEqual(3, words.Count());
                            Assert.AreEqual("Page 2 2", string.Join(" ", words));
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Asserts that passing header-right with CSS to hide the header on the first page of a mult-page HTML does not show the header on the first page.
        /// </summary>
        [TestMethod]
        public void HeaderRight_WithCssToHideHeaderOnFirstPageOfMultiplePages_DoesNotShowHeaderOnFirstPage()
        {
            HtmlToPdfRunner runner = new HtmlToPdfRunner(HtmlToPdfRunner.HtmlToPdfExe);

            string html = @"<!DOCTYPE html>
<html>
  <head>
    <style>
        @page:first {margin-right: 0;}
    </style>
  </head>
  <body>
   Page 1
   <p style=""page-break-before: always""/>
   Page 2
  </body>
</html>";

            using (TempHtmlFile htmlFile = new TempHtmlFile(html, this.TestContext))
            {
                using (TempPdfFile pdfFile = new TempPdfFile(this.TestContext))
                {
                    string commandLine = $"--header-right [page] \"{htmlFile.FilePath}\" \"{pdfFile.FilePath}\"";
                    HtmlToPdfRunResult result = runner.Run(commandLine);
                    Assert.AreEqual(0, result.ExitCode, result.Output);

                    using (var pdfDocument = UglyToad.PdfPig.PdfDocument.Open(pdfFile.FilePath))
                    {
                        Assert.AreEqual(2, pdfDocument.NumberOfPages);
                        Page page1 = pdfDocument.GetPage(1);
                        IEnumerable<Word> words = page1.GetWords();
                        Assert.AreEqual(3, words.Count(), string.Join(" ", words.Select(x => x.Text)));
                        Assert.AreEqual("1 Page 1", string.Join(" ", words));

                        Page page2 = pdfDocument.GetPage(2);
                        words = page2.GetWords();
                        Assert.AreEqual(3, words.Count(), string.Join(" ", words.Select(x => x.Text)));
                        Assert.AreEqual("2 Page 2", string.Join(" ", words));
                    }
                }
            }
        }
    }
}
