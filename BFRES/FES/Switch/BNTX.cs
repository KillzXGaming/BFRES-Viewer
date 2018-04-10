using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using System.IO;

namespace BFRES
{
    public class Formats
    {
        public enum BNTXImageFormat
        {
            IMAGE_FORMAT_INVALID = 0x0,
            IMAGE_FORMAT_R8_G8_B8_A8 = 0x0b,
            IMAGE_FORMAT_R5_G6_B5 = 0x07,
            IMAGE_FORMAT_R8 = 0x02,
            IMAGE_FORMAT_R8_G8 = 0x09,
            IMAGE_FORMAT_BC1 = 0x1a,
            IMAGE_FORMAT_BC2 = 0x1b,
            IMAGE_FORMAT_BC3 = 0x1c,
            IMAGE_FORMAT_BC4 = 0x1d,
            IMAGE_FORMAT_BC5 = 0x1e,
            IMAGE_FORMAT_BC6 = 0x1f,
            IMAGE_FORMAT_BC7 = 0x20,
        };

        public enum BNTXImageTypes
        {
            UNORM = 0x01,
            SNORM = 0x02,
            SRGB = 0x06,
        };

        public static uint blk_dims(uint format)
        {
            switch (format)
            {
                case (uint)BNTXImageFormat.IMAGE_FORMAT_BC1:
                case (uint)BNTXImageFormat.IMAGE_FORMAT_BC2:
                case (uint)BNTXImageFormat.IMAGE_FORMAT_BC3:
                case (uint)BNTXImageFormat.IMAGE_FORMAT_BC4:
                case (uint)BNTXImageFormat.IMAGE_FORMAT_BC5:
                case (uint)BNTXImageFormat.IMAGE_FORMAT_BC6:
                case (uint)BNTXImageFormat.IMAGE_FORMAT_BC7:
                case 0x2d:
                    return 0x44;

                case 0x2e: return 0x54;
                case 0x2f: return 0x55;
                case 0x30: return 0x65;
                case 0x31: return 0x66;
                case 0x32: return 0x85;
                case 0x33: return 0x86;
                case 0x34: return 0x88;
                case 0x35: return 0xa5;
                case 0x36: return 0xa6;
                case 0x37: return 0xa8;
                case 0x38: return 0xaa;
                case 0x39: return 0xca;
                case 0x3a: return 0xcc;

                default: return 0x11;
            }
        }

