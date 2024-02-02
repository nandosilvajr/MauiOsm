using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Mapsui.Layers;
using Mapsui.Layers.AnimatedLayers;
using Mapsui.Nts;
using Mapsui.Nts.Extensions;
using Mapsui.Projections;
using Mapsui.Styles;
using Mapsui.Tiling;
using Mapsui.UI.Maui;
using Mapsui.Widgets;
using MauiMaps.Models;
using MauiMaps.Providers;
using MauiMaps.Services;
using MauiMapsui.Shared.Models;
using NetTopologySuite.Geometries;
using Newtonsoft.Json;
using Coordinate = NetTopologySuite.Geometries.Coordinate;
using Position = Mapsui.UI.Maui.Position;
using HorizontalAlignment = Mapsui.Widgets.HorizontalAlignment;
using VerticalAlignment = Mapsui.Widgets.VerticalAlignment;
using Brush = Mapsui.Styles.Brush;
using Color = Mapsui.Styles.Color;
using SkiaSharp.Views.Maui;
using Mapsui.Extensions;
using BruTile.Web;
using BruTile.Predefined;
using Mapsui.Tiling.Layers;
using System.Diagnostics;

#pragma warning disable IDISP008 // Don't assign member with injected and created disposables.

namespace Mapsui.ViewModels;

public partial class MainViewModel(IItineroServiceAPI itineroServiceApi) : ObservableObject
{
    protected readonly IItineroServiceAPI _itineroServiceApi = itineroServiceApi;

    private DeliveryPointProvider _deliveryPointProvider;

    private List<Coordinate> _cordinates = new();

    private Pin _startPin = new Pin();

    private Pin _endPin = new Pin();

    private LineString _lineString;

    readonly MapControl _mapControl = new MapControl();

    [ObservableProperty] public bool _directionsActive;

    [ObservableProperty] Map _map;

    [ObservableProperty] private MapView _mapView;

    [ObservableProperty] private ObservableCollection<Pin> _pins;

    [ObservableProperty] ItineroRoute _itineroRoute;

    [ObservableProperty] bool _isBusy;

    [RelayCommand]
    private async Task GetDirections()
    {
        IsBusy = true;
        if (_cordinates.Count != 2) {
            IsBusy = false;
            return;
        };

        var lineStringLayer = await CreateLineStringLayer(style: CreateLineStringStyle());

        if (lineStringLayer is null) {
            IsBusy = false; 
            return;
            }

        if (MapView.Map.Layers.Count == 0) {
            IsBusy = false;
            return;
            }

        var pinLayers = MapView.Map?.Layers.FirstOrDefault(x => x.Name.Equals("Pins"));
        var pinIndex = MapView.Map?.Layers.ToList().IndexOf(pinLayers);

        MapView.Map?.Layers.Insert(pinIndex.Value - 1, lineStringLayer);
        MapView.Map.Navigator.CenterOnAndZoomTo(lineStringLayer.Extent!.Centroid, 16);

        if (_lineString is not null)
        {
            MapView.Map.Layers.Add(
                new AnimatedPointLayer(_deliveryPointProvider = new DeliveryPointProvider(_lineString))
                {
                    Name = "Delivery",
                    Style = new LabelStyle
                    {
                        BackColor = new Brush(Color.Black),
                        ForeColor = Color.White,
                        Text = "Delivery",
                        MaxWidth = 100,
                        LineHeight = 1.5
                    }
                });
        }
        IsBusy = false;
    }

    [RelayCommand]
    private async Task ClearMap()
    {
        if (_cordinates.Any())
        {
            _cordinates.Clear();
            MapView?.Map?.Layers?.Remove(x => x.Name.Equals("LineStringLayer"));
            MapView?.Map?.Layers?.Remove(x => x.Name.Equals("Delivery"));
            _deliveryPointProvider?.Dispose();
            MapView?.Pins?.ToList().ForEach(x => x.HideCallout());
            MapView?.Pins?.Clear();
        }
    }

