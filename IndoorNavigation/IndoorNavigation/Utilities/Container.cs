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
 * File Description:
 * 
 *      
 * Version:
 *
 *      1.0.0-beta.1, 20190530
 * 
 * File Name:
 *
 *      Container.cs
 *
 * Abstract:
 *
 *      Waypoint-based navigator is a mobile Bluetooth navigation application
 *      that runs on smart phones. It is structed to support anywhere 
 *      navigation. Indoors in areas covered by different indoor positioning 
 *      system (IPS) and outdoors covered by GPS. In particilar, it can rely on 
 *      BeDIS (Building/environment Data and Information System) for indoor 
 *      positioning. Using this IPS, the navigator does not need to 
 *      continuously monitor its own position, since the IPS broadcast to the 
 *      navigator the location of each waypoint. 
 *      This version makes use of Xamarin.Forms, which is a complete 
 *      cross-platform UI tookit that runs on both iOS and Android.
 *
 * Authors:
 *
 *      Kenneth Tang, kenneth@gm.nssh.ntpc.edu.tw
 *      
 */

using System;
using System.Collections.Generic;
using System.Linq;

namespace IndoorNavigation.Modules
{
    public class Container
    {
        private Dictionary<string, List<Type>> containerDictionary 
            = new Dictionary<string, List<Type>>();

        public Container BuildContainerProvider()
        {
            return this;
        }

        public void Add(string Key, Type type)
        {
            if (containerDictionary.ContainsKey(Key))
            {
                if (containerDictionary[Key].FirstOrDefault(container => 
                type.IsAssignableFrom(container)) == null)
                    containerDictionary[Key].Add(type);
            }
            else
                containerDictionary.Add(Key, new List<Type>() { type });
        }

        public void Add<T>(string Key)
        {
            Add(Key, typeof(T));
        }

        public T Get<T>(string Key) where T : class
        {
            if (containerDictionary.ContainsKey(Key))
            {
                var type = containerDictionary[Key]
                    .FirstOrDefault(container => 
                    typeof(T).IsAssignableFrom(container));
                if (type != null)
                    return Activator.CreateInstance(type) as T;
            }

            return default(T);
        }

        public object Get(string Key)
        {
            if (containerDictionary.ContainsKey(Key))
            {
                var type = containerDictionary[Key].FirstOrDefault();
                if (type != null)
                    return Activator.CreateInstance(type);
            }

            return null;
        }

        public IEnumerable<T> Gets<T>(string Key) where T : class
        {
            if (containerDictionary.ContainsKey(Key))
            {
                var container = containerDictionary[Key]
                    .Where(c => typeof(T)
                    .IsAssignableFrom(c));
                if (container.Count() != 0)
                    foreach (var type in container)
                        yield return Activator.CreateInstance(type) as T;
            }
        }

        public IEnumerable<object> Gets(string Key)
        {
            if (containerDictionary.ContainsKey(Key))
            {
                var container = containerDictionary[Key];
                if (container.Count() != 0)
                    foreach (var type in container)
                        yield return Activator.CreateInstance(type);
            }
        }

        public void Remove<T>(string Key)
        {
            if (containerDictionary.ContainsKey(Key))
                containerDictionary[Key].Remove(typeof(T));
        }

        public void Remove(string Key)
        {
            if (containerDictionary.ContainsKey(Key))
                containerDictionary.Remove(Key);
        }
    }
}
