using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using DungeonDigger.Generation;
using DungeonDigger.UI.Controls;

namespace DungeonDigger.UI
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            var map = new MapControl(TempCreateMap());
            Grid.SetColumn(map,0);
            Grid.SetRow(map,0);
            Grid.Children.Add(map);
        }

        private static Tile[,] TempCreateMap()
        {
            var locs = new Tile[30,20];
            for (int i = 0; i < locs.GetLength(0); i++)
            {
                for (int j = 0; j < locs.GetLength(1); j++)
                {
                    locs[i,j] = (j + i) % 4 == 0 ? Tile.Room : Tile.Wall;
                }
            }
            return locs;
        }
    }
}
