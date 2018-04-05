using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace BFRES
{
    public class IVTX : RenderableNode
    {
        public IVTX(FileData f)
        {
            Text = f.fname;
            Read(f);
        }

        List<IVTXShape> shapes = new List<IVTXShape>();
        List<IVTXObject> obs = new List<IVTXObject>();

        public class IVTXShape
        {
            public int vCount = 0, unk1;
            public int lCount = 0, unk2;
        }

        public class IVTXObject
        {
            public int c1, c2, c3, c4, c5, c6;

            public List<Vector3> verts = new List<Vector3>();
            public List<int> faces = new List<int>();
            public List<int> faces2 = new List<int>();
            public List<Vector4> colors = new List<Vector4>();
            public List<Vector4> colors2 = new List<Vector4>();

            public void Render(Matrix4 v, List<IVTXShape> shapes)
            {
                GL.PointSize(5f);
                GL.LineWidth(2f);
                GL.Begin(PrimitiveType.Triangles);
                int c = 0;
                foreach(int i in faces)
                {
                    GL.Color4(colors[c++]);
                    GL.Vertex3(verts[i] * 4);
                }
                GL.End();
                c = 0;
                GL.Begin(PrimitiveType.LineStrip);
                int start = -1;
                foreach (int i in faces2)
                {
                    GL.Color4(colors2[c++]);
                    GL.Vertex3(verts[i] * 4);
                    if (start == -1)
                    {
                        start = i;
                    }
                    else
                    if(start == i)
                    {
                        start = -1;
                        GL.End();
                        GL.Begin(PrimitiveType.LineStrip);
                    }
                }
                GL.End();

            }
        }

        // offsets are header realative
        public void Read(FileData f)
        {
            f.Endian = Endianness.Little;
            int shapeOffset = f.readInt() + 0x1C;
            int firstOff = f.readInt() + 0x1C;
            int dataOff = f.readInt() + 0x1C;
            int shapeCount = f.readInt();
            int frameCount = f.readInt();

            f.seek(shapeOffset);
            int sc = 0;
            for (int i = 0; i < shapeCount; i++)
            {
                int c = f.readByte();
                f.skip(3);
                for (int j = 0; j < c; j++)
                {
                    shapes.Add(new IVTXShape()
                    {
                        vCount = f.readShort(),
                        lCount = f.readShort(),
                    });
                    sc += shapes[shapes.Count - 1].vCount;
                }
            }

            f.seek(firstOff);
            for(int i = 0; i < frameCount; i++)
            {
                IVTXObject ob = new IVTXObject()
                {
                    c1 = f.readShort(),
                    c2 = f.readShort(),
                    c3 = f.readShort(),
                    c4 = f.readShort(),
                    c5 = f.readShort(),
                    c6 = f.readShort(),
                };
                obs.Add(ob);

                // next I think are colors?
                for(int j =0; j < ob.c2; j++)
                {
                    f.skip(4); // index
                    f.skip(2); // vert offset
                    int ccount = f.readShort();
                    Vector4 col = new Vector4(f.readFloat(), f.readFloat(), f.readFloat(), f.readFloat());
                    for(int k = 0; k < ccount; k++)
                    {
                        ob.colors.Add(col);
                    }
                }
                // next I think are colors?
                for (int j = 0; j < ob.c3; j++)
                {
                    f.skip(4); // index
                    f.skip(2); // vert offset
                    int ccount = f.readShort();
                    Vector4 col = new Vector4(f.readFloat(), f.readFloat(), f.readFloat(), f.readFloat());
                    for (int k = 0; k < ccount; k++)
                    {
                        ob.colors2.Add(col);
                    }
                }
            }


            foreach (IVTXObject ob in obs)
            {
                for (int i = 0; i < ob.c4; i++)
                {
                    ob.verts.Add(new Vector3(f.readFloat(), f.readFloat(), 0));
                }
                for (int i = 0; i < ob.c5; i++)
                {
                    ob.faces.Add(f.readShort());
                }
                for (int i = 0; i < ob.c6; i++)
                {
                    ob.faces2.Add(f.readShort());
                }
            }

            Console.WriteLine(sc + " " + obs[0].faces2.Count + " " + obs[0].faces.Count + " " + obs[0].verts.Count);
            Console.WriteLine(f.pos().ToString("x"));
        }

        public int frame = 0;

        public override void Render(Matrix4 v)
        {

            GL.Disable(EnableCap.Texture2D);
            GL.Disable(EnableCap.DepthTest);
            obs[frame].Render(v, shapes);
            frame++;
            if (frame >= obs.Count) frame = 0;
            GL.Enable(EnableCap.Texture2D);
            GL.Enable(EnableCap.DepthTest);
        }
    }
}
