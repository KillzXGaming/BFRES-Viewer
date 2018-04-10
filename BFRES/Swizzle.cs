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
            public uint alignment;
            public int pitch;
            public uint sizeRange;

            public byte[] data;

            public int[] mipOffset;


            public uint width
            {
                set;
                get;
            }
            public uint height
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

        public static uint blk_dims(uint format) //Blk height/width
        {
            uint blkWidth; uint blkHeight;
            if (format == 0x2d) { blkWidth = 4; blkHeight = 4; }
            else if (format == 0x1a) {blkWidth = 4; blkHeight = 4; } 
            else if (format == 0x1b) { blkWidth = 4; blkHeight = 4; }
            else if (format == 0x1c) { blkWidth = 4; blkHeight = 4; }
            else if (format == 0x1e) { blkWidth = 4; blkHeight = 4; }
            else if (format == 0x1f) { blkWidth = 4; blkHeight = 4; }
            else if (format == 0x1d) { blkWidth = 4; blkHeight = 4; }
            else if (format == 0x20) { blkWidth = 4; blkHeight = 4; }
            else if (format == 0x2e) { blkWidth = 5; blkHeight = 4; }
            else if (format == 0x2f) { blkWidth = 5; blkHeight = 5; }
            else if (format == 0x30) { blkWidth = 6; blkHeight = 6; }
            else if (format == 0x31) { blkWidth = 6; blkHeight = 6; }
            else if (format == 0x32) { blkWidth = 6; blkHeight = 6; }
            else if (format == 0x33) { blkWidth = 8; blkHeight = 6; }
            else if (format == 0x34) { blkWidth = 8; blkHeight = 8; }
            else if (format == 0x35) { blkWidth = 10; blkHeight = 5; }
            else if (format == 0x36) { blkWidth = 10; blkHeight = 6; }
            else if (format == 0x37) { blkWidth = 10; blkHeight = 8; }
            else if (format == 0x38) { blkWidth = 10; blkHeight = 10; }
            else if (format == 0x39) { blkWidth = 12; blkHeight = 10; }
            else if (format == 0x3a) { blkWidth = 12; blkHeight = 12; }
            else
                throw new Exception("Unknown format");

            return blkWidth << 4 | blkHeight;
        }

        public static uint bpps(uint format)  //Bytes per pixel
        {
            uint bpp;
            if (format == 0x0b ) bpp = 0x04;
            else if (format == 0x1a) bpp = 0x8;
            else if(format == 0x1b) bpp = 0x10;
            else if(format == 0x1c) bpp = 0x10;
            else if(format == 0x1d) bpp = 0x08;
            else if(format == 0x1e) bpp = 0x08;
            else if(format == 0x1f) bpp = 0x10;
            else if(format == 0x20) bpp = 0x10;
            else
                throw new Exception("Unknown format");
            return bpp;
        }



        /*---------------------------------------
         * 
         * Code ported from Aboood's BNTX Extractor https://github.com/aboood40091/BNTX-Extractor/blob/master/swizzle.py
         * 
         *---------------------------------------*/


        public static uint DIV_ROUND_UP(uint n, uint d)
        {
            return (n + d - 1) / d;
        }
        public static uint round_up(uint x, uint y)
        {
            return ((x - 1) | (y - 1)) + 1;
        }

        public static byte[] _swizzle(uint width, uint height, uint blkWidth, uint blkHeight, uint bpp, int tileMode, uint alignment, uint size_range, uint format, byte[] data, int toSwizzle)
        {
            GX2Surface sur = new GX2Surface();

            sur.imageSize = data.Length;

            uint block_height = 1 << size_range;

            width = DIV_ROUND_UP(sur.width, blkWidth);
            height = DIV_ROUND_UP(sur.height, blkHeight);

            uint pitch;
            if (tileMode == 0)
                pitch = round_up(width * bpp, alignment * 64);
            else
                pitch = round_up(width * bpp, 64);

            uint surfSize = round_up(pitch * round_up(height, block_height * 8), alignment);

            byte[] result = new byte[surfSize];

            for (uint y = 0; y < width; y++)
            {
                for (uint x = 0; x < height; x++)
                {
                    uint pos;
                    uint pos_;

                    if (tileMode == 0)
                        pos = y * pitch + x * bpp;

                    else
                        pos = getAddrBlockLinear(x, y, width, bpp, 0, block_height);

                    pos_ = (y * width + x) * bpp;

                }
            }
            return result;
        }

        public static byte[] deswizzle(uint width, uint height, uint blkWidth, uint blkHeight, uint bpp, int tileMode, uint alignment, uint size_range, uint format, byte[] data, int toSwizzle)
        {
            return _swizzle(width, height, blkWidth, blkHeight, bpp, tileMode, alignment, size_range, format, data, toSwizzle);
        }

        public static byte[] swizzle(uint width, uint height, uint blkWidth, uint blkHeight, uint bpp, int tileMode, uint alignment, uint size_range, uint format, byte[] data, int toSwizzle)
        {
            return _swizzle(width, height, blkWidth, blkHeight, bpp, tileMode, alignment, size_range, format, data, toSwizzle);
        }

   


        static uint getAddrBlockLinear(uint x, uint y, uint width, uint bytes_per_pixel, uint base_address, uint block_height)
        {
            /*
              From Tega X1 TRM 
                               */
            uint image_width_in_gobs = DIV_ROUND_UP(width * bytes_per_pixel, 64);


            uint GOB_address = (base_address
                           + (y / (8 * block_height)) * 512 * block_height * image_width_in_gobs
                           + (x * bytes_per_pixel / 64) * 512 * block_height
                           + (y % (8 * block_height) / 8) * 512);

            x *= bytes_per_pixel;

            uint Address = (GOB_address + ((x % 64) / 32) * 256 + ((y % 8) / 2) * 64
               + ((x % 32) / 16) * 32 + (y % 2) * 16 + (x % 16));

            return Address;
        }
    }
}