        public static uint bpps(uint format)
        {
            switch (format)
            {
                case (uint)BNTXImageFormat.IMAGE_FORMAT_R8_G8_B8_A8: return 4;
                case (uint)BNTXImageFormat.IMAGE_FORMAT_R8: return 1;

                case (uint)BNTXImageFormat.IMAGE_FORMAT_R5_G6_B5:
                case (uint)BNTXImageFormat.IMAGE_FORMAT_R8_G8:
                    return 2;

                case (uint)BNTXImageFormat.IMAGE_FORMAT_BC1:
                case (uint)BNTXImageFormat.IMAGE_FORMAT_BC4:
                    return 8;

                case (uint)BNTXImageFormat.IMAGE_FORMAT_BC2:
                case (uint)BNTXImageFormat.IMAGE_FORMAT_BC3:
                case (uint)BNTXImageFormat.IMAGE_FORMAT_BC5:
                case (uint)BNTXImageFormat.IMAGE_FORMAT_BC6:
                case (uint)BNTXImageFormat.IMAGE_FORMAT_BC7:
                case 0x2e:
                case 0x2f:
                case 0x30:
                case 0x31:
                case 0x32:
                case 0x33:
                case 0x34:
                case 0x35:
                case 0x36:
                case 0x37:
                case 0x38:
                case 0x39:
                case 0x3a:
                    return 16;
                default: return 0x00;
            }
        }
    }
    class BNTXData : FileBase
    {
        public BNTXData(FileData f)
        {
            //Todo. Open single BNTX instead of one in BFRES
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
        Swizzle.Surface surf;
        public RenderTexture tex = new RenderTexture();


        public BRTI(FileData f) //Docs thanks to gdkchan!!
        {
            ImageKey = "texture";
            SelectedImageKey = "texture";

            if (!f.readString(4).Equals("BRTI"))
                throw new Exception("Error reading Texture");

            int BRTISize1 = f.readInt();
            long BRTISize2 = (f.readInt() | f.readInt() << 32);
            surf = new Swizzle.Surface();

            surf.tileMode = (sbyte)f.readByte();
            surf.dim = (sbyte)f.readByte();
            ushort Flags = (ushort)f.readShort();
            surf.swizzle = (ushort)f.readShort();
            surf.numMips = (ushort)f.readShort();
            uint unk18 = (uint)f.readInt();
            surf.format = (uint)f.readInt();
            byte DataType = (byte)(surf.format & 0xFF);
            uint unk20 = (uint)f.readInt();
            surf.width = f.readInt();
            surf.height = f.readInt();
            surf.depth = f.readInt();
            int FaceCount = f.readInt();
            surf.sizeRange = f.readInt();
            uint unk38 = (uint)f.readInt();
            uint unk3C = (uint)f.readInt();
            uint unk40 = (uint)f.readInt();
            uint unk44 = (uint)f.readInt();
            uint unk48 = (uint)f.readInt();
            uint unk4C = (uint)f.readInt();
            surf.imageSize = f.readInt();
            surf.alignment = f.readInt();
            int ChannelType = f.readInt();
            int TextureType = f.readInt();
            Text = f.readString((f.readOffset() | f.readOffset() << 32) + ExternalFiles.DataOffset + 2, -1);
            long ParentOffset = f.readOffset() | f.readOffset() << 32;
            long PtrsOffset = f.readOffset() | f.readOffset() << 32;

            Console.WriteLine($"ParentOffset {ParentOffset} PtrsOffset {PtrsOffset}");

            f.seek((int)PtrsOffset + ExternalFiles.DataOffset);
            long dataOff = f.readOffset() | f.readOffset() << 32;
            surf.data = f.getSection((int)dataOff + ExternalFiles.DataOffset, surf.imageSize);
            //Console.WriteLine(surf.data.Length + " " + dataOff.ToString("x") + " " + surf.imageSize);

            uint blk_dim = Formats.blk_dims(surf.format >> 8);
            uint blkWidth = blk_dim >> 4;
            uint blkHeight = blk_dim & 0xF;

            uint bpp = Formats.bpps(surf.format >> 8);

            Console.WriteLine($"{Text} Height {surf.height}wdith = {surf.width}allignment = {surf.alignment}blkwidth = {blkWidth}blkheight = {blkHeight}blkdims = {blk_dim} format = {surf.format} datatype = {DataType} dataoffset = {dataOff}");


           // byte[] result = surf.data;


        
            byte[] result = Swizzle.deswizzle((uint)surf.width, (uint)surf.height, blkWidth, blkHeight, bpp, (uint)surf.tileMode, (uint)surf.alignment, surf.sizeRange, surf.data, 0);


            uint width = Swizzle.DIV_ROUND_UP((uint)surf.width, blkWidth);
            uint height = Swizzle.DIV_ROUND_UP((uint)surf.height, blkHeight);

            byte[] result_ = new byte[width * height * bpp];

            Array.Copy(result, 0, result_, 0, width * height * bpp);

 

            tex.mipmaps.Add(result_);
            tex.width = surf.width;
            tex.height = surf.height;

            //File.WriteAllBytes(dataOff.ToString("x") + ".bin" ,data);

            PropertyGridSimpleDemoClass propgrid = new PropertyGridSimpleDemoClass();

            switch (surf.format >> 8)
            {
                case ((uint)Formats.BNTXImageFormat.IMAGE_FORMAT_BC1):
                    if (DataType == (byte)Formats.BNTXImageTypes.UNORM)
                    {
                        tex.type = PixelInternalFormat.CompressedRgbaS3tcDxt1Ext;
                        propgrid.Format = "FORMAT_T_BC1_UNORM";
                    }
                    else if (DataType == (byte)Formats.BNTXImageTypes.SRGB)
                    {
                        tex.type = PixelInternalFormat.CompressedSrgbAlphaS3tcDxt1Ext;
                        propgrid.Format = "FORMAT_T_BC1_SRGB";
                    }
                    else
                        throw new Exception("Unsupported data type");

                    break;
                case ((uint)Formats.BNTXImageFormat.IMAGE_FORMAT_BC2):
                    if (DataType == (byte)Formats.BNTXImageTypes.UNORM)
                        tex.type = PixelInternalFormat.CompressedRgbaS3tcDxt3Ext;

                    else if (DataType == (byte)Formats.BNTXImageTypes.SRGB)
                        tex.type = PixelInternalFormat.CompressedSrgbAlphaS3tcDxt3Ext;

                    else
                        throw new Exception("Unsupported data type");
                    break;
                case ((uint)Formats.BNTXImageFormat.IMAGE_FORMAT_BC3):
                    if (DataType == (byte)Formats.BNTXImageTypes.UNORM)
                    {
                        tex.type = PixelInternalFormat.CompressedRgbaS3tcDxt5Ext;
                        propgrid.Format = "FORMAT_T_BC3_UNORM";
                    }
                    else if (DataType == (byte)Formats.BNTXImageTypes.SRGB)
                    {
                        tex.type = PixelInternalFormat.CompressedSrgbAlphaS3tcDxt5Ext;
                        propgrid.Format = "FORMAT_T_BC3_SRGB";
                    }
                    else
                        throw new Exception("Unsupported data type");

                    break;
                case ((uint)Formats.BNTXImageFormat.IMAGE_FORMAT_BC4):
                    if (DataType == (byte)Formats.BNTXImageTypes.UNORM)
                    {
                        tex.type = PixelInternalFormat.CompressedRedRgtc1;
                        propgrid.Format = "FORMAT_T_BC4_UNORM";
                    }
                    else if (DataType == (byte)Formats.BNTXImageTypes.SNORM)
                    {
                        tex.type = PixelInternalFormat.CompressedSignedRedRgtc1;
                        propgrid.Format = "FORMAT_T_BC4_SNORM";
                    }
                    else
                        throw new Exception("Unsupported data type");

                    break;

                case ((uint)Formats.BNTXImageFormat.IMAGE_FORMAT_BC5):
                    if (DataType == (byte)Formats.BNTXImageTypes.UNORM)
                    {
                        tex.type = PixelInternalFormat.CompressedRedRgtc1;
                        propgrid.Format = "FORMAT_T_BC5_UNORM";
                    }
                    else if (DataType == (byte)Formats.BNTXImageTypes.SNORM)
                    {
                        tex.type = PixelInternalFormat.CompressedSignedRedRgtc1;
                        propgrid.Format = "FORMAT_T_BC5_SNORM";
                    }
                    else
                        Console.WriteLine("Unsupported data type");

                    break;
                case ((uint)Formats.BNTXImageFormat.IMAGE_FORMAT_R8_G8_B8_A8):
                    if (DataType == (byte)Formats.BNTXImageTypes.UNORM || DataType == (byte)Formats.BNTXImageTypes.SRGB)
                    {
                        tex.type = PixelInternalFormat.Rgba;
                        tex.utype = OpenTK.Graphics.OpenGL.PixelFormat.Rgba;
                    }
                    else
                        throw new Exception("Unsupported data type");

                    break;

                default:
                    MessageBox.Show($"Error! Unsupported texture type for {Text}","Error");
                    break;
            }

            tex.load();



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
