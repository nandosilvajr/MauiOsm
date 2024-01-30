namespace MauiMapsui.Shared.Models
{
    public class LocationPoint
    {
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public string ToString() => Latitude + "," + Longitude;
    }
}