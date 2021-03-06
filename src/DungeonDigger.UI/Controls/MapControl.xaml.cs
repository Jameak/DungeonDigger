﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using DungeonDigger.Generation;
using DungeonDigger.UI.Events;
using DungeonDigger.UI.ViewModels;

namespace DungeonDigger.UI.Controls
{
    public partial class MapControl : UserControl
    {
        private const double Epsilon = 0.0000001;

        private readonly MapControlViewModel _vm;
        private readonly TileStatus[,] _tiles;
        private bool _dragging;
        private Point? _dragStartPoint;
        private double _tileWidth;
        private List<TileStatus> _selectedTiles;
        private List<TileStatus> _previousSelection;

        public BitmapSource UIImage => _vm.TileBitmap;
        
        public MapControl(UITile[,] map)
        {
            InitializeComponent();
            _tiles = new TileStatus[map.GetLength(0),map.GetLength(1)];
            _vm = new MapControlViewModel();
            DataContext = _vm;

            _vm.TileBitmap = new WriteableBitmap(BitmapHelper.CreateBitmap(map, TileHelper.UI_MAP_TILE_PIXELSIZE));

            for (int x = 0; x < map.GetLength(0); x++)
            {
                for (int y = 0; y < map.GetLength(1); y++)
                {
                    _tiles[x,y] = new TileStatus(x,y){Tile = map[x,y]};
                }
            }


            //Set the startup size of the grid. This should be immediately overwritten by the SizeChanged event firing, but this ensures we have a valid startup configuration.
            MapGrid.Width = map.GetLength(0) * TileHelper.UI_MAP_TILE_PIXELSIZE;
            MapGrid.Height = map.GetLength(1) * TileHelper.UI_MAP_TILE_PIXELSIZE;
            _tileWidth = TileHelper.UI_MAP_TILE_PIXELSIZE;

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
            if (endPos.X - Epsilon < 0.0 && endPos.X + Epsilon > MapGrid.Width && endPos.Y - Epsilon < 0.0 && endPos.Y + Epsilon > MapGrid.Height)
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
            if (endPos.X - Epsilon < 0.0 && endPos.X + Epsilon > MapGrid.Width && endPos.Y - Epsilon < 0.0 && endPos.Y + Epsilon > MapGrid.Height)
            {
                CancelDrag();
                return;
            }

            SelectTiles(_dragStartPoint.Value, endPos);
            _previousSelection = GetSelectedTiles();
            StopDrag();
            RaiseEvent(new AreaSelectionChangedEvent(AreaSelectionChangedEvent, GetSelectedTiles().Count != 0));
        }

        private void MapGrid_OnMouseLeave(object sender, MouseEventArgs e)
        {
            if (_dragging)
            {
                CancelDrag();
            }
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
                var dirtyRects = new ConcurrentBag<Int32Rect>();
                _vm.TileBitmap.Lock();

                var ptr = _vm.TileBitmap.BackBuffer;
                var stride = _vm.TileBitmap.BackBufferStride;

                Parallel.ForEach(_previousSelection, tile =>
                {
                    var rect = Select(tile, ptr, stride);
                    dirtyRects.Add(rect);
                });
                
                foreach (var rect in dirtyRects)
                {
                    _vm.TileBitmap.AddDirtyRect(rect);
                }
                _vm.TileBitmap.Unlock();
            }

            RaiseEvent(new AreaSelectionChangedEvent(AreaSelectionChangedEvent, GetSelectedTiles().Count != 0));
        }

        private void ResetSelection()
        {
            var dirtyRects = new ConcurrentBag<Int32Rect>();
            _vm.TileBitmap.Lock();
            var ptr = _vm.TileBitmap.BackBuffer;
            var stride = _vm.TileBitmap.BackBufferStride;

            Parallel.For(0, _tiles.GetLength(0), i =>
            {
                for (int j = 0; j < _tiles.GetLength(1); j++)
                {
                    var rect = DeSelect(_tiles[i,j], ptr, stride);
                    dirtyRects.Add(rect);
                }
            });

            foreach (var rect in dirtyRects)
            {
                _vm.TileBitmap.AddDirtyRect(rect);
            }
            _vm.TileBitmap.Unlock();

            _selectedTiles = null;
        }

