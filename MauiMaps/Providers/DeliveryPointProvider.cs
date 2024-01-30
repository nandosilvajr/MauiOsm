using System.Diagnostics;
using Itinero.Profiles.Lua.Debugging;
using Mapsui;
using Mapsui.Extensions;
using Mapsui.Fetcher;
using Mapsui.Layers;
using Mapsui.Projections;
using Mapsui.Providers;
using NetTopologySuite.Geometries;

namespace MauiMaps.Providers
{
    internal sealed class DeliveryPointProvider : MemoryProvider, IDynamic, IDisposable
    {
        public event DataChangedEventHandler? DataChanged;

        private readonly PeriodicTimer _timer = new PeriodicTimer(TimeSpan.FromSeconds(1));

        private int _lastCoordinate = 0;

        private int _firstCoordinate = 0;

        private LineString _lineString;
        public DeliveryPointProvider(LineString lineString)
        {
            _lineString = lineString;
            _lastCoordinate = lineString.Coordinates.Length;
            _previousCoordinates = (
                SphericalMercator.ToLonLat(_lineString.Coordinates.First().X, _lineString.Coordinates.First().Y).lon,
                SphericalMercator.ToLonLat(_lineString.Coordinates.First().X, _lineString.Coordinates.First().Y).lat);
            Catch.TaskRun(RunTimerAsync);
        }

        private (double Lon, double Lat) _previousCoordinates;
        private async Task RunTimerAsync()
        {
            while (_firstCoordinate < _lastCoordinate)
            {
                await _timer.WaitForNextTickAsync();
                var coordinate = _firstCoordinate++;
                var lat = SphericalMercator.ToLonLat(_lineString.Coordinates[coordinate].X, _lineString.Coordinates[coordinate].Y ).lat;
                var lon = SphericalMercator.ToLonLat(_lineString.Coordinates[coordinate].X, _lineString.Coordinates[coordinate].Y ).lon;
                _previousCoordinates = (lon, lat);

                OnDataChanged();
            }
        }

        void IDynamic.DataHasChanged()
        {
            OnDataChanged();
        }

        private void OnDataChanged()
        {
            DataChanged?.Invoke(this, new DataChangedEventArgs(null, false, null));
        }

        public override Task<IEnumerable<IFeature>> GetFeaturesAsync(FetchInfo fetchInfo)
        {
            var busFeature = new PointFeature(SphericalMercator.FromLonLat(_previousCoordinates.Lon, _previousCoordinates.Lat).ToMPoint());
            busFeature["ID"] = "bus";
            return Task.FromResult((IEnumerable<IFeature>)new[] { busFeature });
        }

        public void Dispose()
        {
            _timer.Dispose();
        }
    }
}