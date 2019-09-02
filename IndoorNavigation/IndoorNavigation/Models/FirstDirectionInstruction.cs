using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;

namespace IndoorNavigation.Models.NavigaionLayer
{
    public class FirstDirectionInstruction
    {
        private Dictionary<Guid, string> _landmark;
        private Dictionary<Guid, CardinalDirection> _relatedDirection;

        public FirstDirectionInstruction(XmlDocument fileName)
        {
            _landmark = new Dictionary<Guid, string>();
            _relatedDirection = new Dictionary<Guid, CardinalDirection>();
            XmlNodeList xmlWaypoint = fileName.SelectNodes("first_direction_XML/waypoint");

            foreach(XmlNode xmlNode in xmlWaypoint)
            {
                
                string tempLandmark = "";
                CardinalDirection tempRelatedDirection;
                XmlElement xmlElement = (XmlElement)xmlNode;
                
                tempLandmark = xmlElement.GetAttribute("Landmark").ToString();
                tempRelatedDirection = (CardinalDirection)Enum.Parse(typeof(CardinalDirection),
                                                  xmlElement.GetAttribute("RelatedDirection"),
                                                  false);
                string waypointIDs = xmlElement.GetAttribute("id");
                string[] arrayWaypointIDs = waypointIDs.Split(';');
                for (int i = 0; i < arrayWaypointIDs.Count(); i++)
                {
                    Guid waypointID = new Guid();
                    waypointID = Guid.Parse(arrayWaypointIDs[i]);
                    _landmark.Add(waypointID, tempLandmark);
                    _relatedDirection.Add(waypointID, tempRelatedDirection);
                }
                
            }
        }
        public string returnLandmark(Guid currentGuid)
        {
            return _landmark[currentGuid];
        }
        public CardinalDirection returnDirection(Guid currentGuid)
        {
            return _relatedDirection[currentGuid];
        }
    }
}
