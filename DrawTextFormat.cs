using System.Windows;
using System.Windows.Media;

namespace WPF_Thumbnails
{
    public struct DrawTextFormat
    {
        private DrawTextFormat(Typeface typeface,
                              double size,
                              Color fill,
                              bool? background,
                              bool? outline,
                              bool? dropshadow,
                              bool? centered,
                              bool? resizeOverflow,
                              Point? adjusted,
                              Point? @default,
                              Color? edge,
                              Color? backgroundColor)
        {
            Typeface = typeface;
            Size = size;
            Fill = fill;
            Background = background;
            Outline = outline;
            DropShadow = dropshadow;
            Centered = centered;
            ResizeOverflow = resizeOverflow;
            Adjusted = adjusted??default;
            Default = @default??default;
            Edge = edge;
            BackgroundColor = backgroundColor;
        }
        public bool? Background { get; }
        public bool? Outline { get; }
        public bool? DropShadow { get; }
        public bool? Centered { get; }
        public bool? ResizeOverflow { get; }
        public double Size { get; set; }
        public Typeface @Typeface { get; }
        public Point Adjusted { get; }
        public Point Default { get; }
        public Color? Edge { get; }
        public Color? BackgroundColor { get; }
        public Color Fill { get; }

        public static DrawTextFormat Make(Typeface typeface,
                              double size,
                              Color fill,
                              bool? background = null,
                              bool? outline = null,
                              bool? dropshadow = null,
                              bool? centered = null,
                              bool? resizeOverflow = null,
                              Point? adjusted = null,
                              Point? @default = null,
                              Color? edge = null,
                              Color? backgroundColor = null)
        {
            return new DrawTextFormat(typeface, size, fill, background, outline, dropshadow, centered, resizeOverflow, adjusted, @default, edge, backgroundColor);
        }
    }


}
