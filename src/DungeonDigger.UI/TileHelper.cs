using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows;
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

        public static BitmapSource TileEmpty { get; }
        public static BitmapSource TileWall { get; }
        public static BitmapSource TileUnknown { get; }
        public static BitmapSource Selection { get; }

        public static Tuple<byte[], int, int> TileEmptyPixels { get; }
        public static Tuple<byte[], int, int> TileWallPixels { get; }
        public static Tuple<byte[], int, int> TileUnknownPixels { get; }
        public static Tuple<byte[], int, int> SelectionPixels { get; }

        public static Tuple<Colour[], int, int> TileEmptyColours { get; }
        public static Tuple<Colour[], int, int> TileWallColours { get; }
        public static Tuple<Colour[], int, int> TileUnknownColours { get; }
        public static Tuple<Colour[], int, int> SelectionColours { get; }

        static TileHelper()
        {
            TileEmpty = BitmapHelper.ConvertBitmapToImageSource(Properties.Resources.TileEmpty);
            TileWall    = BitmapHelper.ConvertBitmapToImageSource(Properties.Resources.TileWall);
            TileUnknown = BitmapHelper.ConvertBitmapToImageSource(Properties.Resources.TileUnknown);
            Selection  = BitmapHelper.ConvertBitmapToImageSource(Properties.Resources.Selection);
            if(!(TileEmpty.PixelWidth   == UI_MAP_TILE_PIXELSIZE && TileEmpty.PixelHeight   == UI_MAP_TILE_PIXELSIZE)) TileEmpty   = new TransformedBitmap(TileEmpty,   new ScaleTransform(UI_MAP_TILE_PIXELSIZE / (double) TileEmpty.PixelWidth,   UI_MAP_TILE_PIXELSIZE / (double)TileEmpty.PixelHeight));
            if(!(TileWall.PixelWidth    == UI_MAP_TILE_PIXELSIZE && TileWall.PixelHeight    == UI_MAP_TILE_PIXELSIZE)) TileWall    = new TransformedBitmap(TileWall,    new ScaleTransform(UI_MAP_TILE_PIXELSIZE / (double) TileWall.PixelWidth,    UI_MAP_TILE_PIXELSIZE / (double)TileWall.PixelHeight));
            if(!(TileUnknown.PixelWidth == UI_MAP_TILE_PIXELSIZE && TileUnknown.PixelHeight == UI_MAP_TILE_PIXELSIZE)) TileUnknown = new TransformedBitmap(TileUnknown, new ScaleTransform(UI_MAP_TILE_PIXELSIZE / (double) TileUnknown.PixelWidth, UI_MAP_TILE_PIXELSIZE / (double)TileUnknown.PixelHeight));
            if(!(Selection.PixelWidth   == UI_MAP_TILE_PIXELSIZE && Selection.PixelHeight   == UI_MAP_TILE_PIXELSIZE)) Selection   = new TransformedBitmap(Selection,   new ScaleTransform(UI_MAP_TILE_PIXELSIZE / (double) Selection.PixelWidth,   UI_MAP_TILE_PIXELSIZE / (double)Selection.PixelHeight));

            Debug.Assert(TileEmpty.Format.BitsPerPixel == 32);
            Debug.Assert(TileWall.Format.BitsPerPixel == 32);
            Debug.Assert(TileUnknown.Format.BitsPerPixel == 32);
            Debug.Assert(Selection.Format.BitsPerPixel == 32);
            Debug.Assert(TileEmpty.PixelWidth    == UI_MAP_TILE_PIXELSIZE);
            Debug.Assert(TileEmpty.PixelHeight   == UI_MAP_TILE_PIXELSIZE);
            Debug.Assert(TileWall.PixelWidth     == UI_MAP_TILE_PIXELSIZE);
            Debug.Assert(TileWall.PixelHeight    == UI_MAP_TILE_PIXELSIZE);
            Debug.Assert(TileUnknown.PixelWidth  == UI_MAP_TILE_PIXELSIZE);
            Debug.Assert(TileUnknown.PixelHeight == UI_MAP_TILE_PIXELSIZE);
            Debug.Assert(Selection.PixelWidth    == UI_MAP_TILE_PIXELSIZE);
            Debug.Assert(Selection.PixelHeight   == UI_MAP_TILE_PIXELSIZE);
            TileEmpty.Freeze();
            TileWall.Freeze();
            TileUnknown.Freeze();
            Selection.Freeze();

            //Cache the tiles' pixels at the size used in the UI.
            TileEmptyPixels =    new Tuple<byte[], int, int>(CopyPixels(TileEmpty),   TileEmpty.PixelWidth,   TileEmpty.PixelHeight);
            TileWallPixels =     new Tuple<byte[], int, int>(CopyPixels(TileWall),    TileWall.PixelWidth,    TileWall.PixelHeight);
            TileUnknownPixels =  new Tuple<byte[], int, int>(CopyPixels(TileUnknown), TileUnknown.PixelWidth, TileUnknown.PixelHeight);
            SelectionPixels =    new Tuple<byte[], int, int>(CopyPixels(Selection),   Selection.PixelWidth,   Selection.PixelHeight);
            TileEmptyColours =   new Tuple<Colour[], int, int>(ConvertToColour(TileEmptyPixels.Item1),   TileEmpty.PixelWidth,   TileEmpty.PixelHeight);
            TileWallColours =    new Tuple<Colour[], int, int>(ConvertToColour(TileWallPixels.Item1),    TileWall.PixelWidth,    TileWall.PixelHeight);
            TileUnknownColours = new Tuple<Colour[], int, int>(ConvertToColour(TileUnknownPixels.Item1), TileUnknown.PixelWidth, TileUnknown.PixelHeight);
            SelectionColours =   new Tuple<Colour[], int, int>(ConvertToColour(SelectionPixels.Item1),   Selection.PixelWidth,   Selection.PixelHeight);
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
        public static Tuple<Colour[], int, int> GetTileColours(Tile tile, int tileSize)
        {
            //If we need to scale to the desired tile size, we have to scale the tile and grab the colours.
            if (tileSize != UI_MAP_TILE_PIXELSIZE)
            {
                BitmapSource tileImage = GetTileImage(tile);
                tileImage = new TransformedBitmap(tileImage, new ScaleTransform(tileSize / (double)tileImage.PixelWidth, tileSize / (double)tileImage.PixelHeight));
                return new Tuple<Colour[], int, int>(ConvertToColour(CopyPixels(tileImage)), tileImage.PixelWidth, tileImage.PixelHeight);
            }

            //Otherwise use the cached, precomputed values.
            switch (tile)
            {
                case Tile.Room:
                case Tile.Hallway:
                    return TileEmptyColours;
                case Tile.Wall:
                case Tile.Unassigned:
                    return TileWallColours;
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
        public static Tuple<byte[],int,int> GetTilePixels(Tile tile, int tileSize)
        {
            //If we need to scale to the desired tile size, we have to scale the tile and grab the pixels.
            if (tileSize != UI_MAP_TILE_PIXELSIZE)
            {
                BitmapSource tileImage = GetTileImage(tile);
                tileImage = new TransformedBitmap(tileImage, new ScaleTransform(tileSize / (double)tileImage.PixelWidth, tileSize / (double)tileImage.PixelHeight));
                return new Tuple<byte[], int, int>(CopyPixels(tileImage), tileImage.PixelWidth, tileImage.PixelHeight);
            }
            
            //Otherwise use the cached, precomputed values.
            switch (tile)
            {
                case Tile.Room:
                case Tile.Hallway:
                    return TileEmptyPixels;
                case Tile.Wall:
                case Tile.Unassigned:
                    return TileWallPixels;
                default:
                    return TileUnknownPixels;
            }
        }

        public static BitmapSource GetTileImage(Tile tile)
        {
            switch (tile)
            {
                case Tile.Room:
                case Tile.Hallway:
                    return TileEmpty;
                case Tile.Wall:
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
