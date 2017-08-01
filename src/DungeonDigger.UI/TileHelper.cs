using System;
using System.Diagnostics;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using DungeonDigger.Generation;

namespace DungeonDigger.UI
{
    public static class TileHelper
    {
        /// <summary>
        /// The size of each tile in the map shown in the main UI.
        /// </summary>
        public const int UI_MAP_TILE_PIXELSIZE = 16;

        private static readonly BitmapSource TileFloor;
        private static readonly BitmapSource TileWall;
        private static readonly BitmapSource TileDoorClosed_NorthSouth;
        private static readonly BitmapSource TileDoorClosed_EastWest;
        private static readonly BitmapSource TileDoorOpen_NorthSouth;
        private static readonly BitmapSource TileDoorOpen_EastWest;
        private static readonly BitmapSource TileDoorSecret;
        private static readonly BitmapSource TileStairSingle_North;
        private static readonly BitmapSource TileStairSingle_South; 
        private static readonly BitmapSource TileStairSingle_East;
        private static readonly BitmapSource TileStairSingle_West;
        private static readonly BitmapSource TileStairPartOne_North;
        private static readonly BitmapSource TileStairPartOne_South;
        private static readonly BitmapSource TileStairPartOne_East;
        private static readonly BitmapSource TileStairPartOne_West;
        private static readonly BitmapSource TileStairPartTwo_North;
        private static readonly BitmapSource TileStairPartTwo_South;
        private static readonly BitmapSource TileStairPartTwo_East;
        private static readonly BitmapSource TileStairPartTwo_West;
        private static readonly BitmapSource TileUnknown;

        private static readonly ImageData<byte[]> TileFloorPixels;
        private static readonly ImageData<byte[]> TileWallPixels;
        private static readonly ImageData<byte[]> TileDoorClosed_NorthSouthPixels;
        private static readonly ImageData<byte[]> TileDoorClosed_EastWestPixels;
        private static readonly ImageData<byte[]> TileDoorOpen_NorthSouthPixels;
        private static readonly ImageData<byte[]> TileDoorOpen_EastWestPixels;
        private static readonly ImageData<byte[]> TileDoorSecretPixels;
        private static readonly ImageData<byte[]> TileStairSingle_NorthPixels;
        private static readonly ImageData<byte[]> TileStairSingle_SouthPixels;
        private static readonly ImageData<byte[]> TileStairSingle_EastPixels;
        private static readonly ImageData<byte[]> TileStairSingle_WestPixels;
        private static readonly ImageData<byte[]> TileStairPartOne_NorthPixels;
        private static readonly ImageData<byte[]> TileStairPartOne_SouthPixels;
        private static readonly ImageData<byte[]> TileStairPartOne_EastPixels;
        private static readonly ImageData<byte[]> TileStairPartOne_WestPixels;
        private static readonly ImageData<byte[]> TileStairPartTwo_NorthPixels;
        private static readonly ImageData<byte[]> TileStairPartTwo_SouthPixels;
        private static readonly ImageData<byte[]> TileStairPartTwo_EastPixels;
        private static readonly ImageData<byte[]> TileStairPartTwo_WestPixels;
        private static readonly ImageData<byte[]> TileUnknownPixels;

        private static readonly ImageData<Colour[]> TileFloorColours;
        private static readonly ImageData<Colour[]> TileWallColours;
        private static readonly ImageData<Colour[]> TileDoorClosed_NorthSouthColours;
        private static readonly ImageData<Colour[]> TileDoorClosed_EastWestColours;
        private static readonly ImageData<Colour[]> TileDoorOpen_NorthSouthColours;
        private static readonly ImageData<Colour[]> TileDoorOpen_EastWestColours;
        private static readonly ImageData<Colour[]> TileDoorSecretColours;
        private static readonly ImageData<Colour[]> TileStairSingle_NorthColours;
        private static readonly ImageData<Colour[]> TileStairSingle_SouthColours;
        private static readonly ImageData<Colour[]> TileStairSingle_EastColours;
        private static readonly ImageData<Colour[]> TileStairSingle_WestColours;
        private static readonly ImageData<Colour[]> TileStairPartOne_NorthColours;
        private static readonly ImageData<Colour[]> TileStairPartOne_SouthColours;
        private static readonly ImageData<Colour[]> TileStairPartOne_EastColours;
        private static readonly ImageData<Colour[]> TileStairPartOne_WestColours;
        private static readonly ImageData<Colour[]> TileStairPartTwo_NorthColours;
        private static readonly ImageData<Colour[]> TileStairPartTwo_SouthColours;
        private static readonly ImageData<Colour[]> TileStairPartTwo_EastColours;
        private static readonly ImageData<Colour[]> TileStairPartTwo_WestColours;
        private static readonly ImageData<Colour[]> TileUnknownColours;

