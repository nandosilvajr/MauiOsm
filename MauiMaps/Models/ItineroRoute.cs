using System.Globalization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace MauiMaps.Models
{
     public partial class ItineroRoute
    {
        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("features")]
        public List<Feature> Features { get; set; }
    }

    public partial class Feature
    {
        [JsonProperty("type")]
        public FeatureType Type { get; set; }

        [JsonProperty("name")]
        public Name Name { get; set; }

        [JsonProperty("geometry")]
        public Geometry Geometry { get; set; }

        [JsonProperty("properties")]
        public Properties Properties { get; set; }

        [JsonProperty("Shape", NullValueHandling = NullValueHandling.Ignore)]
        public string Shape { get; set; }
    }

    public partial class Geometry
    {
        [JsonProperty("type")]
        public GeometryType Type { get; set; }
        
        [JsonProperty("coordinates")]
        public object Coordinates { get; set; }
    }

    public partial class Properties
    {
        [JsonProperty("name", NullValueHandling = NullValueHandling.Ignore)]
        public string Name { get; set; }

        [JsonProperty("highway", NullValueHandling = NullValueHandling.Ignore)]
        public string Highway { get; set; }

        [JsonProperty("oneway", NullValueHandling = NullValueHandling.Ignore)]
        public Oneway? Oneway { get; set; }

        [JsonProperty("profile", NullValueHandling = NullValueHandling.Ignore)]
        public Profile? Profile { get; set; }

        [JsonProperty("distance")]
        public string Distance { get; set; }

        [JsonProperty("time")]
        public string Time { get; set; }

        [JsonProperty("bridge", NullValueHandling = NullValueHandling.Ignore)]
        public string Bridge { get; set; }

        [JsonProperty("junction", NullValueHandling = NullValueHandling.Ignore)]
        public Junction? Junction { get; set; }

        [JsonProperty("maxspeed", NullValueHandling = NullValueHandling.Ignore)]
        public string Maxspeed { get; set; }
    }

    public enum GeometryType { LineString, Point };

    public enum Name { ShapeMeta, Stop };

    public enum Highway
    {
        Primary,
        PrimaryLink,
        Residential, 
        Secondary, 
        SecondaryLink,
        Tertiary, 
        TertiaryLink, 
        Track, 
        Unclassified,
        Service,
        Services,
        Road,
        Cycleway,
        Footway,
        Pedestrian,
        Path,
        LivingStreet,
        Ferry,
        Movable,
        ShuttleTrain,
        Default
    };

    public enum Junction { Roundabout, Circular };

    public enum Oneway { No, Yes };

    public enum Profile { Car };

    public enum FeatureType { Feature };

    public partial struct Coordinate
    {
        public double? Double;
        public List<double> DoubleArray;

        public static implicit operator Coordinate(double Double) => new Coordinate { Double = Double };
        public static implicit operator Coordinate(List<double> DoubleArray) => new Coordinate { DoubleArray = DoubleArray };
    }

    public partial class ItineroRoute
    {
        public static ItineroRoute FromJson(string json) => JsonConvert.DeserializeObject<ItineroRoute>(json, Converter.Settings);
    }

    public static class Serialize
    {
        public static string ToJson(this ItineroRoute self) => JsonConvert.SerializeObject(self, Converter.Settings);
    }

    internal static class Converter
    {
        public static readonly JsonSerializerSettings Settings = new JsonSerializerSettings
        {
            MetadataPropertyHandling = MetadataPropertyHandling.Ignore,
            DateParseHandling = DateParseHandling.None,
            Converters =
            {
                CoordinateConverter.Singleton,
                GeometryTypeConverter.Singleton,
                NameConverter.Singleton,
                HighwayConverter.Singleton,
                JunctionConverter.Singleton,
                OnewayConverter.Singleton,
                ProfileConverter.Singleton,
                FeatureTypeConverter.Singleton,
                new IsoDateTimeConverter { DateTimeStyles = DateTimeStyles.AssumeUniversal }
            },
        };
    }

    internal class ParseStringConverter : JsonConverter
    {
        public override bool CanConvert(Type t) => t == typeof(long) || t == typeof(long?);

        public override object ReadJson(JsonReader reader, Type t, object existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null) return null;
            var value = serializer.Deserialize<string>(reader);
            long l;
            if (Int64.TryParse(value, out l))
            {
                return l;
            }
            throw new Exception("Cannot unmarshal type long");
        }

        public override void WriteJson(JsonWriter writer, object untypedValue, JsonSerializer serializer)
        {
            if (untypedValue == null)
            {
                serializer.Serialize(writer, null);
                return;
            }
            var value = (long)untypedValue;
            serializer.Serialize(writer, value.ToString());
            return;
        }

        public static readonly ParseStringConverter Singleton = new ParseStringConverter();
    }

    internal class CoordinateConverter : JsonConverter
    {
        public override bool CanConvert(Type t) => t == typeof(Coordinate) || t == typeof(Coordinate?);

        public override object ReadJson(JsonReader reader, Type t, object existingValue, JsonSerializer serializer)
        {
            switch (reader.TokenType)
            {
                case JsonToken.Integer:
                case JsonToken.Float:
                    var doubleValue = serializer.Deserialize<double>(reader);
                    return new Coordinate { Double = doubleValue };
                case JsonToken.StartArray:
                    var arrayValue = serializer.Deserialize<List<double>>(reader);
                    return new Coordinate { DoubleArray = arrayValue };
            }
            throw new Exception("Cannot unmarshal type Coordinate");
        }

        public override void WriteJson(JsonWriter writer, object untypedValue, JsonSerializer serializer)
        {
            var value = (Coordinate)untypedValue;
            if (value.Double != null)
            {
                serializer.Serialize(writer, value.Double.Value);
                return;
            }
            if (value.DoubleArray != null)
            {
                serializer.Serialize(writer, value.DoubleArray);
                return;
            }
            throw new Exception("Cannot marshal type Coordinate");
        }

        public static readonly CoordinateConverter Singleton = new CoordinateConverter();
    }

    internal class GeometryTypeConverter : JsonConverter
    {
        public override bool CanConvert(Type t) => t == typeof(GeometryType) || t == typeof(GeometryType?);

        public override object ReadJson(JsonReader reader, Type t, object existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null) return null;
            var value = serializer.Deserialize<string>(reader);
            switch (value)
            {
                case "LineString":
                    return GeometryType.LineString;
                case "Point":
                    return GeometryType.Point;
            }
            throw new Exception("Cannot unmarshal type GeometryType");
        }

        public override void WriteJson(JsonWriter writer, object untypedValue, JsonSerializer serializer)
        {
            if (untypedValue == null)
            {
                serializer.Serialize(writer, null);
                return;
            }
            var value = (GeometryType)untypedValue;
            switch (value)
            {
                case GeometryType.LineString:
                    serializer.Serialize(writer, "LineString");
                    return;
                case GeometryType.Point:
                    serializer.Serialize(writer, "Point");
                    return;
            }
            throw new Exception("Cannot marshal type GeometryType");
        }

        public static readonly GeometryTypeConverter Singleton = new GeometryTypeConverter();
    }

    internal class NameConverter : JsonConverter
    {
        public override bool CanConvert(Type t) => t == typeof(Name) || t == typeof(Name?);

        public override object ReadJson(JsonReader reader, Type t, object existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null) return null;
            var value = serializer.Deserialize<string>(reader);
            switch (value)
            {
                case "ShapeMeta":
                    return Name.ShapeMeta;
                case "Stop":
                    return Name.Stop;
            }
            throw new Exception("Cannot unmarshal type Name");
        }

        public override void WriteJson(JsonWriter writer, object untypedValue, JsonSerializer serializer)
        {
            if (untypedValue == null)
            {
                serializer.Serialize(writer, null);
                return;
            }
            var value = (Name)untypedValue;
            switch (value)
            {
                case Name.ShapeMeta:
                    serializer.Serialize(writer, "ShapeMeta");
                    return;
                case Name.Stop:
                    serializer.Serialize(writer, "Stop");
                    return;
            }
            throw new Exception("Cannot marshal type Name");
        }

        public static readonly NameConverter Singleton = new NameConverter();
    }

    internal class HighwayConverter : JsonConverter
    {
        public override bool CanConvert(Type t) => t == typeof(Highway) || t == typeof(Highway?);

        public override object ReadJson(JsonReader reader, Type t, object existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null) return null;
            var value = serializer.Deserialize<string>(reader);
            switch (value)
            {
                case "primary":
                    return Highway.Primary;
                case "primary_link":
                    return Highway.PrimaryLink;
                case "residential":
                    return Highway.Residential;
                case "secondary":
                    return Highway.Secondary;
                case "secondary_link":
                    return Highway.SecondaryLink;
                case "tertiary":
                    return Highway.Tertiary;
                case "tertiary_link":
                    return Highway.TertiaryLink;
                case "track":
                    return Highway.Track;
                case "unclassified":
                    return Highway.Unclassified;
                case "service":
                    return Highway.Service;
                case "services":
                    return Highway.Services;
                case "road":
                    return Highway.Road;
                case "cycleway":
                    return Highway.Cycleway;
                case "footway":
                    return Highway.Footway;
                case "pedestrian":
                    return Highway.Pedestrian;
                case "path":
                    return Highway.Path;
                case "living_street":
                    return Highway.LivingStreet;
                case "ferry":
                    return Highway.Ferry;
                case "movable":
                    return Highway.Movable;
                case "shuttle_train":
                    return Highway.ShuttleTrain;
                case "default":
                    return Highway.Default;
            }
            throw new Exception("Cannot unmarshal type Highway");
        }

        public override void WriteJson(JsonWriter writer, object untypedValue, JsonSerializer serializer)
        {
            if (untypedValue == null)
            {
                serializer.Serialize(writer, null);
                return;
            }
            var value = (Highway)untypedValue;
            switch (value)
            {
                case Highway.Primary:
                    serializer.Serialize(writer, "primary");
                    return;
                case Highway.Residential:
                    serializer.Serialize(writer, "residential");
                    return;
                case Highway.Secondary:
                    serializer.Serialize(writer, "secondary");
                    return;
                case Highway.Tertiary:
                    serializer.Serialize(writer, "tertiary");
                    return;
                case Highway.TertiaryLink:
                    serializer.Serialize(writer, "link");
                    return;
                case Highway.Track:
                    serializer.Serialize(writer, "track");
                    return;
                case Highway.Unclassified:
                    serializer.Serialize(writer, "unclassified");
                    return;
                case Highway.Service:
                    serializer.Serialize(writer, "service");
                    return;
                case Highway.Services:
                    serializer.Serialize(writer, "services");
                    return;
                case Highway.Road:
                    serializer.Serialize(writer, "road");
                    return;
                case Highway.Cycleway:
                    serializer.Serialize(writer, "cycleway");
                    return;
                case Highway.Footway:
                    serializer.Serialize(writer, "footway");
                    return;
                case Highway.Pedestrian:
                    serializer.Serialize(writer, "pedestrian");
                    return;
                case Highway.Path:
                    serializer.Serialize(writer, "path");
                    return;
                case Highway.LivingStreet:
                    serializer.Serialize(writer, "living_street");
                    return;
                case Highway.Ferry:
                    serializer.Serialize(writer, "ferry");
                    return;
                case Highway.Movable:
                    serializer.Serialize(writer, "movable");
                    return;
                case Highway.ShuttleTrain:
                    serializer.Serialize(writer, "shuttle_train");
                    return;
                case Highway.Default:
                    serializer.Serialize(writer, "default");
                    return;
            }
            throw new Exception("Cannot marshal type Highway");
        }

        public static readonly HighwayConverter Singleton = new HighwayConverter();
    }

    internal class JunctionConverter : JsonConverter
    {
        public override bool CanConvert(Type t) => t == typeof(Junction) || t == typeof(Junction?);

        public override object ReadJson(JsonReader reader, Type t, object existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null) return null;
            var value = serializer.Deserialize<string>(reader);
            switch (value)
            {
                case "circular":
                    return Junction.Circular;
                case "roundabout":
                    return Junction.Roundabout;
            }
            throw new Exception("Cannot unmarshal type Junction");
        }

        public override void WriteJson(JsonWriter writer, object untypedValue, JsonSerializer serializer)
        {
            if (untypedValue == null)
            {
                serializer.Serialize(writer, null);
                return;
            }
            var value = (Junction)untypedValue;
            switch (value)
            {
                case Junction.Roundabout:
                    serializer.Serialize(writer, "roundabout");
                    return;
                case Junction.Circular:
                    serializer.Serialize(writer, "Circular");
                    return;
            }
            throw new Exception("Cannot marshal type Junction");
        }

        public static readonly JunctionConverter Singleton = new JunctionConverter();
    }

    internal class OnewayConverter : JsonConverter
    {
        public override bool CanConvert(Type t) => t == typeof(Oneway) || t == typeof(Oneway?);

        public override object ReadJson(JsonReader reader, Type t, object existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null) return null;
            var value = serializer.Deserialize<string>(reader);
            switch (value)
            {
                case "no":
                    return Oneway.No;
                case "yes":
                    return Oneway.Yes;
            }
            throw new Exception("Cannot unmarshal type Oneway");
        }

        public override void WriteJson(JsonWriter writer, object untypedValue, JsonSerializer serializer)
        {
            if (untypedValue == null)
            {
                serializer.Serialize(writer, null);
                return;
            }
            var value = (Oneway)untypedValue;
            switch (value)
            {
                case Oneway.No:
                    serializer.Serialize(writer, "no");
                    return;
                case Oneway.Yes:
                    serializer.Serialize(writer, "yes");
                    return;
            }
            throw new Exception("Cannot marshal type Oneway");
        }

        public static readonly OnewayConverter Singleton = new OnewayConverter();
    }

    internal class ProfileConverter : JsonConverter
    {
        public override bool CanConvert(Type t) => t == typeof(Profile) || t == typeof(Profile?);

        public override object ReadJson(JsonReader reader, Type t, object existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null) return null;
            var value = serializer.Deserialize<string>(reader);
            if (value == "car")
            {
                return Profile.Car;
            }
            throw new Exception("Cannot unmarshal type Profile");
        }

        public override void WriteJson(JsonWriter writer, object untypedValue, JsonSerializer serializer)
        {
            if (untypedValue == null)
            {
                serializer.Serialize(writer, null);
                return;
            }
            var value = (Profile)untypedValue;
            if (value == Profile.Car)
            {
                serializer.Serialize(writer, "car");
                return;
            }
            throw new Exception("Cannot marshal type Profile");
        }

        public static readonly ProfileConverter Singleton = new ProfileConverter();
    }

    internal class FeatureTypeConverter : JsonConverter
    {
        public override bool CanConvert(Type t) => t == typeof(FeatureType) || t == typeof(FeatureType?);

        public override object ReadJson(JsonReader reader, Type t, object existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null) return null;
            var value = serializer.Deserialize<string>(reader);
            if (value == "Feature")
            {
                return FeatureType.Feature;
            }
            throw new Exception("Cannot unmarshal type FeatureType");
        }

        public override void WriteJson(JsonWriter writer, object untypedValue, JsonSerializer serializer)
        {
            if (untypedValue == null)
            {
                serializer.Serialize(writer, null);
                return;
            }
            var value = (FeatureType)untypedValue;
            if (value == FeatureType.Feature)
            {
                serializer.Serialize(writer, "Feature");
                return;
            }
            throw new Exception("Cannot marshal type FeatureType");
        }

        public static readonly FeatureTypeConverter Singleton = new FeatureTypeConverter();
    }
    
    public class SingleValueArrayConverter<T> : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            object retVal = new Object();
            if (reader.TokenType == JsonToken.StartObject)
            {
                T instance = (T)serializer.Deserialize(reader, typeof(T));
                retVal = new List<T>() { instance };
            } else if (reader.TokenType == JsonToken.StartArray) {
                retVal = serializer.Deserialize(reader, objectType);
            }
            return retVal;
        }

        public override bool CanConvert(Type objectType)
        {
            return true;
        }
    }
}