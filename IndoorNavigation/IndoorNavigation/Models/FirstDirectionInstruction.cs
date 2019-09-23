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
        private Dictionary<Guid, int> _faceOrBack;

        public FirstDirectionInstruction(XmlDocument fileName)
        {
            _landmark = new Dictionary<Guid, string>();
            _relatedDirection = new Dictionary<Guid, CardinalDirection>();
            _faceOrBack = new Dictionary<Guid, int>();
            XmlNodeList xmlWaypoint = fileName.SelectNodes("first_direction_XML/waypoint");

            foreach(XmlNode xmlNode in xmlWaypoint)
            {
                
                string tempLandmark = "";
                CardinalDirection tempRelatedDirection;
                int tempFaceOrBack = 0;
                XmlElement xmlElement = (XmlElement)xmlNode;
                
                tempLandmark = xmlElement.GetAttribute("Landmark").ToString();
                tempRelatedDirection = (CardinalDirection)Enum.Parse(typeof(CardinalDirection),
                                                  xmlElement.GetAttribute("RelatedDirection"),
                                                  false);
                tempFaceOrBack = Int32.Parse(xmlElement.GetAttribute("FaceOrBack"));
                string waypointIDs = xmlElement.GetAttribute("id");
                string[] arrayWaypointIDs = waypointIDs.Split(';');
                for (int i = 0; i < arrayWaypointIDs.Count(); i++)
                {
                    Guid waypointID = new Guid();
                    waypointID = Guid.Parse(arrayWaypointIDs[i]);
                    _landmark.Add(waypointID, tempLandmark);
                    _relatedDirection.Add(waypointID, tempRelatedDirection);
                    _faceOrBack.Add(waypointID, tempFaceOrBack);
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
        public int returnFaceOrBack(Guid currentGuid)
        {
            return _faceOrBack[currentGuid];
        }
    }
}
