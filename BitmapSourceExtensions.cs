using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace WPF_Thumbnails
{
    public static class BitmapSourceExtensions
    {
        public static RenderTargetBitmap DrawFilledRectangle(this BitmapSource source, Size size, Point position, Color color)
        {
            var drawingVisual = new DrawingVisual();
            var drawingContext = drawingVisual.RenderOpen();
            drawingContext.DrawImage(source, new Rect(0, 0, source.Width, source.Height));
            var rect = new Rect(position, size);
            drawingContext.DrawRectangle(new SolidColorBrush(color), new Pen(new SolidColorBrush(color), 1), rect);
            drawingContext.Close();
            var bmp = new RenderTargetBitmap(source.PixelWidth, source.PixelHeight, source.DpiX, source.DpiY, PixelFormats.Pbgra32);
            bmp.Render(drawingVisual);
            return bmp;
        }
        public static RenderTargetBitmap DrawImage(this BitmapSource source, BitmapSource addition, Point position)
        {
            var drawingVisual = new DrawingVisual();
            var drawingContext = drawingVisual.RenderOpen();
            drawingContext.DrawImage(source, new Rect(0, 0, source.Width, source.Height));

            drawingContext.DrawImage(addition, new Rect(position.X, position.Y, addition.Width, addition.Height));

            drawingContext.Close();
            var bmp = new RenderTargetBitmap(source.PixelWidth, source.PixelHeight, source.DpiX, source.DpiY, PixelFormats.Pbgra32);
            bmp.Render(drawingVisual);
            return bmp;
        }
        public static bool IsEqual(this BitmapSource image1, BitmapSource image2)
        {
            if (image1 == null || image2 == null)
            {
                return false;
            }
            return image1.ToBytes().SequenceEqual(image2.ToBytes());
        }

        private static IEnumerable<byte> ToBytes(this BitmapSource image)
        {
            byte[] data = { };
            if (image == null) return data;
            try
            {
                var encoder = new BmpBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(image));
                using (var ms = new MemoryStream())
                {
                    encoder.Save(ms);
                    data = ms.ToArray();
                }
                return data;
            }
            catch
            {
                return new byte[0];
            }
        }

        public static RenderTargetBitmap DrawText(this BitmapSource source, string[] text, DrawTextFormat textFormat)
        {
            var drawingVisual = new DrawingVisual();
            var drawingContext = drawingVisual.RenderOpen();
            drawingContext.DrawImage(source, new Rect(0, 0, source.PixelWidth, source.PixelHeight));
            if (textFormat.ResizeOverflow == true)
            {
                textFormat.Size = AdjustFontSize(source, text, textFormat.Typeface, textFormat.Size, textFormat.Fill);
            }

            for (var i = 0; i < text.Length; i++)
            {
                var formatText = new FormattedText(text[i],
                new CultureInfo("en-us"),
                FlowDirection.LeftToRight,
                textFormat.Typeface,
                textFormat.Size,
                new SolidColorBrush(textFormat.Fill),
                source.DpiX / source.DpiY);

                var position = textFormat.Centered == true ? new Point((source.Width - formatText.Width) / 2 + textFormat.Adjusted.X, textFormat.Default.Y + textFormat.Adjusted.Y + i * formatText.Height) : new Point(textFormat.Default.X + textFormat.Adjusted.X, textFormat.Default.Y + textFormat.Adjusted.Y + i * formatText.Height);

                if (textFormat.Background == true)
                {
                    var rect = new Rect(new Point(0, (int)(textFormat.Default.Y + textFormat.Adjusted.Y + i * formatText.Height)), new Size(source.PixelWidth, formatText.Height));
                    drawingContext.DrawRectangle(new SolidColorBrush(textFormat.BackgroundColor ?? Colors.Transparent), new Pen(new SolidColorBrush(textFormat.BackgroundColor ?? Colors.Transparent), 1), rect);
                }

                RenderTextInformation(source, textFormat, drawingContext, formatText, position);
            }

            drawingContext.Close();
            var bmp = new RenderTargetBitmap(source.PixelWidth, source.PixelHeight, source.DpiX, source.DpiY, PixelFormats.Pbgra32);
            bmp.Render(drawingVisual);
            return bmp;
        }

        private static double AdjustFontSize(BitmapSource source, string[] text, Typeface typeface, double size, Color fill)
        {
            double maxWidth = 0;
            var longestIndex = 0;
            for (var i = 0; i < text.Length; i++)
            {
                var formatText = new FormattedText(text[i],
                    new CultureInfo("en-us"),
                    FlowDirection.LeftToRight,
                    typeface,
                    size,
                    new SolidColorBrush(fill),
                    source.DpiX / source.DpiY
                );
                if (formatText.Width + 10 > maxWidth) { maxWidth = formatText.Width; longestIndex = i; };
            }

            if (!(maxWidth > source.PixelWidth)) return size;
            
            double totalWidth;
            do
            {
                size -= 1;
                var formatText = new FormattedText(text[longestIndex],
                    new CultureInfo("en-us"),
                    FlowDirection.LeftToRight,
                    typeface,
                    size,
                    new SolidColorBrush(fill),
                    source.DpiX / source.DpiY
                );
                totalWidth = formatText.Width + 10;
            } while (totalWidth > source.PixelWidth);

            return size;
        }

        public static RenderTargetBitmap DrawText(this BitmapSource source, string text, DrawTextFormat textFormat)
        {
            var drawingVisual = new DrawingVisual();
            var drawingContext = drawingVisual.RenderOpen();
            drawingContext.DrawImage(source, new Rect(0, 0, source.PixelWidth, source.PixelHeight));

            var formatText = new FormattedText(text,
            new CultureInfo("en-us"),
            FlowDirection.LeftToRight,
            textFormat.Typeface,
            textFormat.Size,
            new SolidColorBrush(textFormat.Fill),
            source.DpiX / source.DpiY);

            if (textFormat.ResizeOverflow == true && formatText.Width + 10 > source.PixelWidth)
            {
                do
                {
                    textFormat.Size -= 1;
                    formatText = new FormattedText(text,
                        new CultureInfo("en-us"),
                        FlowDirection.LeftToRight,
                        textFormat.Typeface,
                        textFormat.Size,
                        new SolidColorBrush(textFormat.Fill),
                        source.DpiX / source.DpiY
                    );
                } while (formatText.Width + 10 > source.PixelWidth);
            }

            var position = textFormat.Centered == true ? new Point((source.Width - formatText.Width) / 2 + textFormat.Adjusted.X, textFormat.Default.Y + textFormat.Adjusted.Y) : new Point(textFormat.Default.X + textFormat.Adjusted.X, textFormat.Default.Y + textFormat.Adjusted.Y);

            if (textFormat.Background == true)
            {
                var rect = new Rect(new Point(0, (int)(textFormat.Default.Y + textFormat.Adjusted.Y)), new Size(source.PixelWidth, formatText.Height));
                drawingContext.DrawRectangle(new SolidColorBrush(textFormat.BackgroundColor ?? Colors.Transparent), new Pen(new SolidColorBrush(textFormat.BackgroundColor ?? Colors.Transparent), 1), rect);
            }

            RenderTextInformation(source, textFormat, drawingContext, formatText, position);

            drawingContext.Close();
            var bmp = new RenderTargetBitmap(source.PixelWidth, source.PixelHeight, source.DpiX, source.DpiY, PixelFormats.Pbgra32);
            bmp.Render(drawingVisual);
            return bmp;
        }

        private static void RenderTextInformation(this BitmapSource source, DrawTextFormat textFormat, DrawingContext drawingContext, FormattedText formatText, Point position)
        {
            var shadow = Color.FromArgb(125, 0, 0, 0);
            switch (textFormat.DropShadow)
            {
                case true when textFormat.Outline == true:
                {
                    const int distance = 2;
                    for (var offset = 1; 0 <= offset; offset--)
                    {
                        var point = new Point()
                        {
                            X = position.X + offset * distance,
                            Y = position.Y + offset * distance
                        };

                        var geo = formatText.BuildGeometry(point);
                        drawingContext.DrawGeometry(new SolidColorBrush(offset < 1 ? textFormat.Fill : shadow), new Pen(new SolidColorBrush(textFormat.Edge ?? shadow), 2), geo);
                    }

                    break;
                }

                case true:
                {
                    const int distance = 2;
                    var shadowText = new FormattedText(formatText.Text,
                        new CultureInfo("en-us"),
                        FlowDirection.LeftToRight,
                        textFormat.Typeface,
                        textFormat.Size,
                        new SolidColorBrush(shadow),
                        source.DpiX / source.DpiY);
                    for (var offset = 1; 0 <= offset; offset--)
                    {
                        var point = new Point()
                        {
                            X = position.X + offset * distance,
                            Y = position.Y + offset * distance
                        };

                        drawingContext.DrawText(offset < 1 ? formatText : shadowText, point);
                    }

                    break;
                }

                default:
                {
                    if (textFormat.Outline == true)
                    {
                        var geo = formatText.BuildGeometry(position);
                        drawingContext.DrawGeometry(new SolidColorBrush(textFormat.Fill), new Pen(new SolidColorBrush(textFormat.Edge ?? Colors.Black), 2), geo);
                    }
                    else
                    {
                        drawingContext.DrawText(formatText, position);
                    }

                    break;
                }
            }
        }

        public static BitmapSource ResizedImage(this BitmapSource source, int width, int height, int margin = 0)
        {
            var rect = new Rect(margin, margin, width - margin * 2, height - margin * 2);

            var group = new DrawingGroup();
            RenderOptions.SetBitmapScalingMode(group, BitmapScalingMode.HighQuality);
            group.Children.Add(new ImageDrawing(source, rect));

            var drawingVisual = new DrawingVisual();
            using (var drawingContext = drawingVisual.RenderOpen())
                drawingContext.DrawDrawing(group);

            var resizedImage = new RenderTargetBitmap(
                width, height,         // Resized dimensions
                96, 96,                // Default DPI values
                PixelFormats.Default); // Default pixel format
            resizedImage.Render(drawingVisual);

            return BitmapFrame.Create(resizedImage);
        }

        public static BitmapSource Convert(this System.Drawing.Bitmap image)
        {
            var rect = new System.Drawing.Rectangle(0, 0, image.Width, image.Height);
            var bitmapData = image.LockBits(rect, System.Drawing.Imaging.ImageLockMode.ReadOnly, image.PixelFormat);

            try
            {
                BitmapPalette palette = null;

                if (image.Palette.Entries.Length > 0)
                {
                    var paletteColors = image.Palette.Entries.Select(entry => Color.FromArgb(entry.A, entry.R, entry.G, entry.B)).ToList();
                    palette = new BitmapPalette(paletteColors);
                }

                return BitmapSource.Create(
                    image.Width,
                    image.Height,
                    image.HorizontalResolution,
                    image.VerticalResolution,
                    ConvertPixelFormat(image.PixelFormat),
                    palette,
                    bitmapData.Scan0,
                    bitmapData.Stride * image.Height,
                    bitmapData.Stride
                );
            }
            finally
            {
                image.UnlockBits(bitmapData);
            }
        }

        private static PixelFormat ConvertPixelFormat(System.Drawing.Imaging.PixelFormat sourceFormat)
        {
            switch (sourceFormat)
            {
                case System.Drawing.Imaging.PixelFormat.Format24bppRgb:
                    return PixelFormats.Bgr24;

                case System.Drawing.Imaging.PixelFormat.Format32bppArgb:
                    return PixelFormats.Bgra32;

                case System.Drawing.Imaging.PixelFormat.Format32bppRgb:
                    return PixelFormats.Bgr32;
            }

            return new PixelFormat();
        }

        public static System.Drawing.Bitmap Convert(this BitmapSource source)
        {
            System.Drawing.Bitmap bitmap;
            using (var outStream = new MemoryStream())
            {
                BitmapEncoder enc = new BmpBitmapEncoder();
                enc.Frames.Add(BitmapFrame.Create(source));
                enc.Save(outStream);
                bitmap = new System.Drawing.Bitmap(outStream);
            }
            return bitmap;
        }

        public static BitmapImage ConvertToBitmapImage(this BitmapSource target)
        {
            var bitmapImage = new BitmapImage();
            var bitmapEncoder = new BmpBitmapEncoder();
            bitmapEncoder.Frames.Add(BitmapFrame.Create(target));

            using (var stream = new MemoryStream())
            {
                bitmapEncoder.Save(stream);
                stream.Seek(0, SeekOrigin.Begin);

                bitmapImage.BeginInit();
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.StreamSource = stream;
                bitmapImage.EndInit();
            }

            return bitmapImage;
        }
        
        
        public static BitmapSource ScaleImage(this BitmapSource image, int maxWidth, int maxHeight)
        {
            //Determine MaxWidth Ratio/Scale
            var ratioX = maxWidth / image.Width;

            //Determine MaxHeight Ratio/Scale
            var ratioY = maxHeight / image.Height;

            //Determine Smallest Minimum Scale to maintain best image shape
            var ratio = Math.Min(ratioX, ratioY);


            //get the new width and height values
            var newWidth = (int)(image.Width * ratio);

            var newHeight = (int)(image.Height * ratio);

            //resize image to specified size
            return ResizedImage(image, newWidth, newHeight);

        }
        public static BitmapSource ScaledCrop(this BitmapSource source, int width, int height, double scale, double x,
            double y)
        {
            var initialSource = Math.Abs(scale - 1) > 0 ? source.ResizedImage((int)(source.Width * scale), (int)(source.Height * scale)) : source.Clone();
            try
            {
                if (source.Width < width)
                {
                    var rate = width / source.Width;
                    var newWidth = source.Width * rate;
                    var newHeight = source.Height * rate;
                    initialSource = source.ResizedImage((int)newWidth, (int)newHeight);
                }
                else if (source.Height < height)
                {
                    var rate = height / source.Height;
                    var newWidth = source.Width * rate;
                    var newHeight = source.Height * rate;
                    initialSource = source.ResizedImage((int)newWidth, (int)newHeight);
                }

                //get the middle point of the image
                var posX = x;
                var posY = y;
                //if x goes past zero then reset x to 0
                if (posX < 0)
                {
                    posX = 0;
                }
                //if y goes past zero then reset y to 0
                if (posY < 0)
                {
                    posY = 0;
                }
                //if x makes the width more than the area allows correct x with the difference in widths.
                if (posX + width > initialSource.Width)
                {
                    var diff = (int)(posX + width - initialSource.Width);
                    posX -= diff;
                }
                //if y makes the height more than the area allows correct y with the difference in widths.
                if (posY + height > initialSource.Height)
                {
                    var diff = (int)(posY + height - initialSource.Height);
                    posY -= diff;
                }

                return initialSource.CropAtRect(new Int32Rect((int)posX, (int)posY, width, height));
            }
            catch (Exception )
            {
                //if failed just return original image using the crop size from the top left
                return initialSource.CropAtRect(new Int32Rect((int)0, (int)0, width, height));
            }
           
        }
        public static void SaveToBmp(this BitmapSource visual, string fileName)
        {
            var encoder = new BmpBitmapEncoder();
            SaveUsingEncoder(visual, fileName, encoder);
        }

        public static void SaveToJpeg(this BitmapSource visual, string fileName)
        {
            var encoder = new JpegBitmapEncoder();
            SaveUsingEncoder(visual, fileName, encoder);
        }

        public static void SaveToPng(this BitmapSource visual, string fileName)
        {
            var encoder = new PngBitmapEncoder();
            SaveUsingEncoder(visual, fileName, encoder);
        }

        private static void SaveUsingEncoder(BitmapSource visual, string fileName, BitmapEncoder encoder)
        {
            encoder.Frames.Add(BitmapFrame.Create(visual));

            using (var stream = File.Create(fileName))
            {
                encoder.Save(stream);
            }
        }
        public static BitmapSource CropAtRect(this BitmapSource b, Int32Rect r)
        {
            var cropped = new CroppedBitmap(b,r);
            return cropped;
        }

        public static BitmapSource ResizeImageAndRatio(this BitmapSource original, double newWidth, double newHeight, double adjustedWidth = 0, double adjustedHeight = 0, double rescale = 1)
        {
            //rescale BaseImage
            var initialSource = Math.Abs(rescale - 1) > 0 ? original.ResizedImage((int)(original.Width * rescale), (int)(original.PixelHeight * rescale)) : original.Clone();

            if(Math.Abs(adjustedHeight) > 0 || Math.Abs(adjustedWidth) > 0)
            {
                try
                {
                    //get the middle point of the image
                    var posX = (int)((initialSource.Width - original.Width) / 2 + adjustedWidth);
                    var posY = (int)((initialSource.Height - original.Height) / 2 + adjustedHeight);
                    //if x goes past zero then reset x to 0
                    if (posX < 0)
                    {
                        posX = 0;
                    }
                    //if y goes past zero then reset y to 0
                    if (posY < 0)
                    {
                        posY = 0;
                    }
                    //if x makes the width more than the area allows correct x with the difference in widths.
                    if (posX + original.Width > initialSource.Width)
                    {
                        var diff = (int)(posX + original.Width - initialSource.Width);
                        posX -= diff;
                    }
                    //if y makes the height more than the area allows correct y with the difference in widths.
                    if (posY + original.Height > initialSource.Height)
                    {
                        var diff = (int)(posY + original.Height - initialSource.Height);
                        posY -= diff;
                    }

                    initialSource = initialSource.CropAtRect(new Int32Rect(posX, posY, original.PixelWidth, original.PixelHeight));
                }
                catch (Exception)
                {
                    // ignored, if crop fails because value is out of bounds ignore and reset image
                    initialSource = Math.Abs(rescale - 1) > 0 ? original.ResizedImage((int)(original.Width * rescale), (int)(original.PixelHeight * rescale)) : original.Clone();
                }
            }
            

            //build image and destination ratios
            var destinationRatio = newWidth / newHeight;
            var baseRatio = initialSource.Width / initialSource.Height;
            try
            {
                //if the ratio are both the same (16:9 == 16:9)
                // ReSharper disable once CompareOfFloatsByEqualityOperator
                if (destinationRatio == baseRatio)
                {
                    return ScaleImage(initialSource, (int)newWidth, (int)newHeight);
                }
                else
                {
                    // create a new drawing context to render that changes
                    var drawingVisual = new DrawingVisual();
                    var drawingContext = drawingVisual.RenderOpen();

                    //create the source and destination areas to get and put the image
                    var sourceArea = new Rect(0, 0, 0, 0);
                    var destinationArea = new Rect(0, 0, 0, 0);

                    //if the Destination Ratio is larger than the original ratio as the width is larger than the height of the image
                    if (destinationRatio > baseRatio)
                    {
                        // set start position X, Y
                        sourceArea.X = 0;
                        // set the Y axis to start on the point in the image where the height matches the destinations ratio.
                        sourceArea.Y = int.Parse(Math.Floor((initialSource.Height - initialSource.Width / destinationRatio) / 2).ToString(CultureInfo.CurrentCulture));
                        // set the width to the source
                        sourceArea.Width = initialSource.Width;
                        // set the height to match the base width by the dest. ratio
                        sourceArea.Height = int.Parse(Math.Floor(initialSource.Width / destinationRatio).ToString(CultureInfo.CurrentCulture));

                        // set the X and Y
                        destinationArea.X = 0;
                        destinationArea.Y = 0;
                        //set the width to base width
                        destinationArea.Width = initialSource.Width;
                        // set the height to match the base width by the dest. ratio
                        destinationArea.Height = int.Parse(Math.Floor(initialSource.Width / destinationRatio).ToString(CultureInfo.CurrentCulture));
                    }

                    //if the Destination Ratio is lesser than the original ratio as the height is larger than the width of the image
                    else
                    {
                        // set start position X, Y
                        // set the X axis to start on the point in the image where the width matches the destinations ratio.
                        sourceArea.X = int.Parse(Math.Floor((initialSource.Width - initialSource.Height * destinationRatio) / 2).ToString(CultureInfo.CurrentCulture));
                        sourceArea.Y = 0;
                        // set the width to match the base height by the dest. ratio
                        sourceArea.Width = int.Parse(Math.Floor(initialSource.PixelHeight * destinationRatio).ToString(CultureInfo.CurrentCulture));
                        // set the width to the source
                        sourceArea.Height = initialSource.Height;

                        // set the X and Y
                        destinationArea.X = 0;
                        destinationArea.Y = 0;
                        // set the width to match the base height by the dest. ratio
                        destinationArea.Width = int.Parse(Math.Floor(initialSource.Height * destinationRatio).ToString(CultureInfo.CurrentCulture));
                        //set the height to base height
                        destinationArea.Height = initialSource.Height;
                    }

                    
                    //crop from the initial source the area that matches the destinations ratio.
                    var image = initialSource.CropAtRect(new Int32Rect((int)sourceArea.X, (int)sourceArea.Y, (int)sourceArea.Width, (int)sourceArea.Height));

                    //render the image from the cropped portion using the destination area
                    drawingContext.DrawImage(image, destinationArea);
                    drawingContext.Close();

                    //use the visual context to render into a bitmap target
                    var bmp = new RenderTargetBitmap((int)destinationArea.Width, (int)destinationArea.Height, initialSource.DpiX, initialSource.DpiY, PixelFormats.Pbgra32);

                    //return rendered image
                    bmp.Render(drawingVisual);
                    return bmp;
                }
            }
            catch (Exception)
            {
                return null;
            }

        }

    }
}
