using System;
using System.Collections.Generic;
using System.Text;

namespace IndoorNavigation.Models
{
    public class NavigationEvent
    {
        public event EventHandler EventHandler;

        public void OnEventCall(EventArgs e)
        {
            EventHandler?.Invoke(this, e);
        }
    }

}
