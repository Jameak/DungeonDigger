using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using DungeonDigger.UI.Events;
using DungeonDigger.UI.ViewModels;

namespace DungeonDigger.UI.Controls
{
    public partial class MapCustomizerControl : UserControl
    {
        private MapCustomizerViewModel _vm;
        private bool _listSelectable;

        public MapCustomizerControl()
        {
            InitializeComponent();
            _vm = new MapCustomizerViewModel();
            DataContext = _vm;
        }

        public void EnableCustomizing()
        {
            TileList.UnselectAll();
            NoSelectionNotice.Visibility = Visibility.Collapsed;
            _listSelectable = true;
        }

        public void DisableCustomizing()
        {
            NoSelectionNotice.Visibility = Visibility.Visible;
            _listSelectable = false;
        }

        private void TileList_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!_listSelectable || TileList.SelectedItem == null) return;
            
            RaiseEvent(new ChangeTileEvent(TileChangedEvent, TileList.SelectedItem as MapCustomizerViewModel.TileInfo));
        }

        #region Events
        public static readonly RoutedEvent TileChangedEvent =
            EventManager.RegisterRoutedEvent(nameof(TileChangedEvent), RoutingStrategy.Bubble,
                typeof(RoutedEventHandler), typeof(MapCustomizerControl));

        public event RoutedEventHandler TileChanged
        {
            add { AddHandler(TileChangedEvent, value); }
            remove { RemoveHandler(TileChangedEvent, value); }
        }
        #endregion

    }
}
