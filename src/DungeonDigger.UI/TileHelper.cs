﻿using System.Diagnostics;
using System.Windows.Media;
using DungeonDigger.Generation;

namespace DungeonDigger.UI
{
    public static class TileHelper
    {
        public static ImageSource TileEmpty { get; }
        public static ImageSource TileWall { get; }
        public static ImageSource TileUnknown { get; }

        static TileHelper()
        {
            TileEmpty = BitmapHelper.ConvertBitmapToImageSource(Properties.Resources.TileEmpty);
            TileWall = BitmapHelper.ConvertBitmapToImageSource(Properties.Resources.TileWall);
            TileUnknown = BitmapHelper.ConvertBitmapToImageSource(Properties.Resources.TileUnknown);
        }

        public static ImageSource GetTileImage(Tile tile)
        {
            switch (tile)
            {
                case Tile.Room:
                    return TileEmpty;
                case Tile.Hallway:
                    return TileEmpty;
                case Tile.Wall:
                    return TileWall;
                case Tile.Unassigned:
                    return TileWall;
                default:
                    Debug.WriteLine($"Tile doesn't have a specified image. Enum value was {tile}");
                    return TileUnknown;
            }
        }

        public static string GetTileName(Tile tile)
        {
            switch (tile)
            {
                case Tile.Room:
                    return "Room";
                case Tile.Hallway:
                    return "Hallway";
                case Tile.Wall:
                    return "Wall";
                case Tile.Unassigned:
                    return "Unassigned";
                case Tile.Stair:
                    return "Stair";
                case Tile.StairPartOne:
                    return "Stair, part 1";
                case Tile.StairPartTwo:
                    return "Stair, part 2";
                case Tile.DoorClosed:
                    return "Door, closed";
                case Tile.DoorOpen:
                    return "Door, open";
                case Tile.DoorSecret:
                    return "Door, secret";
                default:
                    Debug.WriteLine($"Tile doesn't have a specified name. Enum value was {tile}");
                    return "Missing name";
            }
        }
    }
}