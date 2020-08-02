// <copyright file="XmlAssert.cs" company="HtmlToPdf">
// Copyright (c) HtmlToPdf. All rights reserved.
// </copyright>

namespace HtmlToPdfTests
{
    using System.IO;
    using System.Reflection;
    using System.Xml;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Microsoft.XmlDiffPatch;

    /// <summary>
    /// XML Assert
    /// </summary>
    internal static class XmlAssert
    {
        /// <summary>
        /// Asserts the values are equal.
        /// </summary>
        /// <param name="expectedXmlFilePath">The expected XML file path.</param>
        /// <param name="actualXmlFilePath">The actual XML file path.</param>
        /// <param name="testContext">The test context.</param>
        /// <param name="message">The message.</param>
        public static void AreEqual(string expectedXmlFilePath, string actualXmlFilePath, TestContext testContext, string message)
        {
            using (TempFile diffGramFile = new TempFile(".xml"))
            {
                bool identical;

                using (XmlWriter diffGramWriter = XmlWriter.Create(diffGramFile.FilePath))
                {
                    XmlDiff xmldiff = new XmlDiff(XmlDiffOptions.IgnoreChildOrder
                        | XmlDiffOptions.IgnoreNamespaces
                        | XmlDiffOptions.IgnorePrefixes);
                    identical = xmldiff.Compare(expectedXmlFilePath, actualXmlFilePath, false, diffGramWriter);
                }

                if (!identical)
                {
                    // generate visual diff
                    using (TempHtmlFile diffGramHtmlFile = new TempHtmlFile(null, testContext))
                    {
                        XmlDiffView xmlDiffView = new XmlDiffView();
                        using (XmlTextReader sourceXml = new XmlTextReader(expectedXmlFilePath))
                        {
                            using (XmlTextReader diffGram = new XmlTextReader(diffGramFile.FilePath))
                            {
                                xmlDiffView.Load(sourceXml, diffGram);
                                using (StreamWriter streamWriter = new StreamWriter(diffGramHtmlFile.FilePath))
                                {
                                    WriteHeaderForXmlDiffGram(expectedXmlFilePath, diffGramFile.FilePath, streamWriter);
                                    xmlDiffView.GetHtml(streamWriter);
                                    streamWriter.Write("</body></html>");
                                }
                            }
                        }

                        message += " Diff: " + diffGramHtmlFile.FileName;
                    }
                }

                Assert.IsTrue(identical, message);
            }
        }

        /// <summary>
        /// Writes the HTML header for XML DiffGram.
        /// </summary>
        /// <param name="sourceXmlFile">name of baseline XML data</param>
        /// <param name="changedXmlFile">name of file being compared</param>
        /// <param name="resultHtml">Output file</param>
        private static void WriteHeaderForXmlDiffGram(
            string sourceXmlFile,
            string changedXmlFile,
            TextWriter resultHtml)
        {
            // this initializes the html
            resultHtml.WriteLine("<html><head>");
            resultHtml.WriteLine("<style TYPE='text/css'>");
            resultHtml.WriteLine(GetEmbeddedString($"{nameof(HtmlToPdfTests)}.Resources.XmlReportStyles.css"));
            resultHtml.WriteLine("</style>");
            resultHtml.WriteLine("</head>");
            resultHtml.WriteLine(GetEmbeddedString($"{nameof(HtmlToPdfTests)}.Resources.XmlDiffHeader.html"));

            resultHtml.WriteLine(
                $"<tr><td></td><td title='{Path.GetDirectoryName(sourceXmlFile)}'><b> File in editor : {Path.GetFileName(sourceXmlFile)}</b></td>" +
                $"  <td title='{Path.GetDirectoryName(changedXmlFile)}'><b> File to compare : {Path.GetFileName(changedXmlFile)}</b></td>" +
                $"</tr>");
        }

        private static string GetEmbeddedString(string name)
        {
            using (Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(name))
            {
                StreamReader sr = new StreamReader(stream);
                return sr.ReadToEnd();
            }
        }
    }
}
