// <copyright file="CustomUTF8Encoding.cs" company="HtmlToPdf">
// Copyright (c) HtmlToPdf. All rights reserved.
// </copyright>

namespace HtmlToPdf.Console
{
    using System.Text;

    /// <summary>
    /// Custom UTF8 Encoding.
    /// </summary>
    /// <seealso cref="System.Text.UTF8Encoding" />
    internal class CustomUTF8Encoding : UTF8Encoding
    {
        /// <summary>
        /// Gets a name for the current encoding that can be used with mail agent header tags.
        /// </summary>
        public override string HeaderName => base.HeaderName.ToUpper();

        /// <summary>
        /// Gets the name registered with the Internet Assigned Numbers Authority (IANA) for the current encoding.
        /// </summary>
        public override string WebName => base.WebName.ToUpper();

        /// <summary>
        /// Gets a name for the current encoding that can be used with mail agent body tags.
        /// </summary>
        public override string BodyName => base.BodyName.ToUpper();
    }
}