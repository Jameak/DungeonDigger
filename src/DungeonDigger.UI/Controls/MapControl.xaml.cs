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
using DungeonDigger.UI.ViewModels;

namespace DungeonDigger.UI.Controls
{
    public partial class MapControl : UserControl
    {
        private readonly TileControl[,] _tiles;
        private bool _dragging;
        private Point? _dragStartPoint;
        private double _tileWidth;
        private List<TileControl> _selectedTiles;
        private List<TileControl> _previousSelection;

        public MapControl(TempLocationEnum[,] map)
        {
            InitializeComponent();
            _tiles = new TileControl[map.GetLength(0),map.GetLength(1)];

            //Run through the given map, creating each tile and adding it to the UI grid and the _tiles array.
            for (int i = 0; i < map.GetLength(0); i++)
            {
                MapGrid.ColumnDefinitions.Add(new ColumnDefinition());
            }

            for (int i = 0; i < map.GetLength(1); i++)
            {
                MapGrid.RowDefinitions.Add(new RowDefinition());
            }
            
            for (int i = 0; i < map.GetLength(0); i++)
            {
                for (int j = 0; j < map.GetLength(1); j++)
                {
                    var elem = new TileControl(map[i, j], i, j);
                    Grid.SetColumn(elem, i);
                    Grid.SetRow(elem, j);
                    MapGrid.Children.Add(elem);
                    _tiles[i, j] = elem;
                }
            }

            //Set the startup size of the grid. This should be immediately overwritten by the SizeChanged event firing, but this ensures we have a valid startup configuration.
            MapGrid.Width = map.GetLength(0) * TileControl.TILE_SIZE_AT_STARTUP;
            MapGrid.Height = map.GetLength(1) * TileControl.TILE_SIZE_AT_STARTUP;
            _tileWidth = TileControl.TILE_SIZE_AT_STARTUP;

            //When the control is resized, resize the internal grid to fill the available space without stretching.
            SizeChanged += (sender, args) =>
            {
                double widthMult = args.NewSize.Width / map.GetLength(0);
                double heightMult = args.NewSize.Height / map.GetLength(1);
                double mult = Math.Min(widthMult, heightMult);
                var width = (int) Math.Floor(map.GetLength(0) * mult);
                var height = (int) Math.Floor(map.GetLength(1) * mult);
                MapGrid.Width = width;
                MapGrid.Height = height;
                _tileWidth = mult;
            };
        }

        private void MapGrid_OnQueryCursor(object sender, QueryCursorEventArgs e)
        {
            //Not a drag, so dont do anything.
            if (!_dragging) return;

            //If we haven't defined a start point, then the user started "dragging" from outside of the control, which means the drag action is invalid
            if (!_dragStartPoint.HasValue)
            {
                CancelDrag();
                return;
            }

            var endPos = e.GetPosition(MapGrid);
            //User moved their mouse out of the control while dragging, so cancel the drag.
            if (endPos.X - double.Epsilon < 0.0 && endPos.X + double.Epsilon > MapGrid.Width && endPos.Y - double.Epsilon < 0.0 && endPos.Y + double.Epsilon > MapGrid.Height)
            {
                CancelDrag();
                return;
            }

            SelectTiles(_dragStartPoint.Value, endPos);
        }

        private void MapGrid_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            _dragging = true;
            _dragStartPoint = e.GetPosition(MapGrid);
            
