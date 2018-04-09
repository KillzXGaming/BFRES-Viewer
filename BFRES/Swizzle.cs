using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BFRES
{
    public class Swizzle
    {

        public struct GX2Surface
        {
            public int dim;
            public int depth;
            public int numMips;
            public uint format;
            public int aa;
            public int use;
            public int resourceFlags;
            public int imageSize;
            public int imagePtr;
            public int pMem;
            public int mipSize;
            public int mipPtr;
            public int tileMode;
            public int swizzle;
            public int alignment;
            public int pitch;
            public int sizeRange;

            public byte[] data;

            public int[] mipOffset;


            public int width
            {
                set;
                get;
            }
            public int height
            {
                set;
                get;
            }
        };
        public enum SurfaceFormat
        {
            FORMAT_INVALID = 0x0,
            FORMAT_T_BC1_UNORM = 0x1a01,
            FORMAT_T_BC1_SRGB = 0x1a06,
            FORMAT_T_BC2_UNORM = 0x1b01,
            FORMAT_T_BC2_SRGB = 0x1b06,
            FORMAT_T_BC3_UNORM = 0x1c01,
            FORMAT_T_BC3_SRGB = 0x1c06,
            FORMAT_T_BC4_UNORM = 0x1d01,
            FORMAT_T_BC4_SNORM = 0x1d02,
            FORMAT_T_BC5_UNORM = 0x1e01,
            FORMAT_T_BC5_SNORM = 0x1e02,
            FORMAT_TCS_R8_G8_B8_A8_UNORM = 0x0b01,
            FORMAT_FIRST = 0x1,
            FORMAT_LAST = 0x83F,
        };

        public enum BCn_formats
        {
            BC1 = 0x1a,
            BC2 = 0x1b,
            BC3 = 0x1c,
            BC4 = 0x1d,
            BC5 = 0x1e,
            BC6 = 0x1f,
            BC7 = 0x20,
        }

        public enum ASTC_formats
        {
            ASTC4x4 = 0x2d,
            ASTC5x4 = 0x2e,
            ASTC5x5 = 0x2f,
            ASTC6x5 = 0x30,
            ASTC6x6 = 0x31,
            ASTC8x5 = 0x32,
            ASTC8x6 = 0x33,
            ASTC8x8 = 0x34,
            ASTC10x5 = 0x35,
            ASTC10x6 = 0x36,
            ASTC10x8 = 0x37,
            ASTC10x10 = 0x38,
            ASTC12x10 = 0x39,
            ASTC12x12 = 0x3a,
        }

        public static uint blk_dims(uint format, uint blkWidth, uint blkHeight) //Blk height/width
        {
            if (format == 0x2d)
                return (4 | 4);
            else if (format == 0x2e)
                return (5 | 4);
            else if (format == 0x2f)
                return (5 | 5);
            else if (format == 0x30)
                return (6 | 6);
            else if (format == 0x31)
                return (6 | 6);
            else if (format == 0x32)
                return (8 | 5);
            else if (format == 0x33)
                return (8 | 6);
            else if (format == 0x34)
                return (8 | 8);
            else if (format == 0x35)
                return (10 | 5);
            else if (format == 0x36)
                return (10 | 6);
            else if (format == 0x37)
                return (10 | 8);
            else if (format == 0x38)
                return (10 | 10 );
            else if (format == 0x39)
                return (12 | 10);
            else
                return (12 | 12);

        }

        public static byte[] bpps;  //Bytes per pixel
        

        

        /*---------------------------------------
         * 
         * Code ported from Aboood's BNTX Extractor https://github.com/aboood40091/BNTX-Extractor/blob/master/swizzle.py
         * 
         * With help by gdkchan!
         * 
         *---------------------------------------*/


        public static int DIV_ROUND_UP(int n, int d)
        {
            return (n + d - 1) / d;
        }
        public static int round_up(int x, int y)
        {
            return ((x - 1) | (y - 1)) + 1;
        }

        public static byte[] _swizzle(int width, int height, int blkWidth, int blkHeight, int bpp, int tileMode, int alignment, int size_range, uint format, byte[] data, int toSwizzle)
        {
            GX2Surface sur = new GX2Surface();
            sur.width = width;
            sur.height = height;
            sur.format = format;
            sur.data = data;
            sur.imageSize = data.Length;

            int block_height = 1 << size_range;

            width = DIV_ROUND_UP(sur.width, blkWidth);
            height = DIV_ROUND_UP(sur.height, blkHeight);

            int pitch;
            if (tileMode == 0)
                pitch = round_up(width * bpp, alignment * 64);
            else
                pitch = round_up(width * bpp, 64);

            int surfSize = round_up(pitch * round_up(height, block_height * 8), alignment);

            byte[] result = new byte[surfSize];

            for (int y = 0; y < width; y++)
            {
                for (int x = 0; x < height; x++)
                {
                    int pos;
                    int pos_;

                    if (tileMode == 0)
                        pos = y * pitch + x * bpp;

                    else
                        pos = getAddrBlockLinear(x, y, width, bpp, 0, block_height);

                    pos_ = (y * width + x) * bpp;

                }
            }
            return result;
        }

        public static byte[] deswizzle(int width, int height, int blkWidth, int blkHeight, int bpp, int tileMode, int alignment, int size_range, uint format, byte[] data, int toSwizzle)
        {
            return _swizzle(width, height, blkWidth, blkHeight, bpp, tileMode, alignment, size_range, format, data, toSwizzle);
        }

        public static byte[] swizzle(int width, int height, int blkWidth, int blkHeight, int bpp, int tileMode, int alignment, int size_range, uint format, byte[] data, int toSwizzle)
        {
            return _swizzle(width, height, blkWidth, blkHeight, bpp, tileMode, alignment, size_range, format, data, toSwizzle);
        }

   


        static int getAddrBlockLinear(int x, int y, int width, int bytes_per_pixel, int base_address, int block_height)
        {
            /*
              From Tega X1 TRM 
                               */
            int image_width_in_gobs = DIV_ROUND_UP(width * bytes_per_pixel, 64);


            int GOB_address = (base_address
                           + (y / (8 * block_height)) * 512 * block_height * image_width_in_gobs
                           + (x * bytes_per_pixel / 64) * 512 * block_height
                           + (y % (8 * block_height) / 8) * 512);

            x *= bytes_per_pixel;

            int Address = (GOB_address + ((x % 64) / 32) * 256 + ((y % 8) / 2) * 64
               + ((x % 32) / 16) * 32 + (y % 2) * 16 + (x % 16));

            return Address;
        }
    }
}
