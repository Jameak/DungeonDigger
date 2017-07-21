using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using DungeonDigger.UI.ViewModels;

namespace DungeonDigger.UI.Events
{
    public class ChangeTileEvent : RoutedEventArgs
    {
        public MapCustomizerViewModel.TileInfo TileInfo { get; }

        public ChangeTileEvent(RoutedEvent routedEvent, MapCustomizerViewModel.TileInfo tileInfo) : base(routedEvent)
        {
            TileInfo = tileInfo;
        }
    }
}