        private void SelectTiles(Point start, Point end)
        {
            Debug.Assert(start.X + Epsilon > 0.0 && start.X - Epsilon < MapGrid.Width, "Start point X-value outside of expected range");
            Debug.Assert(start.Y + Epsilon > 0.0 && start.Y - Epsilon < MapGrid.Height, "Start point Y-value outside of expected range");
            Debug.Assert(end.X + Epsilon > 0.0 && end.X - Epsilon < MapGrid.Width, "End point X-value outside of expected range");
            Debug.Assert(end.Y + Epsilon > 0.0 && end.Y - Epsilon < MapGrid.Height, "End point Y-value outside of expected range");
            
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
            
            var dirtyRects = new ConcurrentBag<Int32Rect>();
            _vm.TileBitmap.Lock();
            var ptr = _vm.TileBitmap.BackBuffer;
            var stride = _vm.TileBitmap.BackBufferStride;

            //Rounding might cause us to get indexes with the same value, which we still want to mark as a selection.
            //This rounding will cause the index to be larger than required, so subtract 1 when using the index directly.
            //minX and minY should always be bigger than 0 due to this rounding, but the nested if-else statement is included just to be on the safe side.
            if (minX == maxX && minY != maxY)
            {
                if (minX > 0)
                {
                    Parallel.For(minY, maxY, j =>
                    {
                        var rect = Select(_tiles[minX - 1, j], ptr, stride);
                        dirtyRects.Add(rect);
                    });
                }
                else
                {
                    Parallel.For(minY, maxY, j =>
                    {
                        var rect = Select(_tiles[0, j], ptr, stride);
                        dirtyRects.Add(rect);
                    });
                }
            }
            else if (minX != maxX && minY == maxY)
            {
                if (minY > 0)
                {
                    Parallel.For(minX, maxX, i =>
                    {
                        var rect = Select(_tiles[i, minY - 1], ptr, stride);
                        dirtyRects.Add(rect);
                    });
                }
                else
                {
                    Parallel.For(minX, maxX, i =>
                    {
                        var rect = Select(_tiles[i, 0], ptr, stride);
                        dirtyRects.Add(rect);
                    });
                }
            }
            else if (minX == maxX && minY == maxY)
            {
                if (minX > 0 && minY > 0)
                {
                    dirtyRects.Add(Select(_tiles[minX - 1, minY - 1], ptr, stride));
                }
                else
                {
                    dirtyRects.Add(Select(_tiles[0, 0], ptr, stride));
                }
            }
            else
            {
                //Proper drag that spans multiple tiles in both the x- and y-axis
                Parallel.For(minX, maxX, i =>
                {
                    for (int j = minY; j < maxY; j++)
                    {
                        var rect = Select(_tiles[i, j], ptr, stride);
                        dirtyRects.Add(rect);
                    }
                });
            }

            if ((Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl)) && _previousSelection != null)
            {
                Parallel.ForEach(_previousSelection, tile =>
                {
                    var rect = Select(tile, ptr, stride);
                    dirtyRects.Add(rect);
                });
            }
            
            foreach (var rect in dirtyRects)
            {
                _vm.TileBitmap.AddDirtyRect(rect);
            }

            _vm.TileBitmap.Unlock();
        }

        public List<TileStatus> GetSelectedTiles()
        {
            if (_selectedTiles == null)
            {
                _selectedTiles = new List<TileStatus>();
                foreach (var tile in _tiles)
                {
                    if (tile.Selected)
                    {
                        _selectedTiles.Add(tile);
                    }

                }
            }

            return _selectedTiles;
        }

        public UITile[,] GetMapTiles()
        {
            var map = new UITile[_tiles.GetLength(0),_tiles.GetLength(1)];
            foreach (var tileStatus in _tiles)
            {
                map[tileStatus.X, tileStatus.Y] = tileStatus.Tile;
            }

            return map;
        }

        private static Int32Rect Select(TileStatus tile, IntPtr backBufferPtr, int backBufferStride)
        {
            tile.Selected = true;
            return BitmapHelper.SelectTile(backBufferPtr, backBufferStride, tile.X, tile.Y, TileHelper.UI_MAP_TILE_PIXELSIZE);
        }

        private static Int32Rect DeSelect(TileStatus tile, IntPtr backBufferPtr, int backBufferStride)
        {
            tile.Selected = false;
            return BitmapHelper.OverwriteTile(backBufferPtr, backBufferStride, tile.Tile, tile.X, tile.Y, TileHelper.UI_MAP_TILE_PIXELSIZE);
        }

        /// <summary>
        /// Changes the given tiles to the specified tile and updates the UI bitmap to show the updated tiles.
        /// </summary>
        public void SetTile(IEnumerable<TileStatus> tiles, UITile newTile)
        {
            var dirtyRects = new ConcurrentBag<Int32Rect>();
            _vm.TileBitmap.Lock();
            var ptr = _vm.TileBitmap.BackBuffer;
            var stride = _vm.TileBitmap.BackBufferStride;

            Parallel.ForEach(tiles, tile =>
            {
                tile.Tile = newTile;
                var rect = BitmapHelper.OverwriteTile(ptr, stride, tile.Tile, tile.X, tile.Y, TileHelper.UI_MAP_TILE_PIXELSIZE);
                dirtyRects.Add(rect);

                if (tile.Selected)
                {
                    Select(tile, ptr, stride);
                }
            });

            foreach (var rect in dirtyRects)
            {
                _vm.TileBitmap.AddDirtyRect(rect);
            }

            _vm.TileBitmap.Unlock();
        }

        #region Events
        public static readonly RoutedEvent AreaSelectionChangedEvent =
            EventManager.RegisterRoutedEvent(nameof(AreaSelectionChangedEvent), RoutingStrategy.Bubble,
                typeof(RoutedEventHandler), typeof(MapControl));

        public event RoutedEventHandler AreaSelectionChanged
        {
            add => AddHandler(AreaSelectionChangedEvent, value);
            remove => RemoveHandler(AreaSelectionChangedEvent, value);
        }
        #endregion

        public class TileStatus
        {
            public UITile Tile { get; set; }
            public bool Selected { get; set; }
            public readonly int X;
            public readonly int Y;

            public TileStatus(int x, int y)
            {
                X = x;
                Y = y;
            }
        }
    }
}
