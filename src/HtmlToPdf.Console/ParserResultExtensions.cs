// <copyright file="ParserResultExtensions.cs" company="HtmlToPdf">
// Copyright (c) HtmlToPdf. All rights reserved.
// </copyright>

namespace HtmlToPdf.Console
{
    using CommandLine;
    using CommandLine.Text;

    /// <summary>
    /// Parser Result Extension Methods
    /// </summary>
    internal static class ParserResultExtensions
    {
        /// <summary>
        /// Gets the help text.
        /// </summary>
        /// <typeparam name="T">The parser result type.</typeparam>
        /// <param name="parserResult">The parser result.</param>
        /// <returns>The help text.</returns>
        internal static HelpText GetHelpText<T>(this ParserResult<T> parserResult)
        {
            return HelpText.AutoBuild(parserResult, h =>
            {
                h.AutoHelp = false;
                h.AutoVersion = false;
                return h;
            })
            .AddPreOptionsLines(EmbeddedResource.GetCommandLinePreOptions())
            .AddPostOptionsLines(EmbeddedResource.GetCommandLinePostOptions());
        }
    }
}
