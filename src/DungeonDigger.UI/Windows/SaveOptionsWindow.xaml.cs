using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using DungeonDigger.UI.Controls;

namespace DungeonDigger.UI.Windows
{
    public partial class SaveOptionsWindow : Window
    {
        public MapControl Map { get; set; }

        public SaveOptionsWindow()
        {
            InitializeComponent();
        }

        private void CancelButton_OnClick(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void SaveButton_OnClick(object sender, RoutedEventArgs e)
        {
            MapType mapType = MapType.GM;
            if(GMType.IsChecked.Value) mapType = MapType.GM;
            else if(PlayerType.IsChecked.Value) mapType = MapType.Player;
            
            int tileSize;
            if (Size16.IsChecked.Value) tileSize = 16;
            else if (Size32.IsChecked.Value) tileSize = 32;
            else if (Size70.IsChecked.Value) tileSize = 70;
            else if (SizeCustom.IsChecked.Value)
            {
                if (!int.TryParse(SizeCustomTextBox.Text, out tileSize))
                {
                    MessageBox.Show(this, "Please input a valid custom pixel size.", "Tile size error");
                    return;
                }

                if (tileSize <= 0)
                {
                    MessageBox.Show(this, "Custom pixel size must be larger than zero.", "Tile size error");
                }
            }
            else tileSize = TileHelper.UI_MAP_TILE_PIXELSIZE;

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
                var image = GetImage(tileSize, mapType);
                BitmapHelper.SaveBitmapSource(image, dialog.FileName, TileHelper.UI_MAP_TILE_PIXELSIZE, tileSize);
            }
        }

        private BitmapSource GetImage(int tileSize, MapType mapType)
        {
            var tileMap = Map.GetMapTiles();

            //If the maptype to create is a player map, replace all secret doors with normal walls.
            if (mapType == MapType.Player)
            {
                for (int x = 0; x < tileMap.GetLength(0); x++)
                {
                    for (int y = 0; y < tileMap.GetLength(1); y++)
                    {
                        if(tileMap[x,y] == UITile.DoorSecret) tileMap[x,y] = UITile.Wall;
                    }
                }
            }

            return BitmapHelper.CreateBitmap(tileMap, tileSize);
        }

        private void SizeRadioButton_OnClick(object sender, RoutedEventArgs e)
        {
            SizeCustomTextBox.IsEnabled = false;
        }

        private void SizeCustom_OnClick(object sender, RoutedEventArgs e)
        {
            SizeCustomTextBox.IsEnabled = true;
        }

        private enum MapType
        {
            GM,
            Player
        }

        private void SizeCustomTextBox_OnPreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !int.TryParse(e.Text, out _);
        }

        private void SizeCustomTextBox_OnPasting(object sender, DataObjectPastingEventArgs e)
        {
            if (e.DataObject.GetDataPresent(typeof(string)))
            {
                if (((string)e.DataObject.GetData(typeof(string))).Contains('-'))
                {
                    e.CancelCommand();
                }

                if (!int.TryParse((string)e.DataObject.GetData(typeof(string)), out _))
                {
                    e.CancelCommand();
                }
            }
            else
            {
                e.CancelCommand();
            }
        }
    }
}
