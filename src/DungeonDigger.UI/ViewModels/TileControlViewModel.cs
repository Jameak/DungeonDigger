using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using DungeonDigger.UI.ViewModels.Base;

namespace DungeonDigger.UI.ViewModels
{
    public class TileControlViewModel : BaseViewModel
    {
        private int _marginSize = 0;
        public int MarginSize { get => _marginSize; set { if (_marginSize != value) { _marginSize = value; OnPropertyChanged(); } } }

        private Color _gridBackground = Colors.Transparent;
        public Color GridBackground { get => _gridBackground; set { if (_gridBackground != value) { _gridBackground = value; OnPropertyChanged(); OnPropertyChanged(nameof(GridBackgroundBrush)); } } }
        public SolidColorBrush GridBackgroundBrush => new SolidColorBrush(_gridBackground);
    }
}
