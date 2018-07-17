using System;
using System.Collections.Generic;
using System.Text;

namespace IndoorNavigation.Models
{
    public class LocationInformation
    {
        public string Name { get; set; }
        public Guid UUID { get; set; }
        public int RSSI { get; set; }
        public int Floor { get; set; }
    }

    public class LocationAssociation
    {
        public Guid LocationA { get; set; }
        public Guid LocatiobB { get; set; }
    }
}
