using Mapsui.Controls;
using Mapsui.Nts.Editing;
using Mapsui.UI;

#pragma warning disable IDISP001 // Dispose created

namespace MauiMaps.Controls;

public class EditingAddPolygonSample : IMapControlSample
{
    public string Name => "Editing Add Polygon";
    public string Category => "Editing";
    public void Setup(IMapControl mapControl)
    {
        EditingSample.InitEditMode(mapControl, EditMode.AddPolygon);
    }
}
