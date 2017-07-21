using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using DungeonDigger.UI.Controls;

namespace DungeonDigger.UI.Events
{
    public class AreaSelectionChangedEvent : RoutedEventArgs
    {
        public bool TilesSelected { get; }

        public AreaSelectionChangedEvent(RoutedEvent routedEvent, bool tilesSelected) : base(routedEvent)
        {
            TilesSelected = tilesSelected;
        }
    }
}
