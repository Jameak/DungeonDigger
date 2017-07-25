using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using DungeonDigger.Generation;

namespace DungeonDigger.UI
{
    public static class BitmapHelper
    {
        /// <summary>
        /// Returns a BitmapImage made from the given Bitmap that can be shown in a WPF Image UI element.
        /// </summary>
        public static BitmapImage ConvertBitmapToImageSource(Bitmap image)
        {
            using (var ms = new MemoryStream())
            {
                //To avoid "ExternalException" from System.Drawing with message "A Generic Error occured at GDI+" we need to create a new temporary bitmap here.
                //  Caused by the original stream that was used to create the original bitmap having been disposed.
                //  From MSDN Bitmap page: "You must keep the stream open for the lifetime of the Bitmap."
                var img = new Bitmap(image);

                img.Save(ms, ImageFormat.Png);
                ms.Seek(0, SeekOrigin.Begin);

                var output = new BitmapImage();
                output.BeginInit();
                output.CreateOptions = BitmapCreateOptions.PreservePixelFormat;
                output.CacheOption = BitmapCacheOption.OnLoad;
                output.StreamSource = ms;
                output.EndInit();
                img.Dispose();
                return output;
            }
        }

        public static void SaveBitmapSource(BitmapSource image, string path)
        {
            BitmapEncoder encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(image));

            using (var fs = new FileStream(path, FileMode.Create))
            {
                encoder.Save(fs);
            }
        }

        public static void SaveBitmapSource(BitmapSource image, string path, int internalTileSize, int outputTileSize)
        {
            var img = internalTileSize == outputTileSize
                ? image
                : new TransformedBitmap(image,
                    new ScaleTransform(outputTileSize / (double) internalTileSize,
                        outputTileSize / (double) internalTileSize));

            BitmapEncoder encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(img));

