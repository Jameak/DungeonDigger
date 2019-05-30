using System;
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

                Debug.Assert(output.Format.BitsPerPixel == 32);
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

            SaveBitmapSource(img, path);
        }

        /// <summary>
        /// Create an image from the map where each tile has the specified size.
        /// </summary>
        /// <param name="map">The map for which to create an image</param>
        /// <param name="tileSize">The size of each tile</param>
        /// <returns>The resulting bitmap</returns>
        public static BitmapSource CreateBitmap(UITile[,] map, int tileSize)
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

                    var imageData = TileHelper.GetTilePixels(map[x, y], tileSize);
                    var tilePixels = imageData.Data;
                    var pixelWidth = imageData.Width;
                    var pixelHeight = imageData.Height;
                    
                    for (int tx = 0; tx < pixelWidth; tx++)
                    {
                        for (int ty = 0; ty < pixelHeight; ty++)
                        {
                            //Offset on the full image for the position of the pixel
                            var imageTileOffset = pixelOffset + (ty * width + tx) * imagedepth;
                            //Offset on the tile image for the position of the pixel
                            var tileOffset = (ty * pixelWidth + tx) * imagedepth;

                            pixels[imageTileOffset + 0] = tilePixels[tileOffset + 0]; //B
                            pixels[imageTileOffset + 1] = tilePixels[tileOffset + 1]; //G
                            pixels[imageTileOffset + 2] = tilePixels[tileOffset + 2]; //R
                            pixels[imageTileOffset + 3] = tilePixels[tileOffset + 3]; //A
                        }
                    }
                }
            });
            
            var image = BitmapSource.Create(width, height, 96, 96, PixelFormats.Pbgra32, null, pixels, stride);
            Debug.Assert(image.Format.BitsPerPixel == 32);
            return image;
        }

        /// <summary>
        /// Overlay the image representing a selection on top of the specified tile, letting the tile poke through where the selection-image is transparent.
        /// Use <see cref="OverwriteTile"/> to deselect a tile.
        /// </summary>
        /// <param name="backBufferPtr">A pointer to the backbuffer of the WriteableBitmap. See <see cref="WriteableBitmap.BackBuffer"/></param>
        /// <param name="backBufferStride">The stride of the backbuffer of the WriteableBitmap. See <see cref="WriteableBitmap.BackBufferStride"/></param>
        /// <param name="x">The x-coordinate of the tile. Zero-indexed</param>
        /// <param name="y">The y-coordinate of the tile. Zero-indexed</param>
        /// <param name="tileSize">The number of pixels that the side of one tile is. MUST be identical to the tilesize used to create the image.</param>
        /// <returns>A rectangle specifing the dirty rectangle that has to be updated.</returns>
        public static unsafe Int32Rect SelectTile(IntPtr backBufferPtr, int backBufferStride, int x, int y, int tileSize)
        {
            byte[] selectionPixels;
            TileHelper.Colour[] selectionColours;
            int pixelWidth;
            int pixelHeight;
            if (tileSize != TileHelper.UI_MAP_TILE_PIXELSIZE)
            {
                //We have to scale the tile to the desired size.
                var selectionImage = TileHelper.Selection;
                var scaled = new TransformedBitmap(selectionImage,
                    new ScaleTransform(tileSize / (double) selectionImage.PixelWidth, tileSize / (double) selectionImage.PixelHeight));
                selectionPixels = TileHelper.CopyPixels(scaled);
                selectionColours = TileHelper.ConvertToColour(selectionPixels);
                pixelWidth = scaled.PixelWidth;
                pixelHeight = scaled.PixelHeight;
            }
            else
            {
                //We can use our precomputed values.
                var imageData = TileHelper.SelectionPixels;
                selectionPixels = imageData.Data;
                selectionColours = TileHelper.SelectionColours.Data;
                pixelWidth = imageData.Width;
                pixelHeight = imageData.Height;
            }
            
            //Only use the non-transparent pixels from the selection, by leaving the tile-pixels intact where the selection is transparent.
            for (int i = 0; i < pixelWidth; i++)
            {
                for (int j = 0; j < pixelHeight; j++)
                {
                    var offset = (j * pixelWidth + i) * 4; // Image depth is 4
                    var offsetColour = j * pixelWidth + i; //Offset into the array of Colour-values

                    var selectAlpha = selectionPixels[offset + 3];
                    if (selectAlpha != 0) //The pixel is not transparent
                    {
                        // The location of a pixel in the backbuffer is: bmp.BackBuffer + (bmp.BackBufferStride * Y) + X * (bmp.Format.BitsPerPixel / 8)
                        var ptr = (TileHelper.Colour*) (backBufferPtr + (y * tileSize + j) * backBufferStride + (x * tileSize + i) * 4);
                        *ptr = selectionColours[offsetColour];
                    }
                }
            }
            
            return new Int32Rect(x * tileSize, y * tileSize, tileSize, tileSize);
        }
        
        /// <summary>
        /// Overlay the tile-image on the given image, at position x, y.
        /// </summary>
        /// <param name="backBufferPtr">A pointer to the backbuffer of the WriteableBitmap. See <see cref="WriteableBitmap.BackBuffer"/></param>
        /// <param name="backBufferStride">The stride of the backbuffer of the WriteableBitmap. See <see cref="WriteableBitmap.BackBufferStride"/></param>
        /// <param name="tile">The tile to place on the image</param>
        /// <param name="x">The x-coordinate of the tile. Zero-indexed</param>
        /// <param name="y">The y-coordinate of the tile. Zero-indexed</param>
        /// <param name="tileSize">The number of pixels that the side of one tile is. MUST be identical to the tilesize used to create the image.</param>
        /// <returns>A rectangle specifing the dirty rectangle that has to be updated.</returns>
        public static unsafe Int32Rect OverwriteTile(IntPtr backBufferPtr, int backBufferStride, UITile tile, int x, int y, int tileSize)
        {
            var imageData = TileHelper.GetTileColours(tile, tileSize);
            var tileColours = imageData.Data;
            var pixelWidth = imageData.Width;
            var pixelHeight = imageData.Height;
            
            for (int j = 0; j < pixelHeight; j++)
            {
                // The location of a pixel in the backbuffer is: bmp.BackBuffer + (bmp.BackBufferStride * Y) + X * (bmp.Format.BitsPerPixel / 8)
                var ptr = (TileHelper.Colour*)(backBufferPtr + (y * tileSize + j) * backBufferStride + x * tileSize * 4);
                var offset = j * pixelWidth;
                for (int i = 0; i < pixelWidth; i++, ptr++, offset++)
                {
                    *ptr = tileColours[offset];
                }
            }

            return new Int32Rect(x * tileSize, y * tileSize, tileSize, tileSize);
        }
    }
}