        public static readonly BitmapSource Selection;
        public static readonly ImageData<byte[]> SelectionPixels;
        public static readonly ImageData<Colour[]> SelectionColours;

        static TileHelper()
        {
            TileFloor                 = BitmapHelper.ConvertBitmapToImageSource(Properties.Resources.TileEmpty);
            TileWall                  = BitmapHelper.ConvertBitmapToImageSource(Properties.Resources.TileWall);
            TileDoorClosed_NorthSouth = BitmapHelper.ConvertBitmapToImageSource(Properties.Resources.TileDoor);
            TileDoorClosed_EastWest   = new TransformedBitmap(BitmapHelper.ConvertBitmapToImageSource(Properties.Resources.TileDoor), new RotateTransform(90));
            TileDoorOpen_NorthSouth   = BitmapHelper.ConvertBitmapToImageSource(Properties.Resources.TileDoorOpen);
            TileDoorOpen_EastWest     = new TransformedBitmap(BitmapHelper.ConvertBitmapToImageSource(Properties.Resources.TileDoorOpen), new RotateTransform(90));
            TileDoorSecret            = BitmapHelper.ConvertBitmapToImageSource(Properties.Resources.TileSecret);
            TileStairSingle_North     = BitmapHelper.ConvertBitmapToImageSource(Properties.Resources.TileStairSingle);
            TileStairSingle_South     = new TransformedBitmap(BitmapHelper.ConvertBitmapToImageSource(Properties.Resources.TileStairSingle), new RotateTransform(180));
            TileStairSingle_East      = new TransformedBitmap(BitmapHelper.ConvertBitmapToImageSource(Properties.Resources.TileStairSingle), new RotateTransform(90));
            TileStairSingle_West      = new TransformedBitmap(BitmapHelper.ConvertBitmapToImageSource(Properties.Resources.TileStairSingle), new RotateTransform(270));
            TileStairPartOne_North    = BitmapHelper.ConvertBitmapToImageSource(Properties.Resources.TileStairPart_One);
            TileStairPartOne_South    = new TransformedBitmap(BitmapHelper.ConvertBitmapToImageSource(Properties.Resources.TileStairPart_One), new RotateTransform(180));
            TileStairPartOne_East     = new TransformedBitmap(BitmapHelper.ConvertBitmapToImageSource(Properties.Resources.TileStairPart_One), new RotateTransform(90));
            TileStairPartOne_West     = new TransformedBitmap(BitmapHelper.ConvertBitmapToImageSource(Properties.Resources.TileStairPart_One), new RotateTransform(270));
            TileStairPartTwo_North    = BitmapHelper.ConvertBitmapToImageSource(Properties.Resources.TileStairPart_Two);
            TileStairPartTwo_South    = new TransformedBitmap(BitmapHelper.ConvertBitmapToImageSource(Properties.Resources.TileStairPart_Two), new RotateTransform(180));
            TileStairPartTwo_East     = new TransformedBitmap(BitmapHelper.ConvertBitmapToImageSource(Properties.Resources.TileStairPart_Two), new RotateTransform(90));
            TileStairPartTwo_West     = new TransformedBitmap(BitmapHelper.ConvertBitmapToImageSource(Properties.Resources.TileStairPart_Two), new RotateTransform(270));
            TileUnknown               = BitmapHelper.ConvertBitmapToImageSource(Properties.Resources.TileUnknown);
            Selection                 = BitmapHelper.ConvertBitmapToImageSource(Properties.Resources.Selection);

            CacheTile(ref TileFloor,                 ref TileFloorPixels,                 ref TileFloorColours);
            CacheTile(ref TileWall,                  ref TileWallPixels,                  ref TileWallColours);
            CacheTile(ref TileDoorClosed_NorthSouth, ref TileDoorClosed_NorthSouthPixels, ref TileDoorClosed_NorthSouthColours);
            CacheTile(ref TileDoorClosed_EastWest,   ref TileDoorClosed_EastWestPixels,   ref TileDoorClosed_EastWestColours);
            CacheTile(ref TileDoorOpen_NorthSouth,   ref TileDoorOpen_NorthSouthPixels,   ref TileDoorOpen_NorthSouthColours);
            CacheTile(ref TileDoorOpen_EastWest,     ref TileDoorOpen_EastWestPixels,     ref TileDoorOpen_EastWestColours);
            CacheTile(ref TileDoorSecret,            ref TileDoorSecretPixels,            ref TileDoorSecretColours);
            CacheTile(ref TileStairSingle_North,     ref TileStairSingle_NorthPixels,     ref TileStairSingle_NorthColours);
            CacheTile(ref TileStairSingle_South,     ref TileStairSingle_SouthPixels,     ref TileStairSingle_SouthColours);
            CacheTile(ref TileStairSingle_East,      ref TileStairSingle_EastPixels,      ref TileStairSingle_EastColours);
            CacheTile(ref TileStairSingle_West,      ref TileStairSingle_WestPixels,      ref TileStairSingle_WestColours);
            CacheTile(ref TileStairPartOne_North,    ref TileStairPartOne_NorthPixels,    ref TileStairPartOne_NorthColours);
            CacheTile(ref TileStairPartOne_South,    ref TileStairPartOne_SouthPixels,    ref TileStairPartOne_SouthColours);
            CacheTile(ref TileStairPartOne_East,     ref TileStairPartOne_EastPixels,     ref TileStairPartOne_EastColours);
            CacheTile(ref TileStairPartOne_West,     ref TileStairPartOne_WestPixels,     ref TileStairPartOne_WestColours);
            CacheTile(ref TileStairPartTwo_North,    ref TileStairPartTwo_NorthPixels,    ref TileStairPartTwo_NorthColours);
            CacheTile(ref TileStairPartTwo_South,    ref TileStairPartTwo_SouthPixels,    ref TileStairPartTwo_SouthColours);
            CacheTile(ref TileStairPartTwo_East,     ref TileStairPartTwo_EastPixels,     ref TileStairPartTwo_EastColours);
            CacheTile(ref TileStairPartTwo_West,     ref TileStairPartTwo_WestPixels,     ref TileStairPartTwo_WestColours);
            CacheTile(ref TileUnknown,               ref TileUnknownPixels,               ref TileUnknownColours);
            CacheTile(ref Selection,                 ref SelectionPixels,                 ref SelectionColours);
        }
        
