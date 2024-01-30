
using System.Net;
using System.Reflection;
using BruTile.Predefined;
using BruTile.Web;
using Itinero;
using Itinero.Algorithms;
using Mapsui;
using Mapsui.Extensions;
using Mapsui.Layers;
using Mapsui.Layers.AnimatedLayers;
using Mapsui.Nts;
using Mapsui.Nts.Extensions;
using Mapsui.Projections;
using Mapsui.Samples.Maui.ViewModel;
using Mapsui.Styles;
using Mapsui.Tiling;
using Mapsui.UI.Maui;
using Mapsui.Utilities;
using Mapsui.Widgets;
using Mapsui.Widgets.ButtonWidget;
using MauiMaps.Controls;
using MauiMaps.Models;
using MauiMaps.Providers;
using MauiMaps.Services;
using MauiMaps.Widgets;
using MauiMapsui.Shared.Models;
using NetTopologySuite.Geometries;
using Newtonsoft.Json;
using Brush = Mapsui.Styles.Brush;
using Color = Mapsui.Styles.Color;
using Coordinate = NetTopologySuite.Geometries.Coordinate;
using HorizontalAlignment = Mapsui.Widgets.HorizontalAlignment;
using Map = Mapsui.Map;
using Position = Mapsui.UI.Maui.Position;
using TappedEventArgs = Mapsui.UI.TappedEventArgs;
using VerticalAlignment = Mapsui.Widgets.VerticalAlignment;

namespace MauiMaps;

public partial class MainPage : ContentPage
{
    protected readonly IItineroServiceAPI _itineroServiceApi;
    
    MainViewModel vm => BindingContext as MainViewModel;
    ItineroRoute ItineroRoute { get; set; } = new();

    private DeliveryPointProvider _deliveryPointProvider;

    private List<NetTopologySuite.Geometries.Coordinate> _cordinates = new();
    
    private readonly ObservableRangeCollection<Callout> _callouts = new ObservableRangeCollection<Callout>();

    private double _distance = 0.0f;

    private double _time = 0.0f;

    private Pin _startPin = new Pin();
    
    private Pin _endPin = new Pin();
    
    private LineString _lineString;
    
    readonly MapControl _mapControl = new MapControl();

