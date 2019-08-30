using System;
using System.Collections.Generic;
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
                Guid tempGuid= new Guid();
                string tempLandmark = "";
                CardinalDirection tempRelatedDirection;
                XmlElement xmlElement = (XmlElement)xmlNode;
                tempGuid = Guid.Parse(xmlElement.GetAttribute("id"));
 
                tempLandmark = xmlElement.GetAttribute("Landmark").ToString();
                tempRelatedDirection = (CardinalDirection)Enum.Parse(typeof(CardinalDirection),
                                                  xmlElement.GetAttribute("RelatedDirection"),
                                                  false);
                _landmark.Add(tempGuid, tempLandmark);
                _relatedDirection.Add(tempGuid, tempRelatedDirection);
                
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