        private static void CacheTile(ref BitmapSource image, ref ImageData<byte[]> pixels, ref ImageData<Colour[]> colours)
        {
            if (!(image.PixelWidth == UI_MAP_TILE_PIXELSIZE && image.PixelHeight == UI_MAP_TILE_PIXELSIZE))
            {
                image = new TransformedBitmap(image,
                    new ScaleTransform(UI_MAP_TILE_PIXELSIZE / (double) image.PixelWidth,
                        UI_MAP_TILE_PIXELSIZE / (double) image.PixelHeight));
            }

            Debug.Assert(image.Format.BitsPerPixel == 32);
            Debug.Assert(image.PixelWidth == UI_MAP_TILE_PIXELSIZE);
            Debug.Assert(image.PixelHeight == UI_MAP_TILE_PIXELSIZE);

            image.Freeze();
            pixels = new ImageData<byte[]>{Data = CopyPixels(image), Width = image.PixelWidth, Height = image.PixelHeight};
            colours = new ImageData<Colour[]>{Data = ConvertToColour(pixels.Data) , Width = image.PixelWidth, Height = image.PixelHeight};
        }

        /// <summary>
        /// Convert the byte-array into a Colour struct that can be used to directly write pixels to the backbuffer of a <see cref="WriteableBitmap"/>.
        /// </summary>
        public static Colour[] ConvertToColour(byte[] bgraBytes)
        {
            var colours = new Colour[bgraBytes.Length / 4];

            var pI = 0;
            for (int i = 0; i+3 < bgraBytes.Length; i = i + 4, pI++)
            {
                colours[pI] = new Colour
                {
                    Blue = bgraBytes[i],
                    Green = bgraBytes[i + 1],
                    Red = bgraBytes[i + 2],
                    Alpha = bgraBytes[i + 3]
                };
            }

            return colours;
        }

