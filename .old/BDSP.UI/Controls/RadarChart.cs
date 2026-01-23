using System;
using System.Diagnostics;
using System.Globalization;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;

namespace BDSP.UI.Controls;

public interface IRadarData
{
    ReadOnlySpan<double> Values { get; }
    ReadOnlySpan<string> Labels { get; }
    double MaxValue { get; }
}

public sealed class RadarChart : Control
{
    public static readonly StyledProperty<double[]> ValuesProperty =
        AvaloniaProperty.Register<RadarChart, double[]>(
            nameof(Values), Array.Empty<double>());

    public static readonly StyledProperty<string[]> LabelsProperty =
        AvaloniaProperty.Register<RadarChart, string[]>(
            nameof(Labels), Array.Empty<string>());

    public static readonly StyledProperty<double> MaxValueProperty =
        AvaloniaProperty.Register<RadarChart, double>(
            nameof(MaxValue), 100);

    public double[] Values
    {
        get => GetValue(ValuesProperty);
        set => SetValue(ValuesProperty, value);
    }

    public string[] Labels
    {
        get => GetValue(LabelsProperty);
        set => SetValue(LabelsProperty, value);
    }

    public double MaxValue
    {
        get => GetValue(MaxValueProperty);
        set => SetValue(MaxValueProperty, value);
    }

    static RadarChart()
    {
        AffectsRender<RadarChart>(
            ValuesProperty,
            LabelsProperty,
            MaxValueProperty);
    }

    public override void Render(DrawingContext context)
    {
        base.Render(context);

        if (Values.Length == 0 || Labels.Length != Values.Length)
            return;

        var center = Bounds.Center;
        var radius = Math.Min(Bounds.Width, Bounds.Height) * 0.45;

        Debug.WriteLine($"Radar size: {Bounds.Width} x {Bounds.Height}");
        Debug.WriteLine($"Values: {string.Join(",", Values)}");
        Debug.WriteLine($"MaxValue: {MaxValue}");

        DrawGrid(context, center, radius);
        DrawPolygon(context, center, radius);
        DrawLabels(context, center, radius);
    }

    private void DrawGrid(DrawingContext ctx, Point center, double radius)
    {
        var pen = new Pen(
            new SolidColorBrush(Color.FromArgb(60, 255, 255, 255)), 1);

        int axes = Values.Length;

        // Rings
        for (int r = 1; r <= 3; r++)
        {
            ctx.DrawEllipse(null, pen, center,
                radius * r / 3, radius * r / 3);
        }

        // Spokes
        for (int i = 0; i < axes; i++)
        {
            var angle = -Math.PI / 2 + i * 2 * Math.PI / axes;
            var end = new Point(
                center.X + radius * Math.Cos(angle),
                center.Y + radius * Math.Sin(angle));

            ctx.DrawLine(pen, center, end);
        }
    }

    private void DrawPolygon(DrawingContext ctx, Point center, double radius)
    {
        var geo = new StreamGeometry();
        using var g = geo.Open();

        int axes = Values.Length;

        for (int i = 0; i < axes; i++)
        {
            var angle = -Math.PI / 2 + i * 2 * Math.PI / axes;
            var value = Math.Clamp(Values[i] / MaxValue, 0, 1);
            var r = value * radius;

            var point = new Point(
                center.X + r * Math.Cos(angle),
                center.Y + r * Math.Sin(angle));

            if (i == 0)
                g.BeginFigure(point, true);
            else
                g.LineTo(point);
        }

        g.EndFigure(true);

        var fill = new SolidColorBrush(Color.FromArgb(120, 102, 204, 255));
        var stroke = new Pen(
            new SolidColorBrush(Color.FromRgb(102, 204, 255)), 2);

        ctx.DrawGeometry(fill, stroke, geo);
    }

    private void DrawLabels(DrawingContext ctx, Point center, double radius)
    {
        int axes = Labels.Length;

        for (int i = 0; i < axes; i++)
        {
            var angle = -Math.PI / 2 + i * 2 * Math.PI / axes;

            var pos = new Point(
                center.X + (radius + 18) * Math.Cos(angle),
                center.Y + (radius + 18) * Math.Sin(angle));

            var text = new FormattedText(
                Labels[i],
                CultureInfo.InvariantCulture,
                FlowDirection.LeftToRight,
                new Typeface("Segoe UI"),
                14,
                Brushes.White);

            // Center the text manually
            var drawPoint = new Point(
                pos.X - text.Width / 2,
                pos.Y - text.Height / 2);

            ctx.DrawText(text, drawPoint);
        }
    }

}
