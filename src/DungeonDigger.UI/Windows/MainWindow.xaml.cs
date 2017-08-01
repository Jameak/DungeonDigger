using System;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using DungeonDigger.Generation;
using DungeonDigger.UI.Controls;
using DungeonDigger.UI.Events;

namespace DungeonDigger.UI.Windows
{
    public partial class MainWindow : Window
    {
        private MapControl _map;

        public MainWindow()
        {
            InitializeComponent();
            SetMap(TempCreateMap());
        }

        private void SetMap(Tile[,] tiles)
        {
            if (_map != null)
            {
                _map.AreaSelectionChanged -= MapControl_OnAreaSelectionChanged;
                MainGrid.Children.Remove(_map);
            }

            _map = new MapControl(TileHelper.Convert(tiles));
            _map.AreaSelectionChanged += MapControl_OnAreaSelectionChanged;

            Grid.SetColumn(_map, 0);
            Grid.SetRow(_map, 0);
            MainGrid.Children.Add(_map);
        }

        private static Tile[,] TempCreateMap()
        {
            var rand = new Random();

            var locs = new Tile[rand.Next(5,40),rand.Next(5,40)];
            for (int i = 0; i < locs.GetLength(0); i++)
            {
                for (int j = 0; j < locs.GetLength(1); j++)
                {
                    locs[i,j] = (j + i) % rand.Next(1,5) == 0 ? Tile.Room : Tile.Wall;
                }
            }
            return locs;
        }

        private void MapCustomizerControl_OnTileChanged(object sender, RoutedEventArgs e)
        {
            var arg = (ChangeTileEvent) e;
            _map.SetTile(_map.GetSelectedTiles(), arg.TileInfo.Tile);
        }

        private void MapControl_OnAreaSelectionChanged(object sender, RoutedEventArgs e)
        {
            var arg = (AreaSelectionChangedEvent)e;
            if (arg.TilesSelected)
            {
                CustomizerTab.EnableCustomizing();
            }
            else
            {
                CustomizerTab.DisableCustomizing();
            }
        }

        private void GenerationControl_OnMapGenerated(object sender, RoutedEventArgs e)
        {
            var arg = (MapGeneratedEvent) e;
            SetMap(arg.Map);
        }

        private void MenuItemImportTsv_OnClick(object sender, RoutedEventArgs e)
        {
            var dialog = new Microsoft.Win32.OpenFileDialog()
            {
                Title = "Select TSV-formatted file to load",
                Filter = "Supported files|*.tsv;*.txt",
                Multiselect = false
            };

            var result = dialog.ShowDialog();

            if (result != null && result.Value)
            {
                using (var stream = new StreamReader(dialog.OpenFile()))
                {
                    SetMap(TsvParser.Parse(stream));
                }

            }
        }

        private void MenuItemSaveMap_OnClick(object sender, RoutedEventArgs e)
        {
            var dialog = new Microsoft.Win32.SaveFileDialog
            {
                Title = "Choose save location",
                FileName = "dungeon",
                DefaultExt = ".png",
                AddExtension = true,
                Filter = "All Files|*.*"
            };

            var result = dialog.ShowDialog();

            if (result != null && result.Value)
            {
                if (!dialog.FileName.EndsWith(".png")) dialog.FileName = dialog.FileName + ".png";
                BitmapHelper.SaveBitmapSource(_map.UIImage, dialog.FileName);
            }
        }
    }
}
