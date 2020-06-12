// <copyright file="TempImageFile.cs" company="HtmlToPdf">
// Copyright (c) HtmlToPdf. All rights reserved.
// </copyright>

namespace HtmlToPdfTests
{
    using System.Drawing;
    using System.Drawing.Drawing2D;
    using System.Drawing.Text;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>
    /// Temp Image File
    /// </summary>
    /// <seealso cref="HtmlToPdfTests.TempFile" />
    internal class TempImageFile : TempFile
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TempImageFile"/> class.
        /// </summary>
        /// <param name="text">The text.</param>
        /// <param name="testContext">The test context.</param>
        public TempImageFile(string text, TestContext testContext = null)
            : base(".bmp", testContext)
        {
            this.Create(text);
        }

        private void Create(string text)
        {
            using (Bitmap bitmap = new Bitmap(1, 1))
            {
                // Create the Font object for the image text drawing.
                Font objFont = new Font(
                    "Arial",
                    50,
                    FontStyle.Regular,
                    GraphicsUnit.Pixel);

                // Create a graphics object to measure the text's width and height.
                using (Graphics graphics = Graphics.FromImage(bitmap))
                {
                    // get size
                    var width = (int)graphics.MeasureString(text, objFont).Width;
                    var height = (int)graphics.MeasureString(text, objFont).Height;

                    // Create the bitmap again with the correct size for the text and font.
                    using (Bitmap bitmap2 = new Bitmap(bitmap, new Size(width, height)))
                    {
                        // Add the colors to the new bitmap.
                        using (Graphics graphics2 = Graphics.FromImage(bitmap2))
                        {
                            // Set Background color
                            graphics2.Clear(Color.White);
                            graphics2.SmoothingMode = SmoothingMode.HighQuality;

                            graphics2.TextRenderingHint = TextRenderingHint.AntiAlias;
                            graphics2.DrawString(
                                text,
                                objFont,
                                new SolidBrush(Color.Black),
                                0,
                                0,
                                StringFormat.GenericDefault);

                            graphics2.Flush();
                        }

                        bitmap2.Save(this.FilePath);
                    }
                }
            }
        }
    }
}
