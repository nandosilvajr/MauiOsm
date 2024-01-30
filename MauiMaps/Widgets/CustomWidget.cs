using Mapsui;
using Mapsui.Styles;
using Mapsui.Widgets;
using Color = Mapsui.Styles.Color;
using HorizontalAlignment = Mapsui.Widgets.HorizontalAlignment;
using VerticalAlignment = Mapsui.Widgets.VerticalAlignment;

namespace MauiMaps.Widgets;

public class CustomWidget : IWidget
{
    public HorizontalAlignment HorizontalAlignment { get; set; }
    public VerticalAlignment VerticalAlignment { get; set; }
    public float MarginX { get; set; } = 20;
    public float MarginY { get; set; } = 20;
    public MRect? Envelope { get; set; }
    public bool HandleWidgetTouched(Navigator navigator, MPoint position)
    {
        navigator.CenterOn(0, 0);
        return true;
    }

    public Color? Color { get; set; }
    public int Width { get; set; }
    public int Height { get; set; }
    public bool Enabled { get; set; } = true;
}
