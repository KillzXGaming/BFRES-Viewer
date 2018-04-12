using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Graphics.OpenGL;

namespace BFRES
{
    public class RenderTexture
    {
        public List<byte[]> mipmaps = new List<byte[]>();
        public int id;
        public int width;
        public int height;
        public PixelInternalFormat type;
        public OpenTK.Graphics.OpenGL.PixelFormat utype;

        public void load()
        {
            id = GL.GenTexture();

            GL.BindTexture(TextureTarget.Texture2D, id);

            if (type != PixelInternalFormat.Rgba)
            {
                GL.CompressedTexImage2D<byte>(TextureTarget.Texture2D, 0, type,
                    width, height, 0, getImageSize(mipmaps[0]), mipmaps[0]);
            }
            else
            {
                GL.TexImage2D<byte>(TextureTarget.Texture2D, 0, type, width, height, 0,
                    utype, PixelType.UnsignedByte, mipmaps[0]);
            }

            GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);
        }

        public int getImageSize(byte[] data)
        {
            switch (type)
            {
                //case PixelInternalFormat.CompressedRgbaS3tcDxt1Ext:
                //case PixelInternalFormat.CompressedSrgbAlphaS3tcDxt1Ext:
                //case PixelInternalFormat.CompressedRedRgtc1:
                case PixelInternalFormat.CompressedSignedRedRgtc1:
                    return (width * height / 2);
                //case PixelInternalFormat.CompressedRgbaS3tcDxt3Ext:
                //case PixelInternalFormat.CompressedSrgbAlphaS3tcDxt3Ext:
                //case PixelInternalFormat.CompressedRgbaS3tcDxt5Ext:
                //case PixelInternalFormat.CompressedSrgbAlphaS3tcDxt5Ext:
                //case PixelInternalFormat.CompressedSignedRgRgtc2:
                case PixelInternalFormat.CompressedRgRgtc2:
                    return (width * height);
                case PixelInternalFormat.Rgba:
                    return data.Length;
                default:
                    return data.Length;
            }
        }

        public unsafe void ExportAsImage(string path)
        {
            Bitmap bitmap = new Bitmap(width, height);
            System.Drawing.Imaging.BitmapData bitmapData = bitmap.LockBits(new Rectangle(0, 0, width, height), System.Drawing.Imaging.ImageLockMode.WriteOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            GL.BindTexture(TextureTarget.Texture2D, id);
            GL.GetTexImage(TextureTarget.Texture2D, 0, PixelFormat.Bgra, PixelType.UnsignedByte, bitmapData.Scan0);

           /* Int32* ptr = (Int32*)bitmapData.Scan0.ToPointer();
            byte[] imagePixels = (mipmaps[0]);
            fixed (byte* pixels = imagePixels)
            {
                Int32* pixelsInt = (Int32*)pixels;
                for (int i = 0; i < imagePixels.Length / 4; i++)
                {
                    ptr[i] = (pixelsInt[i] >> 8) | ((pixelsInt[i] << 24) & 0xFF); //rgbA to Argb
                }
            }*/

            bitmap.UnlockBits(bitmapData);
            bitmap.Save(path);
        }
    }
}
