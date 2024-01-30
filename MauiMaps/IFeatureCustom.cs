using Mapsui;

namespace MauiMaps
{
    public interface IFeatureCustom : IFeature
    {
        void Modified() { } // default implementation
    }
}