            using (var fs = new FileStream(path, FileMode.Create))
            {
                encoder.Save(fs);
            }
        }

        /// <summary>
        /// Create an image from the map where each tile has the specified size.
        /// </summary>
        /// <param name="map">The map for which to create an image</param>
        /// <param name="tileSize">The size of each tile</param>
        /// <returns></returns>
        public static BitmapSource CreateBitmap(Tile[,] map, int tileSize)
        {
            var width = map.GetLength(0) * tileSize;
            var height = map.GetLength(1) * tileSize;
            var pixelformat = PixelFormats.Pbgra32;
            var stride = (width * pixelformat.BitsPerPixel + 7) / 8;
            var pixels = new byte[stride * height];
            var imagedepth = (pixelformat.BitsPerPixel + 7) / 8;

            //Can only do this in parallel because the BitmapImage-instances returned by TileHelper.GetTileImage(...) are frozen.
            Parallel.For(0, map.GetLength(0), x =>
            {
                    for (int y = 0; y < map.GetLength(1); y++)
                    {
                        //Offset for the pixel in the full image that corresponds to pixel (0,0) in the tile.
                        var pixelOffset = (y * tileSize * width + x * tileSize) * imagedepth;

                        var tile = TileHelper.GetTileImage(map[x, y]);
                        //Scaled to our desired tile size.
                        var tileImage = new TransformedBitmap(tile, new ScaleTransform(tileSize / (double)tile.PixelWidth, tileSize / (double)tile.PixelHeight));
                        var tileStride = (tileImage.PixelWidth * pixelformat.BitsPerPixel + 7) / 8;
                        var tilePixels = new byte[tileImage.PixelHeight * tileStride];
                        tileImage.CopyPixels(tilePixels, tileStride, 0);

                        for (int tx = 0; tx < tileImage.PixelWidth; tx++)
                        {
                            for (int ty = 0; ty < tileImage.PixelHeight; ty++)
                            {
                                //Offset on the full image for the position of the pixel
                                var imageTileOffset = pixelOffset + (ty * width + tx) * imagedepth;
                                //Offset on the tile image for the position of the pixel
                                var tileOffset = (ty * tileImage.PixelWidth + tx) * imagedepth;

                                pixels[imageTileOffset + 0] = tilePixels[tileOffset + 0]; //B
                                pixels[imageTileOffset + 1] = tilePixels[tileOffset + 1]; //G
                                pixels[imageTileOffset + 2] = tilePixels[tileOffset + 2]; //R
                                pixels[imageTileOffset + 3] = tilePixels[tileOffset + 3]; //A
                            }
                        }
                    }
            });
            
            var image = BitmapSource.Create(width, height, 96, 96, PixelFormats.Pbgra32, null, pixels, stride);
            return image;
        }

        /// <summary>
        /// Overlay the image representing a selection on top of the specified tile, letting the tile poke through where the selection-image is transparent.
        /// Use <see cref="OverwriteTileSingleThread"/> to deselect a tile.
        /// </summary>
        /// <param name="image">The image to select a tile on</param>
        /// <param name="x">The x-coordinate of the tile. Zero-indexed</param>
        /// <param name="y">The y-coordinate of the tile. Zero-indexed</param>
        /// <param name="tileSize">The number of pixels that the side of one tile is. MUST be identical to the tilesize used to create the image.</param>
        public static void SelectTileSingleThread(WriteableBitmap image, int x, int y, int tileSize)
        {
            var imagedepth = (image.Format.BitsPerPixel + 7) / 8;
            var tileStride = (tileSize * image.Format.BitsPerPixel + 7) / 8;
            var tilePixels = new byte[tileSize * tileStride];
            var sourceRect = new Int32Rect(x * tileSize, y * tileSize, tileSize, tileSize);
            image.CopyPixels(sourceRect, tilePixels, tileStride, 0); //The 'offset' parameter is the offset into the tilePixels-array, NOT into the actual image.
            
            //Grab the image we're using to show that a tile has been selected
            var selectionImage = TileHelper.Selection;
            Debug.Assert(image.Format.BitsPerPixel == selectionImage.Format.BitsPerPixel, "Number of bits per pixel of selection image doesn't match.");
            //Scaled to our desired tile size.
            var scaledSelection = new TransformedBitmap(selectionImage, new ScaleTransform(tileSize / (double)selectionImage.PixelWidth, tileSize / (double)selectionImage.PixelHeight));
            var selectionStride = (scaledSelection.PixelWidth * scaledSelection.Format.BitsPerPixel + 7) / 8;
            var selectionPixels = new byte[scaledSelection.PixelHeight * selectionStride];
            scaledSelection.CopyPixels(selectionPixels, selectionStride, 0);
            
            Debug.Assert(tileStride == selectionStride, "Strides of tile in given image and selection image must be identical.");
            Debug.Assert(tilePixels.Length == selectionPixels.Length, "The size of the tile-array is different from the selection-array.");

            //Only use the non-transparent pixels from the selection, by leaving the tile-pixels intact where the selection is transparent.
            for (int i = 0; i < tileSize; i++)
            {
                for (int j = 0; j < tileSize; j++)
                {
                    var offset = (j * scaledSelection.PixelWidth + i) * imagedepth;

                    var selectAlpha = selectionPixels[offset + 3];
                    if (selectAlpha != 0) //The pixel is not transparent
                    {
                        tilePixels[offset + 0] = selectionPixels[offset + 0];
                        tilePixels[offset + 1] = selectionPixels[offset + 1];
                        tilePixels[offset + 2] = selectionPixels[offset + 2];
                        tilePixels[offset + 3] = selectionPixels[offset + 3];
                    }
                }
            }

            image.WritePixels(sourceRect, tilePixels, selectionStride, 0); //The 'offset' parameter is the offset into the tilePixels-array, NOT into the actual image.
        }

        /// <summary>
        /// Overlay the tile-image on the given image, at position x, y.
        /// </summary>
        /// <param name="image">The image to set a tile on</param>
        /// <param name="tile">The tile to place on the image</param>
        /// <param name="x">The x-coordinate of the tile. Zero-indexed</param>
        /// <param name="y">The y-coordinate of the tile. Zero-indexed</param>
        /// <param name="tileSize">The number of pixels that the side of one tile is. MUST be identical to the tilesize used to create the image.</param>
        public static void OverwriteTileSingleThread(WriteableBitmap image, Tile tile, int x, int y, int tileSize)
        {
            //The tilesize given here MUST match the tilesize used to create the image originally.
            var sourceRect = new Int32Rect(x * tileSize, y * tileSize, tileSize, tileSize);

            //Grab the original, non-selected, image of the tile.
            var tileImage = TileHelper.GetTileImage(tile);
            Debug.Assert(image.Format.BitsPerPixel == tileImage.Format.BitsPerPixel, "Number of bits per pixel of new tile image doesn't match.");
            //Scaled to our desired tile size.
            var scaledTile = new TransformedBitmap(tileImage, new ScaleTransform(tileSize / (double) tileImage.PixelWidth, tileSize / (double) tileImage.PixelHeight));
            var tileStride = (scaledTile.PixelWidth * scaledTile.Format.BitsPerPixel + 7) / 8;
            var tilePixels = new byte[scaledTile.PixelHeight * tileStride];
            scaledTile.CopyPixels(tilePixels, tileStride, 0);

            image.WritePixels(sourceRect, tilePixels, tileStride, 0); //The 'offset' parameter is the offset into the tilePixels-array, NOT the actual image.
        }
    }
}