        /// <summary>
        /// Copy all the pixels from the bitmap into a byte-array
        /// Given bitmap must use a format with 32 bits per pixel.
        /// </summary>
        public static byte[] CopyPixels(BitmapSource bitmap)
        {
            Debug.Assert(bitmap.Format.BitsPerPixel == 32);
            var stride = (bitmap.PixelWidth * bitmap.Format.BitsPerPixel + 7) / 8;
            var pixels = new byte[bitmap.PixelHeight * stride];
            bitmap.CopyPixels(pixels, stride, 0);
            return pixels;
        }

        /// <summary>
        /// Get the PixelColour array that makes up the given tile, at the given tilesize.
        /// The returned PixelColour-array may NOT be modified due to cached return values.
        /// </summary>
        /// <param name="tile">The tile to get pixels for</param>
        /// <param name="tileSize">The desired width and height of the tile</param>
        /// <returns>
        /// A tuple with the following values.
        /// Item 1: A PixelColour-array containing the pixels of the image representing the tile. Do not modify.
        /// Item 2: The pixelwidth of the image.
        /// Item 3: The pixelheight of the image.
        /// </returns>
        public static ImageData<Colour[]> GetTileColours(UITile tile, int tileSize)
        {
            //If we need to scale to the desired tile size, we have to scale the tile and grab the colours.
            if (tileSize != UI_MAP_TILE_PIXELSIZE)
            {
                BitmapSource tileImage = GetTileImage(tile);
                tileImage = new TransformedBitmap(tileImage, new ScaleTransform(tileSize / (double)tileImage.PixelWidth, tileSize / (double)tileImage.PixelHeight));
                return new ImageData<Colour[]>{Data = ConvertToColour(CopyPixels(tileImage)), Width = tileImage.PixelWidth, Height = tileImage.PixelHeight};
            }

            //Otherwise use the cached, precomputed values.
            switch (tile)
            {
                case UITile.Wall:
                    return TileWallColours;
                case UITile.Floor:
                    return TileFloorColours;
                case UITile.StairSingle_North:
                    return TileStairSingle_NorthColours;
                case UITile.StairSingle_South:
                    return TileStairSingle_SouthColours;
                case UITile.StairSingle_East:
                    return TileStairSingle_EastColours;
                case UITile.StairSingle_West:
                    return TileStairSingle_WestColours;
                case UITile.StairPartOne_North:
                    return TileStairPartOne_NorthColours;
                case UITile.StairPartOne_South:
                    return TileStairPartOne_SouthColours;
                case UITile.StairPartOne_East:
                    return TileStairPartOne_EastColours;
                case UITile.StairPartOne_West:
                    return TileStairPartOne_WestColours;
                case UITile.StairPartTwo_North:
                    return TileStairPartTwo_NorthColours;
                case UITile.StairPartTwo_South:
                    return TileStairPartTwo_SouthColours;
                case UITile.StairPartTwo_East:
                    return TileStairPartTwo_EastColours;
                case UITile.StairPartTwo_West:
                    return TileStairPartTwo_WestColours;
                case UITile.DoorClosed_NorthSouth:
                    return TileDoorClosed_NorthSouthColours;
                case UITile.DoorClosed_EastWest:
                    return TileDoorClosed_EastWestColours;
                case UITile.DoorSecret:
                    return TileDoorSecretColours;
                case UITile.Unknown:
                    return TileUnknownColours;
                case UITile.DoorOpen_NorthSouth:
                    return TileDoorOpen_NorthSouthColours;
                case UITile.DoorOpen_EastWest:
                    return TileDoorOpen_EastWestColours;
                default:
                    return TileUnknownColours;
            }
        }

