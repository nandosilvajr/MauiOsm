using Mapsui.UI;

namespace MauiMaps.Controls;

public interface IMapControlSample 
{
    string Name { get; }
    string Category { get; }
    void Setup(IMapControl mapControl);
    
}
