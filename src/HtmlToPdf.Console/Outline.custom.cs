// <copyright file="Outline.custom.cs" company="HtmlToPdf">
// Copyright (c) HtmlToPdf. All rights reserved.
// </copyright>

namespace HtmlToPdf.Console.Outline.Xml
{
    using System.Xml.Serialization;

#pragma warning disable SA1300 // Element must begin with upper-case letter
    /// <summary>
    /// The item
    /// </summary>
    public partial class item
    {
        /// <summary>
        /// Gets or sets the level.
        /// </summary>
        [XmlIgnore]
        public int level { get; set; }
    }
#pragma warning restore SA1300 // Element must begin with upper-case letter
}