    public bool GetDirections { get; set; }
    public MainPage(IItineroServiceAPI itineroServiceApi, MainViewModel vm)
    {
        BindingContext = vm;
        _itineroServiceApi = itineroServiceApi;

        InitializeComponent();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        
        _mapControl.SetBinding(MapControl.MapProperty, new Binding(nameof(MainViewModel.Map)));

        // Workaround. Samples need the MapControl in the current setup.
        vm.MapControl = _mapControl;

        // The CustomWidgetSkiaRenderer needs to be registered to make the CustomWidget sample work.
        // Perhaps it is possible to let the sample itself do this so we do not have to do this for each platform.
        _mapControl.Renderer.WidgetRenders[typeof(CustomWidget)] = new CustomWidgetSkiaRenderer();
        
        mapView.IsZoomButtonVisible = false;
        mapView.IsMyLocationButtonVisible = true;
        mapView.IsNorthingButtonVisible = false;

        mapView.Map = await CreateMap();

        // Set Lisbon coordinates
        var location = new Microsoft.Maui.Devices.Sensors.Location(38.736946, -9.142685);
        
        mapView.MyLocationLayer.UpdateMyLocation(new Position(location.Latitude, location.Longitude));
        mapView.Map.Navigator.CenterOnAndZoomTo(SphericalMercator.FromLonLat(location.Longitude, location.Latitude).ToMPoint(), 9);
        
        mapView.PinClicked += MapViewOnPinClicked;
        
        mapView.Loaded += MapViewOnLoaded;
        
       var grid = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) },
                new ColumnDefinition { Width = new GridLength(2, GridUnitType.Star) }
            },
        };
       
        //Layout.Add(_mapControl);

    }

    private void MapViewOnLoaded(object sender, EventArgs e)
    {

        var mapControl = new EditingAddPolygonSample();
        
        mapControl.Setup(_mapControl);
    }

    private void MapViewOnPinClicked(object sender, PinClickedEventArgs e)
    {
            e.Pin.ShowCallout();
    }

    public async Task<Map> CreateMap()
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
        
        map.Info += (s, e) =>
        {
            if (e.MapInfo?.WorldPosition == null) return;
            if (GetDirections)
            {
                          // Add a point to the layer using the Info position
                          if (_cordinates.Count < 2)
                          {
                              _cordinates.Add(new NetTopologySuite.Geometries.Point(e.MapInfo.WorldPosition.X, e.MapInfo.WorldPosition.Y).Coordinate);
                              if (_cordinates.Count == 1)
                              {
                                  _startPin.Position = new Position(
                                      SphericalMercator.ToLonLat(e.MapInfo.WorldPosition.X, e.MapInfo.WorldPosition.Y).lat,
                                      SphericalMercator.ToLonLat(e.MapInfo.WorldPosition.X, e.MapInfo.WorldPosition.Y).lon);
                                  
                                  _startPin.Callout.CalloutClicked += CalloutOnCalloutClicked;
                                  _startPin.Callout.Type = CalloutType.Detail;
                                  _startPin.Callout.TitleFontAttributes = FontAttributes.None;
                                  _startPin.Callout.TitleFontSize = 12;
                                  _startPin.Callout.SubtitleFontAttributes = FontAttributes.None;
                                  _startPin.Callout.Padding = 8;
                                  _startPin.Callout.IsClosableByClick = true;
                                  _startPin.Callout.Content = 1;
                                  _startPin.Callout.Subtitle = "Start pin subtitle";
              
                                  _startPin.Callout.RectRadius = 5;
                                  mapView.Pins.Add(_startPin);
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
                                  mapView.Pins.Add(_endPin);
              
                              }
                          }  
            }

        };
        return map;
    }

    private void CalloutOnCalloutClicked(object sender, CalloutClickedEventArgs e)
    {
        throw new NotImplementedException();
    }

    public async Task<ILayer> CreateLineStringLayer(List<Coordinate> cordinates, IStyle style = null)
    {
        RouteRequest routeRequest = new RouteRequest
        {
            Profile = QueryVehicles.car.ToString(),
            StartPoint = new LocationPoint
            {
                Latitude = SphericalMercator.ToLonLat(_cordinates.First().X, _cordinates.First().Y ).lat,
                Longitude = SphericalMercator.ToLonLat(_cordinates.First().X, _cordinates.First().Y ).lon
            },
            EndPoint = new LocationPoint
            {
                Latitude = SphericalMercator.ToLonLat(_cordinates.Last().X, _cordinates.Last().Y ).lat,
                Longitude = SphericalMercator.ToLonLat(_cordinates.Last().X, _cordinates.Last().Y ).lon
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
            Console.WriteLine(e);
            
            await Application.Current.MainPage.DisplayAlert("Error", "Was not possible request the direction", "Ok");

            return null;
        }

        
        var coordinatesLine = new List<Coordinate>();

        //_distance = ItineroRoute.Features.Where(x => x.Geometry.Type.Equals(GeometryType.LineString)).Select(x => double.Parse(x.Properties.Distance)).Sum();
        //_time = ItineroRoute.Features.Where(x => x.Geometry.Type.Equals(GeometryType.LineString)).Select(x => double.Parse(x.Properties.Time)).Sum();
        
        if (ItineroRoute?.Features?.Any() is true)
        {
            foreach (var feature in ItineroRoute.Features)
            {
                if (feature.Geometry.Type is GeometryType.LineString)
                {
                    var coordinates = JsonConvert.DeserializeObject<List<List<double>>>(feature?.Geometry?.Coordinates?.ToString());
                    foreach (var list in coordinates )
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
    public static IStyle CreateLineStringStyle()
    {
        return new VectorStyle
        {
            Fill = null,
            Outline = null,
#pragma warning disable CS8670 // Object or collection initializer implicitly dereferences possibly null member.
            Line = { Color = Color.FromString("Black"), Width = 4 }
        };
    }
    private static IWidget CreateHyperlink(string text, VerticalAlignment verticalAlignment, HorizontalAlignment horizontalAlignment)
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
    private async void GoBtn_OnClicked(object sender, EventArgs e)
    {
        if (_cordinates.Count != 2) return;
        
        var lineStringLayer = await CreateLineStringLayer(_cordinates, CreateLineStringStyle());

        if (lineStringLayer is null) return;
        
        if (mapView.Map.Layers.Count == 0) return;
        
        var pinLayers = mapView.Map?.Layers.FirstOrDefault(x => x.Name.Equals("Pins"));
        var pinIndex = mapView.Map?.Layers.ToList().IndexOf(pinLayers);
        
        mapView.Map?.Layers.Insert(pinIndex.Value -1 ,lineStringLayer);
        mapView.Map.Navigator.CenterOnAndZoomTo(lineStringLayer.Extent!.Centroid, 16);

        if (_lineString is not null)
        {
            mapView.Map.Layers.Add(new AnimatedPointLayer( _deliveryPointProvider = new DeliveryPointProvider(_lineString))
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
    }

    private void ClearBtn_OnClicked(object sender, EventArgs e)
    {
        if (_cordinates.Any())
        {
            _cordinates.Clear();
            mapView.Map.Layers.Remove(x => x.Name.Equals("LineStringLayer"));
            mapView.Map.Layers.Remove(x => x.Name.Equals("Delivery"));
           _deliveryPointProvider?.Dispose();
            if (mapView.Pins.Any())
            {
                var startPin = mapView.Pins.FirstOrDefault(x => (bool)x?.Label?.Equals("Start Pin"));
                if(startPin is not null)
                    mapView.Pins.Remove(startPin);
                
                var endPin = mapView.Pins.FirstOrDefault(x => (bool)x?.Label?.Equals("End Pin"));
                if(endPin is not null)
                    mapView.Pins.Remove(endPin);
            }
        }
    }

    private void CheckBox_OnCheckedChanged(object sender, CheckedChangedEventArgs e)
    {
        GetDirections = !GetDirections;
    }
}


