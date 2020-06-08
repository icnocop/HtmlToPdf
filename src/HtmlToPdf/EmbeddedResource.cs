// <copyright file="EmbeddedResource.cs" company="HtmlToPdf">
// Copyright (c) HtmlToPdf. All rights reserved.
// </copyright>

namespace HtmlToPdf
{
    using System.IO;
    using System.Reflection;

    /// <summary>
    /// Embedded Resource
    /// </summary>
    public static class EmbeddedResource
    {
        /// <summary>
        /// Gets the default table of contents XSL.
        /// </summary>
        /// <returns>The contents of the default table of contents XSL.</returns>
        public static string GetDefaultTableOfContentsXsl()
        {
            return ReadAsString("DefaultTableOfContents.xsl");
        }

        /// <summary>
        /// Reads an embedded resource as a string.
        /// </summary>
        /// <param name="resourceName">Name of the resource.</param>
        /// <returns>The embedded resource contents.</returns>
        private static string ReadAsString(string resourceName)
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            string @namespace = assembly.GetName().Name;
            using (Stream stream = assembly.GetManifestResourceStream($"{@namespace}.{resourceName}"))
            {
                if (stream == null)
                {
                    return null;
                }

                byte[] buffer = new byte[stream.Length];
                stream.Read(buffer, 0, buffer.Length);

                using (var memoryStream = new MemoryStream(buffer))
                {
                    using (var streamReader = new StreamReader(memoryStream))
                    {
                        return streamReader.ReadToEnd();
                    }
                }
            }
        }
    }
}
