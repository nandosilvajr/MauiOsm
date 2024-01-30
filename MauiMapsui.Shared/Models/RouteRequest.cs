namespace MauiMapsui.Shared.Models
{
    public class RouteRequest
    {
        public string? Profile { get; set; }
        public LocationPoint? StartPoint { get; set; }
        public LocationPoint? EndPoint { get; set; }
    }
}