    internal async Task InitializeAsync()
    {
        MapView = new MapView
        {
            Map = CreateMap()
        };

        MapView.Map.Info += MapOnInfo;

        //Set to Lisbon coordinates
        MapView.Map.Home = n => {
            n.CenterOnAndZoomTo(SphericalMercator.FromLonLat(lat: 38.736946, lon:-9.142685).ToMPoint(), 16);
        };
    }
    private void MapOnInfo(object sender, MapInfoEventArgs e)
    {
        if (e.MapInfo?.WorldPosition == null) return;

        if(e.MapInfo?.Layer?.Name?.Equals("Callouts") is true){
            var calloutPosition = e.MapInfo?.MapInfoRecords?.FirstOrDefault()?.Feature as GeometryFeature;
            var calloutCoordinate = calloutPosition?.Geometry?.Coordinates?.FirstOrDefault()?.CoordinateValue;
            var pin = MapView.Pins.FirstOrDefault(x => x?.Callout?.Feature?.Geometry?.Coordinates?.FirstOrDefault().CoordinateValue == calloutCoordinate);
            pin.HideCallout();
        }

        if(e.MapInfo?.Layer?.Name?.Equals("Pins") is true){
            var position = e.MapInfo?.MapInfoRecords?.FirstOrDefault()?.Feature as GeometryFeature;
            var pinCoordinate = position?.Geometry?.Coordinates?.FirstOrDefault()?.CoordinateValue;
    
            var selectedPin = MapView?.Pins?.FirstOrDefault(x => x?.Feature?.Geometry?.Coordinates?.FirstOrDefault().CoordinateValue == pinCoordinate);
            selectedPin?.ShowCallout();
        }

        if (DirectionsActive)
        {
            // Add a point to the layer using the Info position
            if (_cordinates.Count < 2)
            {
                _cordinates.Add(
                    new NetTopologySuite.Geometries.Point(e.MapInfo.WorldPosition.X, e.MapInfo.WorldPosition.Y)
                        .Coordinate);
                if (_cordinates.Count == 1)
                {
                    _startPin.Position = new Position(
                        SphericalMercator.ToLonLat(e.MapInfo.WorldPosition.X, e.MapInfo.WorldPosition.Y).lat,
                        SphericalMercator.ToLonLat(e.MapInfo.WorldPosition.X, e.MapInfo.WorldPosition.Y).lon);

                    _startPin.Callout.Type = CalloutType.Detail;
                    _startPin.Callout.TitleFontAttributes = FontAttributes.None;
                    _startPin.Callout.TitleFontSize = 12;
                    _startPin.Callout.SubtitleFontAttributes = FontAttributes.None;
                    _startPin.Callout.Subtitle = "Start pin subtitle";
                    _startPin.Callout.RectRadius = 5;
                    MapView.Pins.Add(_startPin);
                }
                else
                {
                    _endPin.Position = new Position(
                        SphericalMercator.ToLonLat(e.MapInfo.WorldPosition.X, e.MapInfo.WorldPosition.Y).lat,
                        SphericalMercator.ToLonLat(e.MapInfo.WorldPosition.X, e.MapInfo.WorldPosition.Y).lon);

                    _endPin.Callout.Type = CalloutType.Detail;
                    _endPin.Callout.TitleFontAttributes = FontAttributes.None;
                    _endPin.Callout.TitleFontSize = 12;
                    _endPin.Callout.SubtitleFontAttributes = FontAttributes.None;
                    _endPin.Callout.Subtitle = "End pin subtitle";
                    _endPin.Callout.RectRadius = 5;
                    MapView.Pins.Add(_endPin);
                }
            }
        }
    }
    private Map CreateMap()
    {
        var map = new Map();

        _startPin = new Pin
        {
            Type = PinType.Pin,
            Label = "Start Pin",
        };
        _endPin = new Pin
        {
            Type = PinType.Pin,
            Label = "End Pin",
        };

        map.Layers.Add(OpenStreetMap.CreateTileLayer());

        return map;
    }

    private async Task<ILayer> CreateLineStringLayer(List<Coordinate> coordinate = null, IStyle style = null)
    {
        RouteRequest routeRequest = new RouteRequest
        {
            Profile = QueryVehicles.car.ToString(),
            StartPoint = new LocationPoint
            {
                Latitude = SphericalMercator.ToLonLat(_cordinates.First().X, _cordinates.First().Y).lat,
                Longitude = SphericalMercator.ToLonLat(_cordinates.First().X, _cordinates.First().Y).lon
            },
            EndPoint = new LocationPoint
            {
                Latitude = SphericalMercator.ToLonLat(_cordinates.Last().X, _cordinates.Last().Y).lat,
                Longitude = SphericalMercator.ToLonLat(_cordinates.Last().X, _cordinates.Last().Y).lon
            },
        };

        try
        {
            ItineroRoute = await _itineroServiceApi.GetRoute(
                new ItineroParams(
                    profile: QueryVehicles.car,
                    start: routeRequest.StartPoint.ToString(),
                    end: routeRequest.EndPoint.ToString())
            );
        }
        catch (Exception e)
        {
            Debug.WriteLine(e.Message);
            IsBusy = false;
            await Application.Current.MainPage.DisplayAlert("Error", "Was not possible request the direction", "Ok");
            return null;
        }

        var coordinatesLine = new List<Coordinate>();

        if (ItineroRoute?.Features?.Any() is true)
        {
            foreach (var feature in ItineroRoute.Features)
            {
                if (feature.Geometry.Type is GeometryType.LineString)
                {
                    var coordinates =
                        JsonConvert.DeserializeObject<List<List<double>>>(feature?.Geometry?.Coordinates?.ToString());
                    foreach (var list in coordinates)
                    {
                        if (list.Any())
                        {
                            coordinatesLine.Add(SphericalMercator.FromLonLat(list[0], list[1]).ToCoordinate());
                        }
                    }
                }
            }
        }

        if (coordinatesLine?.Any() is true)
        {
            _lineString = new LineString(coordinatesLine?.ToArray());

            var memoryLayer = new MemoryLayer
            {
                Features = new[] { new GeometryFeature { Geometry = _lineString } },
                Name = "LineStringLayer",
                Style = style
            };
            return memoryLayer;
        }

        return new Layer();
    }

    private static IStyle CreateLineStringStyle()
    {
        return new VectorStyle
        {
            Fill = null,
            Outline = null,
#pragma warning disable CS8670 // Object or collection initializer implicitly dereferences possibly null member.
            Line = { Color = Color.FromString("Black"), Width = 4 }
        };
    }

    private static IWidget CreateHyperlink(string text, VerticalAlignment verticalAlignment,
        HorizontalAlignment horizontalAlignment)
    {
        return new Hyperlink()
        {
            Text = text,
            Url = "http://www.openstreetmap.org/copyright",
            VerticalAlignment = verticalAlignment,
            HorizontalAlignment = horizontalAlignment,
            MarginX = 10,
            MarginY = 10,
            PaddingX = 4,
            PaddingY = 4,
            BackColor = new Color(255, 192, 203)
        };
    }
}