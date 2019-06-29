using System;
using System.Collections.Generic;
using IndoorNavigation.Models.NavigaionLayer;

namespace IndoorNavigation.Models
{   
    public class Waypoint
    {
        public Guid _id { get; set; }
        public string _name { get; set; }
        public LocationType _type { get; set; }
        public CategoryType _category { get; set; }
        public List<Guid> _neighbors { get; set; }
        public double _lon { get; set; }
        public double _lat { get; set; }
    }    
}
