using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using DungeonDigger.UI.ViewModels.Base;

namespace DungeonDigger.UI.ViewModels
{
    public class MapControlViewModel : BaseViewModel
    {
        private WriteableBitmap _tileBitmap;
        public WriteableBitmap TileBitmap { get => _tileBitmap; set { if (_tileBitmap != value) { _tileBitmap = value; OnPropertyChanged(); } } }
    }
}
