// <copyright file="StringAssertWithDiff.cs" company="HtmlToPdf">
// Copyright (c) HtmlToPdf. All rights reserved.
// </copyright>

namespace HtmlToPdfTests
{
    using System;
    using System.Globalization;
    using System.IO;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>
    /// Difference style.
    /// </summary>
    public enum DiffStyle
    {
        /// <summary>
        /// Full
        /// </summary>
        Full,

        /// <summary>
        /// Minimal
        /// </summary>
        Minimal,
    }

    /// <summary>
    /// String Assert with Differences.
    /// </summary>
    public static class StringAssertWithDiff
    {
        /// <summary>
        /// Tests whether the specified values are equal and throws an exception if the two values are not equal.
        /// Outputs detailed differences to the console.
        /// </summary>
        /// <param name="expectedValue">The first value to compare. This is the value the tests expects.</param>
        /// <param name="actualValue">The second value to compare. This is the value produced by the code under test.</param>
        /// <param name="diffStyle">The difference style.</param>
        public static void AreEqual(string expectedValue, string actualValue, DiffStyle diffStyle = DiffStyle.Full)
        {
            ShouldEqualWithDiff(expectedValue, actualValue, diffStyle, Console.Out);
        }

        private static void ShouldEqualWithDiff(string expectedValue, string actualValue, DiffStyle diffStyle, TextWriter output)
        {
            if (actualValue == null || expectedValue == null)
            {
                Assert.AreEqual(expectedValue, actualValue);
                return;
            }

            if (actualValue.Equals(expectedValue, StringComparison.Ordinal))
            {
                return;
            }

            output.WriteLine("  Idx Expected  Actual");
            output.WriteLine("-------------------------");
            int maxLen = Math.Max(actualValue.Length, expectedValue.Length);
            int minLen = Math.Min(actualValue.Length, expectedValue.Length);
            for (int i = 0; i < maxLen; i++)
            {
                if (diffStyle != DiffStyle.Minimal || i >= minLen || actualValue[i] != expectedValue[i])
                {
                    output.WriteLine(
                        "{0} {1,-3} {2,-4} {3,-3}  {4,-4} {5,-3}",
                        i < minLen && actualValue[i] == expectedValue[i] ? " " : "*", // put a mark beside a differing row
                        i, // the index
                        i < expectedValue.Length ? ((int)expectedValue[i]).ToString() : string.Empty, // character decimal value
                        i < expectedValue.Length ? expectedValue[i].ToSafeString() : string.Empty, // character safe string
                        i < actualValue.Length ? ((int)actualValue[i]).ToString() : string.Empty, // character decimal value
                        i < actualValue.Length ? actualValue[i].ToSafeString() : string.Empty); // character safe string
                }
            }

            output.WriteLine();

            Assert.AreEqual(expectedValue, actualValue);
        }

        private static string ToSafeString(this char c)
        {
            if (char.IsControl(c) || char.IsWhiteSpace(c))
            {
                switch (c)
                {
                    case '\r':
                        return @"\r";
                    case '\n':
                        return @"\n";
                    case '\t':
                        return @"\t";
                    case '\a':
                        return @"\a";
                    case '\v':
                        return @"\v";
                    case '\f':
                        return @"\f";
                    default:
                        return $"\\u{(int)c:X};";
                }
            }

            return c.ToString(CultureInfo.InvariantCulture);
        }
    }
}
