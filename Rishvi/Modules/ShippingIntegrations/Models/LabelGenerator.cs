using Newtonsoft.Json;
using Rishvi.Modules.Core.Helpers;
using SkiaSharp;
using ZXing;
using ZXing.QrCode;
using ZXing.SkiaSharp;

namespace Rishvi.Modules.ShippingIntegrations.Models
{
    public class LabelGenerator
    {
        public static int DPI = 400;

        public static string GenerateLabel(GenerateLabelRequest request, Item item, string newTrackingNumber, string consignmentNo, string AddressFormatted, int itemCount, int totalItemCount, string labelReference, string companyName)
        {
            SqlHelper.SystemLogInsert("GenerateLabel", null, request.ToString(), null, "success", "Label generated successfully", false, "Generate Lebel");
            try
            {
                // Specify the width and height of the image
                float widthInInches = 4.0f;
                float heightInInches = 6.0f;
                float dpi = 96.0f;
                string fontName = "sans-serif";

                var paint15PXWithBold = new SKPaint()
                {
                    Color = SKColors.Black,
                    TextSize = 15,
                    IsAntialias = true,
                    Typeface = SKTypeface.FromFamilyName(fontName, SKTypefaceStyle.Bold)
                };

                var paint13PXWithBold = new SKPaint()
                {
                    Color = SKColors.Black,
                    TextSize = 13,
                    IsAntialias = true,
                    Typeface = SKTypeface.FromFamilyName(fontName, SKTypefaceStyle.Bold)
                };

                var paint13PX = new SKPaint()
                {
                    Color = SKColors.Black,
                    TextSize = 13,
                    IsAntialias = true,
                    Typeface = SKTypeface.FromFamilyName(fontName, SKTypefaceStyle.Normal)
                };

                var paint11PX = new SKPaint()
                {
                    Color = SKColors.Black,
                    TextSize = 11,
                    IsAntialias = true,
                    Typeface = SKTypeface.FromFamilyName(fontName, SKTypefaceStyle.Normal)
                };

                var paint25PXWhiteFont = new SKPaint()
                {
                    Color = SKColors.White,
                    TextSize = 25,
                    IsAntialias = true,
                    Typeface = SKTypeface.FromFamilyName(fontName, SKTypefaceStyle.Normal)
                };

                // Convert inches to pixels
                int width = (int)GetPixels(widthInInches, dpi);
                int height = (int)GetPixels(heightInInches, dpi);
                // Create a new SKBitmap
                using (var bitmap = new SKBitmap(width, height))
                {
                    // Create an SKCanvas to draw on the bitmap
                    using (var canvas = new SKCanvas(bitmap))
                    {
                        // Set the background color (optional)
                        canvas.Clear(SKColors.White);

                        //Draw postal code
                        if (!string.IsNullOrEmpty(request.Postalcode))
                        {
                            var splitPostalCode = request.Postalcode.Split(" ");

                            if (splitPostalCode.Length >= 2)
                            {
                                using (var paint = new SKPaint())
                                {
                                    paint.Color = SKColors.Black;
                                    paint.TextSize = 80;
                                    paint.IsAntialias = true;
                                    paint.Typeface = SKTypeface.FromFamilyName("sans-serif", SKTypefaceStyle.Bold);

                                    canvas.DrawText(splitPostalCode[0], 135, 70, paint);
                                    canvas.DrawText(splitPostalCode[1], 135, 135, paint);
                                }
                            }
                            else
                            {
                                using (var paint = new SKPaint())
                                {
                                    paint.Color = SKColors.Black;
                                    paint.TextSize = 70;
                                    paint.IsAntialias = true;
                                    paint.Typeface = SKTypeface.FromFamilyName("sans-serif", SKTypefaceStyle.Bold);

                                    canvas.DrawText(splitPostalCode[0], 40, 100, paint);
                                }
                            }
                        }
                        float maxWidth = 180;
                        canvas.DrawText("SHIPPING ADDRESS:", 15, 160, paint15PXWithBold);
                        //canvas.DrawText(AddressFormatted, 15, 175, paint6PX);
                        // Draw text on multiple lines
                        DrawMultilineText(canvas, paint13PX, AddressFormatted, 15, 175, maxWidth);

                        canvas.DrawText("TRACKING NO:", 210, 160, paint15PXWithBold);
                        canvas.DrawText(newTrackingNumber==null?"":newTrackingNumber, 210, 175, paint13PX);

                        canvas.DrawText("ROUTE NO:", 210, 190, paint15PXWithBold);
                        canvas.DrawText("", 210, 205, paint13PX);

                        canvas.DrawText("COMPANY:", 210, 220, paint15PXWithBold);
                        canvas.DrawText(companyName, 210, 235, paint13PX);

                        canvas.DrawText("ORDER NO:", 210, 290, paint15PXWithBold);
                        if (labelReference == "ChannelReferance")
                        {
                            canvas.DrawText(request.OrderReference, 210, 305, paint13PX);
                        }
                        else
                        {
                            canvas.DrawText((request.OrderId > 0 ? request.OrderId.ToString() : ""), 210, 305, paint13PX);
                        }

                        canvas.DrawText("SHIP TO NAME:", 15, 300, paint15PXWithBold);
                        canvas.DrawText(request.Name, 15, 315, paint13PX);

                        ////QR Code Image
                        SKImage qrCodeImage = GenerateQRCode(consignmentNo + "-0000" + itemCount);
                        SKPoint destinationPoint = new SKPoint(15, 330);
                        var paintqrCode = new SKPaint()
                        {
                            Color = SKColors.Black,
                            IsAntialias = true
                        };
                        canvas.DrawImage(qrCodeImage, destinationPoint, paintqrCode);
                        float itemFlotY = 340;
                        canvas.DrawText("ITEM CODE:", 175, itemFlotY, paint15PXWithBold);
                        // Desired width of the text
                        float desiredItemCodeWidth = 160;
                        // Calculate the width of the text
                        float textItemCodeWidth = paint13PX.MeasureText(item.ProductCode);
                        int extraLineItemCode = 0;
                        // Check if text exceeds the desired width
                        if (textItemCodeWidth > desiredItemCodeWidth)
                        {
                            // Split the text into lines that fit within the desired width
                            string[] linesItemCode = SplitTextIntoLines(item.ProductCode, paint13PX, desiredItemCodeWidth);
                            extraLineItemCode = linesItemCode.Length > 1 ? linesItemCode.Length - 1 : 0;
                            // Draw each line onto the SkiaSharp canvas
                            float lineHeightItemCode = paint13PX.FontMetrics.Descent - paint13PX.FontMetrics.Ascent;
                            itemFlotY = itemFlotY + 15; // Starting Y position
                            foreach (string line in linesItemCode)
                            {
                                if (!string.IsNullOrEmpty(line))
                                {
                                    canvas.DrawText(line, 175, itemFlotY, paint13PX);
                                    itemFlotY += lineHeightItemCode; // Move to the next line
                                }
                            }
                        }
                        else
                        {
                            // Draw the entire text if it fits within the desired width
                            itemFlotY += 15;
                            canvas.DrawText(item.ProductCode, 175, itemFlotY, paint13PX);
                            itemFlotY += 15;
                        }

                        //canvas.DrawText(item.ProductCode, 175, 355, paint13PX);
                        //itemFlotY += 15;
                        canvas.DrawText("ITEM DESCRIPTION:", 175, itemFlotY, paint15PXWithBold);
                        // Desired width of the text
                        float desiredWidth = 160;
                        // Calculate the width of the text
                        float textWidth = paint13PX.MeasureText(item.ItemName);
                        int extraLine = 0;
                        // Check if text exceeds the desired width
                        if (textWidth > desiredWidth)
                        {
                            // Split the text into lines that fit within the desired width
                            string[] lines = SplitTextIntoLines(item.ItemName, paint13PX, desiredWidth);
                            extraLine = lines.Length > 1 ? lines.Length - 1 : 0;
                            // Draw each line onto the SkiaSharp canvas
                            float lineHeight = paint13PX.FontMetrics.Descent - paint13PX.FontMetrics.Ascent;
                            itemFlotY += 15; // Starting Y position
                            foreach (string line in lines)
                            {
                                if (!string.IsNullOrEmpty(line))
                                {
                                    canvas.DrawText(line, 175, itemFlotY, paint13PX);
                                    itemFlotY += lineHeight; // Move to the next line
                                }
                            }
                        }
                        else
                        {
                            // Draw the entire text if it fits within the desired width
                            itemFlotY += 15;
                            canvas.DrawText(item.ItemName, 175, itemFlotY, paint13PX);
                        }


                        //canvas.DrawText(item.ItemName, 175, 314, paint6PX);

                        canvas.DrawText("ITEM COUNT:", 175, (400 + (extraLine * 30)), paint13PXWithBold);
                        canvas.DrawText(itemCount + " of " + totalItemCount + " Items", 175, (415 + (extraLine * 30)), paint11PX);

                        // Draw a horizontal line
                        float startY = 490;
                        float endX = 360;
                        float startX = 15;

                        var paintLine = new SKPaint
                        {
                            Color = SKColors.Black,
                            StrokeWidth = 1
                        };

                        canvas.DrawLine(startX, startY, endX, startY, paintLine);

                        ////Bar code
                        SKImage barcodeImage = GenerateBarcode(consignmentNo + "-0000" + itemCount);
                        SKPoint barcodeSKPoint = new SKPoint(15, 505);
                        var paintBarCode = new SKPaint()
                        {
                            Color = SKColors.Black,
                            IsAntialias = true
                        };
                        canvas.DrawImage(barcodeImage, barcodeSKPoint, paintBarCode);

                        canvas.DrawText(consignmentNo + "-0000" + itemCount, 80, 570, paint13PX);

                        //rectangle
                        var rectanglePaint = new SKPaint()
                        {
                            Color = SKColors.Black
                        };
                        canvas.DrawRect(290, 495, 65, 65, rectanglePaint);
                        canvas.DrawText("S", 315, 535, paint25PXWhiteFont);

                        // Save the bitmap to a MemoryStream
                        using (var stream = new MemoryStream())
                        {
                            bitmap.Encode(SKEncodedImageFormat.Png, 100)
                                  .SaveTo(stream);
                            stream.Position = 0; // Reset stream position to the beginning
                            Console.WriteLine("Image generated successfully!");

                            // Now you can return the MemoryStream or use it as needed
                            // e.g., return stream;
                            return Convert.ToBase64String(stream.ToArray());
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                SqlHelper.SystemLogInsert("GenerateLabel", null, JsonConvert.SerializeObject(request).Replace("'", "''"), JsonConvert.SerializeObject(request).Replace("'", "''"), "LabelGenerateError", ex.Message, true,"Generate Lebel");
                EmailHelper.SendEmail("Failed generate label", ex.Message);
            }
            return string.Empty;
        }

        public static float GetPixels(float inches, float dpi)
        {
            return inches * dpi;
        }

        static SKImage GenerateBarcode(string barcodeNumber)
        {
            try
            {
                BarcodeWriter barcodeWriter = new BarcodeWriter();
                barcodeWriter.Format = BarcodeFormat.CODE_128;
                barcodeWriter.Options = new ZXing.Common.EncodingOptions
                {
                    Width = 300,
                    Height = 50
                };
                ZXing.Common.BitMatrix bitMatrix = barcodeWriter.Encode(barcodeNumber);
                SKBitmap barcodeBitmap = SKBitmap.FromImage(SkiaSharpBarcodeRenderer.Render(bitMatrix));

                return SKImage.FromBitmap(barcodeBitmap);
            }
            catch (Exception ex)
            {
                SqlHelper.SystemLogInsert("CreateOrder", null, barcodeNumber, null, "BarcodeGenerateError", ex.Message, true,"Generate lebel");
                EmailHelper.SendEmail("Failed generate barcode", ex.Message);
            }
            return null;
        }

        static SKImage GenerateQRCode(string qrCodeNumber)
        {
            try
            {
                // Create a QR code writer
                BarcodeWriter barcodeWriter = new BarcodeWriter();
                barcodeWriter.Format = BarcodeFormat.QR_CODE;
                QrCodeEncodingOptions options = new QrCodeEncodingOptions
                {
                    Width = 150,
                    Height = 150,
                    Margin = 0 // Adjust margin if needed
                };
                barcodeWriter.Options = options;
                // Generate a QR code bitmap
                SKBitmap qrCodeBitmap = barcodeWriter.Write(qrCodeNumber);
                return SKImage.FromBitmap(qrCodeBitmap);
            }
            catch (Exception ex)
            {
                SqlHelper.SystemLogInsert("CreateOrder", null, qrCodeNumber, null, "QRCodeGenerateError", ex.Message, true,"Generate Lebel");
                EmailHelper.SendEmail("Failed generate QRCode", ex.Message);
            }
            return null;
        }

        static string[] SplitTextIntoLines(string text, SKPaint paint, float maxWidth)
        {
            // Split the text by preserving both spaces and hyphens
            System.Text.RegularExpressions.Regex regex = new System.Text.RegularExpressions.Regex(@"[\s\-\/\(\)]");
            string[] words = regex.Split(text);
            System.Collections.Generic.List<string> delimiters = new System.Collections.Generic.List<string>();

            foreach (System.Text.RegularExpressions.Match match in regex.Matches(text))
            {
                delimiters.Add(match.Value);
            }

            string currentLine = string.Empty;
            System.Collections.Generic.List<string> lines = new System.Collections.Generic.List<string>();

            for (int i = 0; i < words.Length; i++)
            {
                string word = words[i];
                string delimiter = i < delimiters.Count ? delimiters[i] : string.Empty;

                float currentLineWidth = paint.MeasureText(currentLine + word + delimiter);

                if (currentLineWidth <= maxWidth)
                {
                    currentLine += word + delimiter;
                }
                else
                {
                    lines.Add(currentLine.TrimEnd());
                    currentLine = word + delimiter;
                }
            }

            if (!string.IsNullOrEmpty(currentLine))
            {
                lines.Add(currentLine.TrimEnd());
            }

            return lines.ToArray();
        }

        static void DrawMultilineText(SKCanvas canvas, SKPaint paint, string text, float x, float y, float maxWidth)
        {
            string[] lines = text.Split('\n');

            foreach (string line in lines)
            {
                var yFloat = DrawTextWithLineBreaks(canvas, paint, line, x, y, maxWidth);
                y = yFloat;
                //y += paint.FontMetrics.Descent - paint.FontMetrics.Ascent;
            }
        }

        static float DrawTextWithLineBreaks(SKCanvas canvas, SKPaint paint, string text, float x, float y, float maxWidth)
        {
            string[] words = text.Split(' ');
            string currentLine = string.Empty;

            foreach (string word in words)
            {
                float currentLineWidth = paint.MeasureText(currentLine + " " + word);

                if (currentLineWidth <= maxWidth)
                {
                    currentLine += (currentLine == string.Empty ? "" : " ") + word;
                }
                else
                {
                    canvas.DrawText(currentLine, x, y, paint);
                    y += paint.FontMetrics.Descent - paint.FontMetrics.Ascent;
                    currentLine = word;
                }
            }

            if (!string.IsNullOrEmpty(currentLine))
            {
                canvas.DrawText(currentLine, x, y, paint);
                y += paint.FontMetrics.Descent - paint.FontMetrics.Ascent;
            }
            return y;
        }
    }

    public static class SkiaSharpBarcodeRenderer
    {
        public static SKImage Render(ZXing.Common.BitMatrix bitMatrix)
        {
            int width = bitMatrix.Width;
            int height = bitMatrix.Height;
            SKBitmap bitmap = new SKBitmap(width, height);

            using (SKCanvas canvas = new SKCanvas(bitmap))
            using (SKPaint paint = new SKPaint())
            {
                paint.Color = SKColors.Black;

                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        if (bitMatrix[x, y])
                        {
                            canvas.DrawPoint(x, y, paint);
                        }
                    }
                }
            }

            return SKImage.FromBitmap(bitmap);
        }
    }
}
