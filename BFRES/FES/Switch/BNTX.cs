using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace BFRES
{
    public class Formats
    {
        public enum BNTXImageFormat
        {
            IMAGE_FORMAT_INVALID = 0x0,
            IMAGE_FORMAT_R8_G8_B8_A8_UNORM = 0x0b01,
            IMAGE_FORMAT_R8_G8_B8_A8_SRGB = 0x0b06,
            IMAGE_FORMAT_R5_G6_B5_UNORM = 0x0701,
            IMAGE_FORMAT_R8_UNORM = 0x0201,
            IMAGE_FORMAT_R8_G8_UNORM = 0x0901,
            IMAGE_FORMAT_BC1_UNORM = 0x1a01,
            IMAGE_FORMAT_BC1_SRGB = 0x1a06,
            IMAGE_FORMAT_BC2_UNORM = 0x1b01,
            IMAGE_FORMAT_BC2_SRGB = 0x1b06,
            IMAGE_FORMAT_BC3_UNORM = 0x1c01,
            IMAGE_FORMAT_BC3_SRGB = 0x1c06,
            IMAGE_FORMAT_BC4_UNORM = 0x1d01,
            IMAGE_FORMAT_BC4_SNORM = 0x1d02,
            IMAGE_FORMAT_BC5_UNORM = 0x1e01,
            IMAGE_FORMAT_BC5_SNORM = 0x1e02,
            IMAGE_FORMAT_BC6H_UF16 = 0x1f01,
            IMAGE_FORMAT_BC6H_SF16 = 0x1f02,
            IMAGE_FORMAT_BC7_UNORM = 0x2001,
            IMAGE_FORMAT_BC7_SRGB = 0x2006,
        };
    }
    class BNTXData : FileBase
    {
        public BNTXData(FileData f)
        {

        }
    }

    public class BNTX : TreeNode
    {

        List<BRTI> textures = new List<BRTI>();
        int BRTIOffset;

        public BNTX(FileData f)
        {
   
            int temp = f.pos();


            f.skip(8); //Magic
            int Version = f.readInt();
            int ByteOrderMark = f.readShort();
            int FormatRevision = f.readShort();
            Text = f.readString(f.readOffset() + ExternalFiles.DataOffset, -1);
            f.skip(2);
            int strOffset = f.readShort();
            int relocOffset = f.readInt();
            int FileSize = f.readInt();


            f.skip(4); //NX Magic
            int TexturesCount = f.readInt();
            int InfoPtrsOffset = f.readOffset();
            int DataBlockOffset = f.readOffset();
            int DictOffset = f.readOffset();
            int strDictSize = f.readInt();




            for (int i = 0; i < TexturesCount; i++)
            {
                f.seek(InfoPtrsOffset + ExternalFiles.DataOffset + i * 8);
                BRTIOffset = f.readOffset();


                f.seek(BRTIOffset + ExternalFiles.DataOffset);

                textures.Add(new BRTI(f));
            }

            Nodes.AddRange(textures.ToArray());

        }
    }
    public class BRTI : RenderableNode
    {
        Swizzle.GX2Surface sur;
        public RenderTexture tex = new RenderTexture();


        public  BRTI(FileData f) //Docs thanks to gdkchan!!
        {
            ImageKey = "texture";
            SelectedImageKey = "texture";

            if (!f.readString(4).Equals("BRTI"))
                throw new Exception("Error reading Texture");

            int BRTISize1 = f.readInt();
            int BRTISize2 = f.readInt();
            f.skip(4); //padding
            sur = new Swizzle.GX2Surface();

            sur.tileMode = f.readByte();
            sur.dim = f.readByte();
            int Flags = f.readShort();
            sur.swizzle = f.readShort();
            sur.numMips = f.readShort();
            int unk2 = f.readInt();
            sur.format = (uint)f.readInt();
            int unk3 = f.readInt();
            sur.width = f.readInt();
            sur.height = f.readInt();
            sur.depth = f.readInt();
            int FaceCount = f.readInt();
            sur.sizeRange = f.readInt();
            int unk38 = f.readInt();
            int unk3C = f.readInt();
            int unk40 = f.readInt();
            int unk44 = f.readInt();
            int unk48 = f.readInt();
            int unk4C = f.readInt();
            int datalength = f.readInt();
            sur.alignment = f.readInt();   
            int ChannelType = f.readInt();
            int TextureType = f.readInt();
            Text = f.readString(f.readOffset() + ExternalFiles.DataOffset + 2, -1);
            int unk5 = f.readInt();
            int ParentOffset = f.readInt();
            int unk6 = f.readInt();
            int PtrsOffset = f.readInt();


            f.seek(PtrsOffset + ExternalFiles.DataOffset);
            int dataOff = f.readOffset();
            f.seek(dataOff + ExternalFiles.DataOffset);
            sur.data = f.getSection(dataOff, datalength);
            //Console.WriteLine(sur.data.Length + " " + dataOff.ToString("x") + " " + datalength);

            Console.WriteLine("Texture Size = " + sur.height + " x " + sur.width);

            int blkWidth = 1;
            int blkHeight = 1;
            int bpp = 1;

            Console.WriteLine(sur.format);


            Swizzle.blk_dims(sur.format >> 8, blkWidth , blkHeight);
            Swizzle.bpps(sur.format >> 8, bpp);

            byte[] result = Swizzle.deswizzle(sur.width, sur.height, blkWidth, blkHeight, bpp, sur.tileMode, sur.alignment, sur.sizeRange, sur.format, sur.data, sur.swizzle);


            tex.mipmaps.Add(result);
            tex.width = sur.width;
            tex.height = sur.height;

            //File.WriteAllBytes(dataOff.ToString("x") + ".bin" ,data);

            PropertyGridSimpleDemoClass propgrid = new PropertyGridSimpleDemoClass();

            switch (sur.format)
            {
                case ((int)Swizzle.SurfaceFormat.FORMAT_T_BC1_UNORM):
                    tex.type = PixelInternalFormat.CompressedRgbaS3tcDxt1Ext;
                    propgrid.Format = "FORMAT_T_BC1_UNORM";
                    break;
                case ((int)Swizzle.SurfaceFormat.FORMAT_T_BC1_SRGB):
                    tex.type = PixelInternalFormat.CompressedSrgbAlphaS3tcDxt1Ext;
                    propgrid.Format = "FORMAT_T_BC1_SRGB";
                    break;
                case ((int)Swizzle.SurfaceFormat.FORMAT_T_BC2_UNORM):
                    tex.type = PixelInternalFormat.CompressedRgbaS3tcDxt3Ext;
                    break;
                case ((int)Swizzle.SurfaceFormat.FORMAT_T_BC2_SRGB):
                    tex.type = PixelInternalFormat.CompressedSrgbAlphaS3tcDxt3Ext;
                    break;
                case ((int)Swizzle.SurfaceFormat.FORMAT_T_BC3_UNORM):
                    tex.type = PixelInternalFormat.CompressedRgbaS3tcDxt5Ext;
                    propgrid.Format = "FORMAT_T_BC3_UNORM";
                    break;
                case ((int)Swizzle.SurfaceFormat.FORMAT_T_BC3_SRGB):
                    tex.type = PixelInternalFormat.CompressedSrgbAlphaS3tcDxt5Ext;
                    propgrid.Format = "FORMAT_T_BC3_SRGB";
                    break;
                case ((int)Swizzle.SurfaceFormat.FORMAT_T_BC4_UNORM):
                    tex.type = PixelInternalFormat.CompressedRedRgtc1;
                    propgrid.Format = "FORMAT_T_BC4_UNORM";
                    break;
                case ((int)Swizzle.SurfaceFormat.FORMAT_T_BC4_SNORM):
                    tex.type = PixelInternalFormat.CompressedSignedRedRgtc1;
                    propgrid.Format = "FORMAT_T_BC4_SNORM";
                    break;
                case ((int)Swizzle.SurfaceFormat.FORMAT_T_BC5_UNORM):
                    tex.type = PixelInternalFormat.CompressedRgRgtc2;
                    propgrid.Format = "FORMAT_T_BC5_UNORM";
                    break;
                case ((int)Swizzle.SurfaceFormat.FORMAT_T_BC5_SNORM):
                    tex.type = PixelInternalFormat.CompressedSignedRgRgtc2;
                    propgrid.Format = "FORMAT_T_BC5_SNORM";
                    break;
                case ((int)Swizzle.SurfaceFormat.FORMAT_TCS_R8_G8_B8_A8_UNORM):
                    tex.type = PixelInternalFormat.Rgba;
                    tex.utype = OpenTK.Graphics.OpenGL.PixelFormat.Rgba;
                    break;
                   // default:
                    //throw new Exception("Unknown format");
            }

         //   tex.load();



            //Property grid stuff


        }
        public override void Render(Matrix4 v)
        {
            Console.WriteLine("Rendering texture");

            

            GL.MatrixMode(MatrixMode.Projection);
            GL.LoadIdentity();
            GL.BindTexture(TextureTarget.Texture2D, tex.id);

            GL.Begin(PrimitiveType.Quads);
            GL.TexCoord2(1, 1);
            GL.Vertex2(1, -1);
            GL.TexCoord2(0, 1);
            GL.Vertex2(-1, -1);
            GL.TexCoord2(0, 0);
            GL.Vertex2(-1, 1);
            GL.TexCoord2(1, 0);
            GL.Vertex2(1, 1);
            GL.End();
        }
        public class PropertyGridSimpleDemoClass
        {

            public int Width
            {
                get; 
                set; 
            }

            public string Format;

            public string DisplayFormat
            {
                get { return Format; }
                set { Format = value; }
            }

            public int Height
            {
                get;
                set;
            }

            string m_DisplayString;
            public string DisplayString
            {
                get { return m_DisplayString; }
                set { m_DisplayString = value; }
            }

            bool m_DisplayBool;
            public bool DisplayBool
            {
                get { return m_DisplayBool; }
                set { m_DisplayBool = value; }
            }

        }

    }

}

