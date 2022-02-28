// <copyright file="StringExtensions.cs" company="HtmlToPdf">
// Copyright (c) HtmlToPdf. All rights reserved.
// </copyright>

namespace HtmlToPdf.Extensions
{
    using System.Linq;

    /// <summary>
    /// String Extensions.
    /// </summary>
    public static class StringExtensions
    {
        /// <summary>
        /// Appends the units.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="units">The units.</param>
        /// <returns>The string with units.</returns>
        public static string AppendUnits(this string value, string units)
        {
            string newValue = string.Copy(value);
            if (char.IsDigit(value.Last()))
            {
                newValue += units;
            }

            return newValue;
        }
    }
}