            if (!(Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl)))
            {
                _previousSelection = null;
            }
        }

        private void MapGrid_OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            //If we dont have a start point, then the user started dragging from outside of the control, which means the drag action is invalid
            if (!_dragStartPoint.HasValue)
            {
                CancelDrag();
                return;
            }
            
            var endPos = e.GetPosition(MapGrid);
            //User moved their mouse out of the control while dragging, so cancel the drag.
            if (endPos.X - double.Epsilon < 0.0 && endPos.X + double.Epsilon > MapGrid.Width && endPos.Y - double.Epsilon < 0.0 && endPos.Y + double.Epsilon > MapGrid.Height)
            {
                CancelDrag();
                return;
            }

            SelectTiles(_dragStartPoint.Value, endPos);
            _previousSelection = GetSelectedTiles();
            StopDrag();
        }

        private void MapGrid_OnMouseLeave(object sender, MouseEventArgs e)
        {
            if (_dragging) CancelDrag();
        }

        private void StopDrag()
        {
            _dragStartPoint = null;
            _dragging = false;
        }

        private void CancelDrag()
        {
            StopDrag();
            ResetSelection();

            //If a previous selection exists, then the user had pressed ctrl when the drag started, so cancel the current drag while keeping the previous selection.
            if (_previousSelection != null)
            {
                foreach (var tile in _previousSelection)
                {
                    tile.Select();
                }
            }
        }

        private void ResetSelection()
        {
            for (int i = 0; i < _tiles.GetLength(0); i++)
            {
                for (int j = 0; j < _tiles.GetLength(1); j++)
                {
                    _tiles[i, j].Deselect();
                }
            }
            _selectedTiles = null;
        }

        private void SelectTiles(Point start, Point end)
        {
            Debug.Assert(start.X + double.Epsilon > 0.0 && start.X - double.Epsilon < MapGrid.Width, "Start point X-value outside of expected range");
            Debug.Assert(start.Y + double.Epsilon > 0.0 && start.Y - double.Epsilon < MapGrid.Height, "Start point Y-value outside of expected range");
            Debug.Assert(end.X + double.Epsilon > 0.0 && end.X - double.Epsilon < MapGrid.Width, "End point X-value outside of expected range");
            Debug.Assert(end.Y + double.Epsilon > 0.0 && end.Y - double.Epsilon < MapGrid.Height, "End point Y-value outside of expected range");
            
            ResetSelection();

            //For each index: If the end point is in the positive direction (seen from the perspective of our coordinate system) compared to our start point, then
            //  we should round our index _down_, otherwise we round _up_.
            //This has the effect of always selecting the index that the mouse is hovering over as soon as the mouse touches 1 pixel of that tile.
            //  If we did Math.Round instead, the cursor would have to "select" more than 50% of the square before it was highlighted.
            var startIndexX = start.X < end.X
                ? (int) Math.Floor(start.X / _tileWidth)
                : (int) Math.Floor(start.X / _tileWidth + 1);
            var startIndexY = start.Y < end.Y
                ? (int)Math.Floor(start.Y / _tileWidth)
                : (int)Math.Floor(start.Y / _tileWidth + 1);
            var endIndexX = end.X < start.X
                ? (int)Math.Floor(end.X / _tileWidth)
                : (int)Math.Floor(end.X / _tileWidth + 1);
            var endIndexY = end.Y < start.Y
                ? (int)Math.Floor(end.Y / _tileWidth)
                : (int)Math.Floor(end.Y / _tileWidth + 1);
            
            //The differences between the indexes making up the corners of the selected area can be either positive or negative, depending on where the start and end point are located.
            //  So figure out the x- and y-index for the top-left corner (minX,minY) and bottom-right corner (maxX,maxY) and then just loop over the difference.
            var minX = Math.Min(startIndexX, endIndexX);
            var minY = Math.Min(startIndexY, endIndexY);
            var maxX = Math.Max(startIndexX, endIndexX);
            var maxY = Math.Max(startIndexY, endIndexY);
            
            //Due to floating point imprecision when the control is resized to fit the size of the screen, there is a minor chance that the calculated index will be barely outside of the area, so to be safe we clamp it here.
            if (maxX > _tiles.GetLength(0)) maxX = _tiles.GetLength(0);
            if (maxY > _tiles.GetLength(1)) maxY = _tiles.GetLength(1);

            //Rounding might cause us to get indexes with the same value, which we still want to mark as a selection.
            //This rounding will cause the index to be larger than required, so subtract 1 when using the index directly.
            //minX and minY should always be bigger than 0 due to this rounding, but the nested if-else statement is included just to be on the safe side.
            if (minX == maxX && minY != maxY)
            {
                if (minX > 0)
                {
                    for (int j = minY; j < maxY; j++)
                    {
                        _tiles[minX - 1, j].Select();
                    }
                }
                else
                {
                    for (int j = minY; j < maxY; j++)
                    {
                        _tiles[0, j].Select();
                    }
                }
            }
            else if (minX != maxX && minY == maxY)
            {
                if (minY > 0)
                {
                    for (int i = minX; i < maxX; i++)
                    {
                        _tiles[i, minY - 1].Select();
                    }
                }
                else
                {
                    for (int i = minX; i < maxX; i++)
                    {
                        _tiles[i, 0].Select();
                    }
                }
            }
            else if (minX == maxX && minY == maxY)
            {
                if (minX > 0 && minY > 0)
                {
                    _tiles[minX - 1, minY - 1].Select();
                }
                else
                {
                    _tiles[0, 0].Select();
                }
            }
            else
            {
                //Proper drag that spans multiple tiles in both the x- and y-axis
                for (int i = minX; i < maxX; i++)
                {
                    for (int j = minY; j < maxY; j++)
                    {
                        _tiles[i, j].Select();
                    }
                }
            }

            if ((Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl)) && _previousSelection != null)
            {
                foreach (var tile in _previousSelection)
                {
                    tile.Select();
                }
            }
        }

        public List<TileControl> GetSelectedTiles()
        {
            if (_selectedTiles == null)
            {
                _selectedTiles = new List<TileControl>();
                foreach (var element in MapGrid.Children)
                {
                    if (element is TileControl tile && tile.Selected)
                    {
                        _selectedTiles.Add(tile);
                    }
                }
            }

            return _selectedTiles;
        }
    }
}
