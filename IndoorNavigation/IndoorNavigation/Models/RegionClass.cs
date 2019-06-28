using System;
using System.Collections.Generic;
using IndoorNavigation.Models.NavigaionLayer;

namespace IndoorNavigation.Models
{
    public class Region
    {
        public Guid _id { get; set; }
        public IPSType _IPSType { get; set; }
        public string _name { get; set; }
        public int _floor { get; set; }
        public List<Guid> _neighbors { get; set; }
        public Dictionary<CategoryType, List<Waypoint>> _waypointsByCategory { get; set; }
    }
}
