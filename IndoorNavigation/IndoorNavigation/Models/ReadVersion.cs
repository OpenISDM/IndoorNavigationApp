/*
 * Copyright (c) 2019 Academia Sinica, Institude of Information Science
 *
 * License:
 *      GPL 3.0 : The content of this file is subject to the terms and
 *      conditions defined in file 'COPYING.txt', which is part of this source
 *      code package.
 *
 * Project Name:
 *
 *      IndoorNavigation
 *
 * 
 *     
 *      
 * Version:
 *
 *      1.0.0, 201911125
 * 
 * File Name:
 *
 *      ReadVersion.cs
 *
 * Abstract:
 *
 *      This file used to get the map version, the map version can help
 *      the user know if their map is too old and need to update
 *
 *
 *      
 * Authors:
 *
 *      Eric Lee, ericlee@iis.sinica.edu.tw
 *     
 *
 */

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

