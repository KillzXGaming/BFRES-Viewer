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
            public int format;
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

        public enum blk_dims //Blk height/width
        {
            dim_4x4 = 0x2d,
            dim_5x4 = 0x2e,
            dim_5x5 = 0x2f,
            dim_6x5 = 0x30,
            dim_6x6 = 0x31,
            dim_8x5 = 0x32,
            dim_8x6 = 0x33,
            dim_8x8 = 0x34,
            dim_10x5 = 0x35,
            dim_10x6 = 0x36,
            dim_10x8 = 0x37,
            dim_10x10 = 0x38,
            dim_12x10 = 0x39,
            dim_12x12 = 0x3a,
        }

        public enum bpps  //Bytes per pixel
        {
                
        }

        /*---------------------------------------
         * 
         * Code ported from Aboood's GTX Extractor https://github.com/aboood40091/GTX-Extractor/blob/master/gtx_extract.py
         * 
         * With help by gdkchan!
         * 
         *---------------------------------------*/

        public static byte[] swizzleBC(byte[] data, int width, int height, int format)
        {
            GX2Surface sur = new GX2Surface();

            int swizzle = (format >> 8);


            int W = (sur.width + 3) / 4;
            int H = (sur.height + 3) / 4;
            


            sur.width = width;
            sur.height = height;
            sur.format = format;
            sur.data = data;
            sur.imageSize = data.Length;
            //return swizzleBC(sur);

            byte[] Output = new byte[W * H * 64];



            for (int Y = 0; Y < H; Y++)
            {
                for (int X = 0; X < W; X++)
                {

                    byte[] Tile;
                    int TOffset = 0;


                    for (int TY = 0; TY < 4; TY++)
                    {
                        for (int TX = 0; TX < 4; TX++)
                        {
                            int OOffset = (X * 4 + TX + (Y * 4 + TY) * W * 4) * 4;

                            TOffset += 4;
                        }
                    }
                }
            }

    

            return swizzleSurface(sur, (SurfaceFormat)sur.format != SurfaceFormat.FORMAT_TCS_R8_G8_B8_A8_UNORM);
        }

        public static int getBPP(int i)
        {
            switch ((SurfaceFormat)i)
            {
                case SurfaceFormat.FORMAT_TCS_R8_G8_B8_A8_UNORM:
                    return 0x20;
                case SurfaceFormat.FORMAT_T_BC1_UNORM:
                case SurfaceFormat.FORMAT_T_BC1_SRGB:
                    return 0x40;
                case SurfaceFormat.FORMAT_T_BC2_UNORM:
                    return 0x80;
                case SurfaceFormat.FORMAT_T_BC3_UNORM:
                case SurfaceFormat.FORMAT_T_BC3_SRGB:
                    return 0x80;
                case SurfaceFormat.FORMAT_T_BC4_UNORM:
                    return 0x40;
                case SurfaceFormat.FORMAT_T_BC5_UNORM:
                case SurfaceFormat.FORMAT_T_BC5_SNORM:
                    return 0x80;
            }
            Console.WriteLine("UnkFormat:" + (SurfaceFormat)i);
            return -1;
        }

        public static byte[] swizzleSurface(GX2Surface surface, bool isCompressed)
        {
            byte[] original = new byte[surface.data.Length];

            surface.data.CopyTo(original, 0);

            int swizzle = ((surface.swizzle >> 8) & 1) + (((surface.swizzle >> 9) & 3) << 1);
            int blockSize;
            int width = surface.width;
            int height = surface.height;

            int format = getBPP(surface.format);

            if (isCompressed)
            {
                width /= 4;
                height /= 4;

                if ((SurfaceFormat)surface.format == SurfaceFormat.FORMAT_T_BC1_UNORM ||
                    (SurfaceFormat)surface.format == SurfaceFormat.FORMAT_T_BC4_UNORM ||
                    (SurfaceFormat)surface.format == SurfaceFormat.FORMAT_T_BC1_SRGB)
                {
                    blockSize = 8;
                }
                else
                {
                    blockSize = 16;
                }
            }
            else
            {
                blockSize = format / 8;
            }

            return surface.data;
        }
    }
}
