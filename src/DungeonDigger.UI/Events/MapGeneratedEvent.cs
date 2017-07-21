using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using DungeonDigger.Generation;

namespace DungeonDigger.UI.Events
{
    public class MapGeneratedEvent : RoutedEventArgs
    {
        public Tile[,] Map { get; }

        public MapGeneratedEvent(RoutedEvent routedEvent, Tile[,] map) : base(routedEvent)
        {
            Map = map;
        }
    }
}
