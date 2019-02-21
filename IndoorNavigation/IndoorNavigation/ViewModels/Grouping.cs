using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections;

namespace IndoorNavigation.ViewModels
{
    public class Grouping<TKey, TValue> : ObservableCollection<TValue>
    {
        public TKey Key { get; private set; }

        public Grouping(TKey key, IEnumerable<TValue> items)
        {
            Key = key;
            foreach (var item in items)
                Items.Add(item);
        }
    }
}
