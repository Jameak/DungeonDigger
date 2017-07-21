using System;
using System.Collections.Generic;
using System.Diagnostics;
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
using DungeonDigger.UI.ViewModels;

namespace DungeonDigger.UI.Controls
{
    public partial class TileControl : UserControl
    {
        /// <summary>
        /// The tile size at startup. Should be immediately overwritten by automatic resizing by parent controls.
        /// </summary>
        public const int TILE_SIZE_AT_STARTUP = 16;

        private static readonly Color SelectionColor = Colors.SkyBlue;
        private const int MaxMarginSize = 8;

        private readonly TileControlViewModel _vm;
        private int _marginSize = 1;
        public readonly int X;
        public readonly int Y;

        public Tile Tile { get; private set; }
        public bool Selected { get; private set; }
        
        public TileControl(Tile tile, int x, int y)
        {
            X = x;
            Y = y;
            _vm = new TileControlViewModel();

            InitializeComponent();
            DataContext = _vm;
            
            SizeChanged += (sender, args) =>
            {
                var margin = (int) Math.Floor(ActualHeight / 30 + 1);
                _marginSize = margin < MaxMarginSize ? margin : MaxMarginSize;
                if (Selected) _vm.MarginSize = _marginSize;
            };

            SetTile(tile);
        }

        public void Select()
        {
            _vm.GridBackground = SelectionColor;
            _vm.MarginSize = _marginSize;
            Selected = true;
        }

        public void Deselect()
        {
            _vm.GridBackground = Colors.Transparent;
            _vm.MarginSize = 0;
            Selected = false;
        }

        public void SetTile(Tile tile)
        {
            Image.Source = TileHelper.GetTileImage(tile);
            Tile = tile;
        }
    }
}
