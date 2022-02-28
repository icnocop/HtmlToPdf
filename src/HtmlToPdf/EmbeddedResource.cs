// <copyright file="EmbeddedResource.cs" company="HtmlToPdf">
// Copyright (c) HtmlToPdf. All rights reserved.
// </copyright>

namespace HtmlToPdf
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Reflection;

    /// <summary>
    /// Embedded Resource.
    /// </summary>
    public static class EmbeddedResource
    {
        /// <summary>
        /// Gets the command line pre options.
        /// </summary>
        /// <returns>The command line pre options.</returns>
        public static IEnumerable<string> GetCommandLinePreOptions()
        {
            return ReadLines("CommandLinePreOptions.txt");
        }

        /// <summary>
        /// Gets the command line post options.
        /// </summary>
        /// <returns>The command line post options.</returns>
        public static IEnumerable<string> GetCommandLinePostOptions()
        {
            return ReadLines("CommandLinePostOptions.txt");
        }

        private static IEnumerable<string> ReadLines(string resourceName)
        {
            Assembly assembly = Assembly.GetEntryAssembly();
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
                        return streamReader.ReadToEnd().Split(new[] { Environment.NewLine }, StringSplitOptions.None);
                    }
                }
            }
        }
    }
}