        /// <summary>
        /// Get the byte array that makes up the given tile, at the given tilesize.
        /// The returned byte-array may NOT be modified due to cached return values.
        /// 
        /// If you want a modifiable byte-array use <see cref="GetTileImage"/>, scale the image and then call <see cref="CopyPixels"/>
        /// </summary>
        /// <param name="tile">The tile to get pixels for</param>
        /// <param name="tileSize">The desired width and height of the tile</param>
        /// <returns>
        /// A tuple with the following values.
        /// Item 1: A byte-array containing the pixels of the image representing the tile. Do not modify.
        /// Item 2: The pixelwidth of the image.
        /// Item 3: The pixelheight of the image.
        /// </returns>
        public static ImageData<byte[]> GetTilePixels(UITile tile, int tileSize)
        {
            //If we need to scale to the desired tile size, we have to scale the tile and grab the pixels.
            if (tileSize != UI_MAP_TILE_PIXELSIZE)
            {
                BitmapSource tileImage = GetTileImage(tile);
                tileImage = new TransformedBitmap(tileImage, new ScaleTransform(tileSize / (double)tileImage.PixelWidth, tileSize / (double)tileImage.PixelHeight));
                return new ImageData<byte[]>{Data = CopyPixels(tileImage), Width = tileImage.PixelWidth, Height = tileImage.PixelHeight};
            }
            
            //Otherwise use the cached, precomputed values.
            switch (tile)
            {
                case UITile.Wall:
                    return TileWallPixels;
                case UITile.Floor:
                    return TileFloorPixels;
                case UITile.StairSingle_North:
                    return TileStairSingle_NorthPixels;
                case UITile.StairSingle_South:
                    return TileStairSingle_SouthPixels;
                case UITile.StairSingle_East:
                    return TileStairSingle_EastPixels;
                case UITile.StairSingle_West:
                    return TileStairSingle_WestPixels;
                case UITile.StairPartOne_North:
                    return TileStairPartOne_NorthPixels;
                case UITile.StairPartOne_South:
                    return TileStairPartOne_SouthPixels;
                case UITile.StairPartOne_East:
                    return TileStairPartOne_EastPixels;
                case UITile.StairPartOne_West:
                    return TileStairPartOne_WestPixels;
                case UITile.StairPartTwo_North:
                    return TileStairPartTwo_NorthPixels;
                case UITile.StairPartTwo_South:
                    return TileStairPartTwo_SouthPixels;
                case UITile.StairPartTwo_East:
                    return TileStairPartTwo_EastPixels;
                case UITile.StairPartTwo_West:
                    return TileStairPartTwo_WestPixels;
                case UITile.DoorClosed_NorthSouth:
                    return TileDoorClosed_NorthSouthPixels;
                case UITile.DoorClosed_EastWest:
                    return TileDoorClosed_EastWestPixels;
                case UITile.DoorSecret:
                    return TileDoorSecretPixels;
                case UITile.Unknown:
                    return TileUnknownPixels;
                case UITile.DoorOpen_NorthSouth:
                    return TileDoorOpen_NorthSouthPixels;
                case UITile.DoorOpen_EastWest:
                    return TileDoorOpen_EastWestPixels;
                default:
                    return TileUnknownPixels;
            }
        }

        public static BitmapSource GetTileImage(UITile tile)
        {
            switch (tile)
            {
                case UITile.Unknown:
                    return TileUnknown;
                case UITile.Wall:
                    return TileWall;
                case UITile.Floor:
                    return TileFloor;
                case UITile.StairSingle_North:
                    return TileStairSingle_North;
                case UITile.StairSingle_South:
                    return TileStairSingle_South;
                case UITile.StairSingle_East:
                    return TileStairSingle_East;
                case UITile.StairSingle_West:
                    return TileStairSingle_West;
                case UITile.StairPartOne_North:
                    return TileStairPartOne_North;
                case UITile.StairPartOne_South:
                    return TileStairPartOne_South;
                case UITile.StairPartOne_East:
                    return TileStairPartOne_East;
                case UITile.StairPartOne_West:
                    return TileStairPartOne_West;
                case UITile.StairPartTwo_North:
                    return TileStairPartTwo_North;
                case UITile.StairPartTwo_South:
                    return TileStairPartTwo_South;
                case UITile.StairPartTwo_East:
                    return TileStairPartTwo_East;
                case UITile.StairPartTwo_West:
                    return TileStairPartTwo_East;
                case UITile.DoorClosed_NorthSouth:
                    return TileDoorClosed_NorthSouth;
                case UITile.DoorClosed_EastWest:
                    return TileDoorClosed_EastWest;
                case UITile.DoorSecret:
                    return TileDoorSecret;
                case UITile.DoorOpen_NorthSouth:
                    return TileDoorOpen_NorthSouth;
                case UITile.DoorOpen_EastWest:
                    return TileDoorOpen_EastWest;
                default:
                    Debug.WriteLine($"Tile doesn't have a specified image. Enum value was {tile}");
                    return TileUnknown;
            }
        }

