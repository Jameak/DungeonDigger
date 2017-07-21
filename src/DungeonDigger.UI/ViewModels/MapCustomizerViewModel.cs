using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using DungeonDigger.Generation;
using DungeonDigger.UI.ViewModels.Base;

namespace DungeonDigger.UI.ViewModels
{
    public class MapCustomizerViewModel : BaseViewModel
    {
        public ObservableCollection<TileInfo> TileChoices { get; set; } = new ObservableCollection<TileInfo>();

        public MapCustomizerViewModel()
        {
            foreach (var tile in Enum.GetValues(typeof(Tile)).Cast<Tile>())
            {
                TileChoices.Add(new TileInfo
                {
                    Image = TileHelper.GetTileImage(tile),
                    Name = TileHelper.GetTileName(tile),
                    Tile = tile
                });
            }
        }

        public class TileInfo
        {
            public ImageSource Image { get; set; }
            public string Name { get; set; }
            public Tile Tile { get; set; }
        }
    }
}
