using Refit;

namespace MauiMaps.Models
{
    public class ItineroParams
    {
        public ItineroParams(QueryVehicles profile, string start, string end)
        {
            Profile = profile;
            Start = start;
            End = end;
        }
        
        [AliasAs("profile")]
        public QueryVehicles Profile { get; set; }
        
        [AliasAs("loc")]
        public string Start { get; set; }
        
        [AliasAs("loc")]
        public string End { get; set; }

    }
}