        public static string GetTileName(UITile tile)
        {
            switch (tile)
            {
                case UITile.Floor:
                    return "Floor";
                case UITile.Wall:
                    return "Wall";
                case UITile.Unknown:
                    return "Unknown Tile";
                case UITile.StairSingle_North:
                case UITile.StairSingle_South:
                case UITile.StairSingle_East:
                case UITile.StairSingle_West:
                    return "Stair";
                case UITile.StairPartOne_North:
                case UITile.StairPartOne_South:
                case UITile.StairPartOne_East:
                case UITile.StairPartOne_West:
                    return "Stair, Part 1";
                case UITile.StairPartTwo_North:
                case UITile.StairPartTwo_South:
                case UITile.StairPartTwo_East:
                case UITile.StairPartTwo_West:
                    return "Stair, Part 2";
                case UITile.DoorClosed_NorthSouth:
                case UITile.DoorClosed_EastWest:
                    return "Door, Closed";
                case UITile.DoorOpen_NorthSouth:
                case UITile.DoorOpen_EastWest:
                    return "Door, Open";
                case UITile.DoorSecret:
                    return "Door, Secret";
                default:
                    Debug.WriteLine($"Tile doesn't have a specified name. Enum value was {tile}");
                    return "Missing name";
            }
        }

        /// <summary>
        /// Converts from a map of Tile-enums to a map of UITile-enums, ensuring that the orientation of each tile is correct based on adjacent tiles.
        /// </summary>
        /// <param name="map">The map to convert</param>
        /// <returns>A map of UITiles with correct orientation of all orientation-sensitive tiles</returns>
        public static UITile[,] Convert(Tile[,] map)
        {
            var UIMap = new UITile[map.GetLength(0),map.GetLength(1)];
            for (int x = 0; x < map.GetLength(0); x++)
            {
                for (int y = 0; y < map.GetLength(1); y++)
                {
                    switch (map[x,y])
                    {
                        case Tile.Unassigned:
                        case Tile.Wall:
                            UIMap[x,y] = UITile.Wall;
                            break;
                        case Tile.Room:
                        case Tile.Hallway:
                            UIMap[x,y] = UITile.Floor;
                            break;
                        case Tile.StairUp:
                            if      (IsInRange(x - 1, y) && IsFloorTile(map[x - 1, y])) UIMap[x, y] = UITile.StairSingle_East;
                            else if (IsInRange(x + 1, y) && IsFloorTile(map[x + 1, y])) UIMap[x, y] = UITile.StairSingle_West;
                            else if (IsInRange(x, y + 1) && IsFloorTile(map[x, y + 1])) UIMap[x, y] = UITile.StairSingle_North;
                            else if (IsInRange(x, y - 1) && IsFloorTile(map[x, y - 1])) UIMap[x, y] = UITile.StairSingle_South;
                            else UIMap[x,y] = UITile.StairSingle_North;
                            break;
                        case Tile.StairDown:
                            if      (IsInRange(x - 1, y) && IsFloorTile(map[x - 1, y])) UIMap[x, y] = UITile.StairSingle_West;
                            else if (IsInRange(x + 1, y) && IsFloorTile(map[x + 1, y])) UIMap[x, y] = UITile.StairSingle_East;
                            else if (IsInRange(x, y + 1) && IsFloorTile(map[x, y + 1])) UIMap[x, y] = UITile.StairSingle_South;
                            else if (IsInRange(x, y - 1) && IsFloorTile(map[x, y - 1])) UIMap[x, y] = UITile.StairSingle_North;
                            else UIMap[x, y] = UITile.StairSingle_North;
                            break;
                        case Tile.StairPartOne:
                            if      (IsInRange(x - 1, y) && map[x - 1, y] == Tile.StairPartTwo) UIMap[x, y] = UITile.StairPartOne_East;
                            else if (IsInRange(x + 1, y) && map[x + 1, y] == Tile.StairPartTwo) UIMap[x, y] = UITile.StairPartOne_West;
                            else if (IsInRange(x, y + 1) && map[x, y + 1] == Tile.StairPartTwo) UIMap[x, y] = UITile.StairPartOne_North;
                            else if (IsInRange(x, y - 1) && map[x, y - 1] == Tile.StairPartTwo) UIMap[x, y] = UITile.StairPartOne_South;
                            else UIMap[x, y] = UITile.StairPartOne_North;
                            break;
                        case Tile.StairPartTwo:
                            if      (IsInRange(x - 1, y) && map[x - 1, y] == Tile.StairPartOne) UIMap[x, y] = UITile.StairPartTwo_West;
                            else if (IsInRange(x + 1, y) && map[x + 1, y] == Tile.StairPartOne) UIMap[x, y] = UITile.StairPartTwo_East;
                            else if (IsInRange(x, y + 1) && map[x, y + 1] == Tile.StairPartOne) UIMap[x, y] = UITile.StairPartTwo_South;
                            else if (IsInRange(x, y - 1) && map[x, y - 1] == Tile.StairPartOne) UIMap[x, y] = UITile.StairPartTwo_North;
                            else UIMap[x, y] = UITile.StairPartTwo_North;
                            break;
                        case Tile.DoorClosed:
                            if      ((IsInRange(x - 1, y) && IsFloorTile(map[x - 1, y])) || (IsInRange(x + 1, y) && IsFloorTile(map[x + 1, y]))) UIMap[x, y] = UITile.DoorClosed_EastWest;
                            else if ((IsInRange(x, y + 1) && IsFloorTile(map[x, y + 1])) || (IsInRange(x, y - 1) && IsFloorTile(map[x, y - 1]))) UIMap[x, y] = UITile.DoorClosed_NorthSouth;
                            else UIMap[x, y] = UITile.DoorClosed_NorthSouth;
                            break;
                        case Tile.DoorOpen:
                            if      ((IsInRange(x - 1, y) && IsFloorTile(map[x - 1, y])) || (IsInRange(x + 1, y) && IsFloorTile(map[x + 1, y]))) UIMap[x, y] = UITile.DoorOpen_EastWest;
                            else if ((IsInRange(x, y + 1) && IsFloorTile(map[x, y + 1])) || (IsInRange(x, y - 1) && IsFloorTile(map[x, y - 1]))) UIMap[x, y] = UITile.DoorOpen_NorthSouth;
                            else UIMap[x, y] = UITile.DoorOpen_NorthSouth;
                            break;
                        case Tile.DoorSecret:
                            UIMap[x, y] = UITile.DoorSecret;
                            break;
                        case Tile.Unknown:
                            UIMap[x, y] = UITile.Unknown;
                            break;
                        default:
                            Debug.WriteLine($"Tile-enum missing from Tile to UITile converter. Tile was: {map[x, y]}");
                            UIMap[x, y] = UITile.Unknown;
                            break;
                    }
                }
            }

            bool IsInRange(int x, int y)
            {
                return x >= 0 && y >= 0 && x < map.GetLength(0) && y < map.GetLength(1);
            }

            bool IsFloorTile(Tile tile)
            {
                return tile == Tile.Room || tile == Tile.Hallway;
            }
            
            return UIMap;
        }

        /// <summary>
        /// Mutable tuple of image with named values.
        /// </summary>
        public class ImageData<T>
        {
            public int Width { get; set; }
            public int Height { get; set; }
            public T Data { get; set; }
        }

        /// <summary>
        /// Struct for holding BGRA pixel colours, to facilitate writing directly to the <see cref="WriteableBitmap"/> backbuffer.
        /// </summary>
        [StructLayout(LayoutKind.Explicit)]
        public struct Colour
        {
            // 8 bit components
            [FieldOffset(0)]
            public byte Blue;

            [FieldOffset(1)]
            public byte Green;

            [FieldOffset(2)]
            public byte Red;

            [FieldOffset(3)]
            public byte Alpha;
        }
    }
}
