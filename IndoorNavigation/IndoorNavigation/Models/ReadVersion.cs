using System;
using System.Collections.Generic;
using System.Xml;
using Xamarin.Forms;

namespace IndoorNavigation.Models.NavigaionLayer
{
    public class ReadVersion
    {
        private Dictionary<string, double> returnVersion;

        public ReadVersion(XmlDocument fileName)
        {
            XmlNodeList xmlVersion = fileName.SelectNodes("Map/Location");

            returnVersion = new Dictionary<string, double>();

            foreach (XmlNode xmlNode in xmlVersion)
            {
                string name = "";
                double version = 0;
                XmlElement xmlElement = (XmlElement)xmlNode;
                name = xmlElement.GetAttribute("name").ToString();
                version = Convert.ToDouble(xmlElement.GetAttribute("version"));
                returnVersion.Add(name, version);
            }
        }

        public double ReturnVersion(string name)
        {
            return returnVersion[name];
        }

    }
}

