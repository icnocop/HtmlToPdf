// <copyright file="DateAssert.cs" company="HtmlToPdf">
// Copyright (c) HtmlToPdf. All rights reserved.
// </copyright>

namespace HtmlToPdfTests
{
    using System;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>
    /// Date Assert.
    /// </summary>
    public static class DateAssert
    {
        /// <summary>
        /// Tests whether the specified values are within five seconds and throws an exception if the two values are not within five seconds.
        /// </summary>
        /// <param name="firstDateTime">The first date time.</param>
        /// <param name="secondDateTime">The second date time.</param>
        public static void AreWithinFiveSeconds(DateTime firstDateTime, DateTime secondDateTime)
        {
            TimeSpan timeSpan = firstDateTime - secondDateTime;

            double totalSeconds = timeSpan.TotalSeconds;

            string message = $"Expected: <{firstDateTime}>. Actual: <{secondDateTime}>. The dates are {totalSeconds} second(s) apart.";

            Assert.IsTrue((totalSeconds >= -5) && (totalSeconds <= 5), message);
        }
    }
}
