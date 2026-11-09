using System.Windows;
using System.Windows.Media;

namespace BurhanNexusStopwatch.Controls;

/// <summary>
/// Draws text as a classic LCD seven-segment display (like a Casio digital watch),
/// including faint "ghost" segments for the unlit parts. Supports 0-9, ':', '.', '-' and space.
/// Rendered with vector geometry, so it stays crisp at every window size and needs no font file.
/// </summary>
public sealed class SevenSegmentDisplay : FrameworkElement
{
    // Unit cell geometry (digit height = 100 units).
    private const double H = 100;
    private const double DigitWidth = 54;
    private const double ColonWidth = 20;
    private const double DotWidth = 18;
    private const double Spacing = 9;
    private const double T = 11;          // segment thickness
    private const double Gap = 1.6;       // gap between segments
    private const double SlantDegrees = 7;

    private static readonly double SlantOffset = H * Math.Tan(SlantDegrees * Math.PI / 180);

    // Segment order: a (top), b (top-right), c (bottom-right), d (bottom), e (bottom-left), f (top-left), g (middle)
    private static readonly Geometry[] Segments = BuildSegments();
    private static readonly Geometry ColonGeometry = BuildColon();
    private static readonly Geometry DotGeometry = BuildDot();

    private static readonly byte[] DigitMasks =
    {
        0b0111111, // 0: a b c d e f
        0b0000110, // 1: b c
        0b1011011, // 2: a b d e g
        0b1001111, // 3: a b c d g
        0b1100110, // 4: b c f g
        0b1101101, // 5: a c d f g
        0b1111101, // 6: a c d e f g
        0b0000111, // 7: a b c
        0b1111111, // 8
        0b1101111, // 9: a b c d f g
    };

    public static readonly DependencyProperty TextProperty = DependencyProperty.Register(
        nameof(Text), typeof(string), typeof(SevenSegmentDisplay),
        new FrameworkPropertyMetadata("00:00:00.00",
            FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.AffectsMeasure));

    public static readonly DependencyProperty OnBrushProperty = DependencyProperty.Register(
        nameof(OnBrush), typeof(Brush), typeof(SevenSegmentDisplay),
        new FrameworkPropertyMetadata(Brushes.Black, FrameworkPropertyMetadataOptions.AffectsRender));

    public static readonly DependencyProperty OffBrushProperty = DependencyProperty.Register(
        nameof(OffBrush), typeof(Brush), typeof(SevenSegmentDisplay),
        new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender));

    public static readonly DependencyProperty DigitHeightProperty = DependencyProperty.Register(
        nameof(DigitHeight), typeof(double), typeof(SevenSegmentDisplay),
        new FrameworkPropertyMetadata(100.0,
            FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.AffectsMeasure));

    public string Text
    {
        get => (string)GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    public Brush OnBrush
    {
        get => (Brush)GetValue(OnBrushProperty);
        set => SetValue(OnBrushProperty, value);
    }

    public Brush? OffBrush
    {
        get => (Brush?)GetValue(OffBrushProperty);
        set => SetValue(OffBrushProperty, value);
    }

    public double DigitHeight
    {
        get => (double)GetValue(DigitHeightProperty);
        set => SetValue(DigitHeightProperty, value);
    }

    private double Scale => DigitHeight / H;

    private static double CellWidth(char c) => c switch
    {
        ':' => ColonWidth,
        '.' => DotWidth,
        _ => DigitWidth,
    };

    private double UnitWidth()
    {
        var text = Text ?? string.Empty;
        if (text.Length == 0) return 0;
        double w = 0;
        foreach (var c in text) w += CellWidth(c);
        w += Spacing * (text.Length - 1);
        return w + SlantOffset;
    }

    protected override Size MeasureOverride(Size availableSize)
        => new(UnitWidth() * Scale, H * Scale);

    protected override void OnRender(DrawingContext dc)
    {
        var text = Text ?? string.Empty;
        if (text.Length == 0) return;

        var on = OnBrush;
        var off = OffBrush;

        dc.PushTransform(new ScaleTransform(Scale, Scale));
        // Italic lean like a real watch display: shift the top to the right.
        dc.PushTransform(new MatrixTransform(1, 0, -Math.Tan(SlantDegrees * Math.PI / 180), 1, SlantOffset, 0));

        double x = 0;
        foreach (var c in text)
        {
            dc.PushTransform(new TranslateTransform(x, 0));
            switch (c)
            {
                case ':':
                    dc.DrawGeometry(on, null, ColonGeometry);
                    break;
                case '.':
                    dc.DrawGeometry(on, null, DotGeometry);
                    break;
                case ' ':
                    if (off is not null) DrawMask(dc, 0b1111111, off);
                    break;
                default:
                    int mask = c == '-' ? 0b1000000 : (c is >= '0' and <= '9' ? DigitMasks[c - '0'] : 0);
                    if (off is not null) DrawMask(dc, 0b1111111 & ~mask, off);
                    DrawMask(dc, mask, on);
                    break;
            }
            dc.Pop();
            x += CellWidth(c) + Spacing;
        }

        dc.Pop();
        dc.Pop();
    }

    private static void DrawMask(DrawingContext dc, int mask, Brush brush)
    {
        for (int i = 0; i < 7; i++)
        {
            if ((mask & (1 << i)) != 0)
                dc.DrawGeometry(brush, null, Segments[i]);
        }
    }

    private static Geometry[] BuildSegments()
    {
        const double half = T / 2;
        double xL = half, xR = DigitWidth - half;

        Geometry Horizontal(double yc) => Polygon(
            new Point(xL + Gap, yc),
            new Point(xL + Gap + half, yc - half),
            new Point(xR - Gap - half, yc - half),
            new Point(xR - Gap, yc),
            new Point(xR - Gap - half, yc + half),
            new Point(xL + Gap + half, yc + half));

        Geometry Vertical(double xc, double y0, double y1) => Polygon(
            new Point(xc, y0),
            new Point(xc + half, y0 + half),
            new Point(xc + half, y1 - half),
            new Point(xc, y1),
            new Point(xc - half, y1 - half),
            new Point(xc - half, y0 + half));

        double top = half, mid = H / 2, bottom = H - half;

        return new[]
        {
            Horizontal(top),                               // a
            Vertical(xR, top + Gap, mid - Gap),            // b
            Vertical(xR, mid + Gap, bottom - Gap),         // c
            Horizontal(bottom),                            // d
            Vertical(xL, mid + Gap, bottom - Gap),         // e
            Vertical(xL, top + Gap, mid - Gap),            // f
            Horizontal(mid),                               // g
        };
    }

    private static Geometry BuildColon()
    {
        double s = T * 0.95;
        double cx = ColonWidth / 2;
        var group = new GeometryGroup();
        group.Children.Add(new RectangleGeometry(new Rect(cx - s / 2, H * 0.30 - s / 2, s, s)));
        group.Children.Add(new RectangleGeometry(new Rect(cx - s / 2, H * 0.70 - s / 2, s, s)));
        group.Freeze();
        return group;
    }

    private static Geometry BuildDot()
    {
        double s = T * 0.95;
        var g = new RectangleGeometry(new Rect(DotWidth / 2 - s / 2, H - s, s, s));
        g.Freeze();
        return g;
    }

    private static Geometry Polygon(params Point[] points)
    {
        var geo = new StreamGeometry();
        using (var ctx = geo.Open())
        {
            ctx.BeginFigure(points[0], isFilled: true, isClosed: true);
            ctx.PolyLineTo(points.Skip(1).ToArray(), isStroked: false, isSmoothJoin: false);
        }
        geo.Freeze();
        return geo;
    }
}
