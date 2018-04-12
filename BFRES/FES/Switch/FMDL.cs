using System;
using System.IO;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using System.Timers;


namespace BFRES
{


    public class FMDL : RenderableNode
    {
        List<FMAT> mats = new List<FMAT>();
        List<FVTX> vertattr = new List<FVTX>();
        List<FSHP> shapes = new List<FSHP>();
        public FSKL skel;
        
        public BaseRenderData ExportModel(int LODindex = 0)
        {
            BaseRenderData res = new BaseRenderData();
            res.mats = mats;
            foreach (var shape in shapes)
            {
                BaseRenderData curShape = new BaseRenderData();
                curShape.data = vertattr[shape.fvtxindex].VertData;
                curShape.PolygonO = shape.lodModels[LODindex].ShapeData;
                res.Join(curShape);
            }

            foreach (TreeNode n in ((BFRES)Parent.Parent).Nodes) //really ugly :(
            {
                if (n.Text.Equals("Embedded Files"))
                {
                    foreach (TreeNode no in n.Nodes)
                    {
                        foreach (TreeNode tn in no.Nodes)
                        {
                            foreach (TreeNode T in tn.Nodes)
                            {
                                res.textures.Add(((BRTI)T)); 
                            }
                        }
                    }
                    break;
                }
            }

            return res;
        }
        
        public FMDL(FileData f)
        {
            ImageKey = "model";
            SelectedImageKey = "model";

            f.skip(4); // MAGIC

            f.skip(8); // Header Length
            f.skip(4); // padding
            Text = f.readString(f.readOffset() + 2, -1);
            f.skip(4); // padding
            int EndOfStringTable = f.readOffset();
            f.skip(4); // padding
            int FSKLOffset = f.readOffset();
            f.skip(4); // padding
            int FVTXOffset = f.readOffset();
            f.skip(4); // padding
            int FSHPOffset = f.readOffset();
            f.skip(4); // padding
            int FSHPDict = f.readOffset();
            f.skip(4); // padding
            int FMATOffset = f.readOffset();
            f.skip(4); // padding
            int FMATDict = f.readOffset();
            f.skip(4); // padding
            int UserDataOffset = f.readOffset();
            f.skip(20); // padding
            int FVTXCount = f.readShort();
            int FSHPCount = f.readShort();
            int FMATCount = f.readShort();
            int UserData = f.readShort();
            int TotalAmountOfVerticies = f.readShort();
            f.skip(4); // padding

            // FSKL for skeleton
            f.seek(FSKLOffset);
            skel = new FSKL();
            skel.Read(f);
            Nodes.Add(skel);

            // FMAT is obs materials

            for (int i = 0; i < FMATCount; i++)
            {
                f.seek(FMATOffset + (i * 184));
                mats.Add(new FMAT(f));
            }

            // FVTX is the vertex buffer object attributes
            for (int i = 0; i < FVTXCount; i++)
            {
                f.seek(FVTXOffset + (i * 96));

                vertattr.Add(new FVTX(f));
            }

            // FSHP is the mesh objects
            for (int i = 0; i < FSHPCount; i++)
            {
                f.seek(FSHPOffset + (i * 112));

                shapes.Add(new FSHP(f));
            }


            Nodes.AddRange(shapes.ToArray());
            Nodes.AddRange(vertattr.ToArray());
            Nodes.AddRange(mats.ToArray());

            GL.GenBuffers(1, out ibo);
        }

        int ibo;

        public static int Tex2 = 0;


        public override void Render(Matrix4 v)
        {
            //Console.WriteLine("Rendering " + Text);
            GL.UseProgram(BFRES.shader.programID);

            GL.UniformMatrix4(BFRES.shader.getAttribute("modelview"), false, ref v);

            FSKL skel = ((FMDL)shapes[0].Parent).skel;

            Matrix4[] f = skel.getBoneTransforms();
            int[] bind = skel.bindId;
            GL.UniformMatrix4(BFRES.shader.getAttribute("bones"), f.Length, false, ref f[0].Row0.X);
            if (bind.Length != 0)
                GL.Uniform1(BFRES.shader.getAttribute("bonematch"), bind.Length, ref bind[0]);

            BFRES.shader.enableAttrib();
            foreach (FSHP shape in shapes)
            {
                FVTX vert = vertattr[shape.fvtxindex];

                //Textures are disabled atm

   
                
                string tex = mats[shape.fmatIndex].tex[0].Text;
                // find it in textures
                foreach (TreeNode n in ((BFRES)Parent.Parent).Nodes)
                {
                    if (n.Text.Equals("Embedded Files"))
                    {
                        foreach (TreeNode no in n.Nodes)
                        {
                            foreach (TreeNode tn in no.Nodes)
                            {
                                foreach (TreeNode T in tn.Nodes)
                                {
                                    if (T.Text.Equals(tex))
                                    {
                                        Console.WriteLine("Binding " + T.Text);
                                        GL.ActiveTexture(TextureUnit.Texture0);
                                        GL.BindTexture(TextureTarget.Texture2D, ((BRTI)T).tex.id);
                                        GL.Uniform1(BFRES.shader.getAttribute("tex"), 0);
                                        break;
                                    }
                                }
                            }
                        }
                        break;
                    }
                }





                GL.BindBuffer(BufferTarget.ArrayBuffer, vert.gl_vbo);
                GL.VertexAttribPointer(BFRES.shader.getAttribute("_p0"), 3, VertexAttribPointerType.Float, false, BaseRenderData.Vertex.Stride, 0);
                GL.VertexAttribPointer(BFRES.shader.getAttribute("_n0"), 3, VertexAttribPointerType.Float, false, BaseRenderData.Vertex.Stride, 12);
                GL.VertexAttribPointer(BFRES.shader.getAttribute("_u0"), 2, VertexAttribPointerType.Float, false, BaseRenderData.Vertex.Stride, 24);
                GL.VertexAttribPointer(BFRES.shader.getAttribute("_i0"), 4, VertexAttribPointerType.Float, false, BaseRenderData.Vertex.Stride, 32);
                GL.VertexAttribPointer(BFRES.shader.getAttribute("_w0"), 4, VertexAttribPointerType.Float, false, BaseRenderData.Vertex.Stride, 48);
                GL.BindBuffer(BufferTarget.ArrayBuffer, 0);


                // bind attributes
                //Console.WriteLine(shape.Text + " " + shape.singleBind);
                GL.Uniform1(BFRES.shader.getAttribute("single"), shape.singleBind);
                /*foreach (BFRESAttribute att in vert.attributes)
                {
                    int size = 0;
                    BFRESBuffer buffer = vert.buffers[att.bufferIndex];
                    float[] data = att.data.ToArray();
                    //Console.WriteLine(att.Text + " " + ((int)(att.format)).ToString("x"));
                    switch (att.Text)
                    {
                        case "_p0": GL.BindBuffer(BufferTarget.ArrayBuffer, vbo_po); size = 4; break;
                        case "_n0": GL.BindBuffer(BufferTarget.ArrayBuffer, vbo_n0); size = 4; break;
                        case "_i0":
                            GL.BindBuffer(BufferTarget.ArrayBuffer, vbo_i0);
                            if (att.format == 256) { size = 1; } else
                            if (att.format == 260) { size = 2; } else
                            //if (att.format == 261) { size = 4; } else
                            if (att.format == 266) { size = 4; } else
                            if (att.format == 268) { size = 2; }
                            //else if (att.format == 272) { size = 4; }
                            else { Console.WriteLine("Unused bone type "); }
                            GL.Uniform1(BFRES.shader.getAttribute("boneSize"), size);

                            for (int i = 0; i < att.data.Count; i++)
                            {
                                if (data[i] < skel.bindId.Count)
                                    data[i] = skel.bindId[(int)data[i]];
                            }
                            break;
                        case "_w0":
                            GL.BindBuffer(BufferTarget.ArrayBuffer, vbo_w0);
                            if (att.format == 4) { size = 2; }
                            if (att.format == 10) { size = 4; }
                            if (att.format == 2061) { size = 2; }
                            break;
                        default: continue;
                    }
                    //Console.WriteLine(buffer.stride);
                    GL.BufferData<float>(BufferTarget.ArrayBuffer, (IntPtr)(data.Length * sizeof(float)), data, BufferUsageHint.StaticDraw);
                    GL.VertexAttribPointer(BFRES.shader.getAttribute(att.Text), size, VertexAttribPointerType.Float, false, 
                        buffer.stride * BFRESAttribute.formatStrideMultiplyer[att.format], att.bufferOffset);// 
                }*/

                // draw models
                foreach (LODModel mod in shape.lodModels)
                {
                    GL.BindBuffer(BufferTarget.ElementArrayBuffer, ibo);

                    if (mod.type == DrawElementsType.UnsignedShort)
                    {
                        GL.BufferData(BufferTarget.ElementArrayBuffer, (IntPtr)(mod.data.Length * sizeof(ushort)), mod.data, BufferUsageHint.StaticDraw);
                        GL.DrawElements(PrimitiveType.Triangles, mod.fcount, mod.type, mod.skip * sizeof(ushort));
                    }
                    else
                    if (mod.type == DrawElementsType.UnsignedInt)
                    {
                        GL.BufferData(BufferTarget.ElementArrayBuffer, (IntPtr)(mod.data.Length * sizeof(uint)), mod.data, BufferUsageHint.StaticDraw);
                        GL.DrawElements(PrimitiveType.Triangles, mod.fcount, mod.type, mod.skip * sizeof(uint));
                    }

                    break; // only draw first one
                }
            }
            BFRES.shader.disableAttrib();

            GL.UseProgram(0);
            GL.Disable(EnableCap.DepthTest);
            skel.Render(v);
            GL.Enable(EnableCap.DepthTest);
        }
    }

    public class FSKL : Skeleton
    {
        public int[] bindId;

        public FSKL()
        {
            ImageKey = "skeleton";
            SelectedImageKey = "skeleton";
        }

        public void Read(FileData f)
        {
            Text = "Skeleton";
            if (!f.readString(4).Equals("FSKL"))
                throw new Exception("Error reading Skeleton");


            f.skip(8); // header length
            f.skip(4); // padding
            int boneIndexGroupOffset = f.readOffset();
            f.skip(4); // padding
            int boneArrayOffset = f.readOffset();
            f.skip(4); // padding
            int inverseIndexOffset = f.readOffset();
            f.skip(4); // padding
            int inverseMatrixOffset = f.readOffset();

            if (BFRES.verNumB == 8)
            {
                f.skip(28); // padding
            }
            else
            {
                f.skip(12); // padding
            }
            int flags = f.readInt();
            int bcount = f.readShort();
            int inverseIndexCount = f.readShort();
            int extraCount = f.readShort();
            f.skip(4); // padding 

            f.seek(inverseIndexOffset);
            bindId = new int[inverseIndexCount + extraCount];
            for (int i = 0; i < inverseIndexCount + extraCount; i++)
            {
                bindId[i] = (f.readShort());
                Console.WriteLine(bindId[i]);
            }

            // now read bones
            f.seek(boneArrayOffset);
            Bone[] bones = new Bone[bcount];
            for (int i = 0; i < bcount; i++)
            {
                Bone b = new Bone();
                b.Text = f.readString(f.readOffset() + 2, -1);
                if (BFRES.verNumB == 8)
                {
                    f.skip(36); // padding
                }
                else
                {
                    f.skip(20); // padding
                }
                bones[i] = b;
                b.id = f.readShort();
                b.p1 = (short)f.readShort();
                b.p2 = (short)f.readShort();
                b.p3 = (short)f.readShort();
                b.p4 = (short)f.readShort();
                f.skip(2); // padding 0
                f.skip(4); // TODO: flags

                b.sca = new Vector3(f.readFloat(),
                    f.readFloat(),
                    f.readFloat());
                b.rot = new Vector3(f.readFloat(),
                    f.readFloat(),
                    f.readFloat());
                f.skip(4); // for quat, with eul is 1.0
                b.pos = new Vector3(f.readFloat(),
                    f.readFloat(),
                    f.readFloat());
            }
            Nodes.AddRange(bones);
            Reset();
            //PrintDepth();
        }
    }

        #region Vertex Attributes



    public class BufferData
    {

        public int VertexBufferSizeData { get; set; }
        public int DataOffsetData { get; set; }
        public int VertexStrideSizeData { get; set; }
    }



    public class BufferSizes
    {

        public byte[] BuffSizes { get; set; }

    }

    public class FVTX : TreeNode
    {
        public List<BFRESBuffer> buffers = new List<BFRESBuffer>();
        public List<BFRESAttribute> attributes = new List<BFRESAttribute>();


        List<BufferData> BufferOffsets = new List<BufferData>();
        List<BufferSizes> BuffSizes = new List<BufferSizes>();

        public int vertCount;
        public int unk1;
        public int DataOffset;
        public int VertexBufferSize;
        public int vertexStrideSize;
        public int DataOff;
        public byte[] dataBuff;
        public int VertStart;

        // okay, so because converting between buffers can get complicated, Imma use my own system
        public int gl_vbo;


        public FVTX(FileData f)
        {
            Text = "VertexAttributeBuffer";
            if (!f.readString(4).Equals("FVTX"))
                throw new Exception("Error reading Skeleton");
            f.skip(12); //padding
            int vertexAttribArrayOffset = f.readOffset();
            f.skip(4); //padding
            int vertexAttribDictOffset = f.readOffset();
            f.skip(4); //padding
            int unk = f.readOffset();
            f.skip(4); //padding
            int unk2 = f.readOffset();
            f.skip(4); //padding
            int unk3 = f.readOffset();
            f.skip(4); //padding
            int vertexBufferSizeOffset = f.readOffset();
            f.skip(4); //padding
            int vertexStrideSizeOffset = f.readOffset();
            f.skip(4); //padding
            int vertexBufferArrayOffset = f.readOffset();
            f.skip(4); //padding
            int BufferOffset = f.readOffset();
            int attrCount = f.readByte();
            int bufferCount = f.readByte();
            int index = f.readShort();
            vertCount = f.readInt();
            int SkinWeightInfluence = f.readInt();
            int temp = f.pos();



            for (int i = 0; i < attrCount; i++)
            {
                f.seek(vertexAttribArrayOffset + (i * 0x10));
                attributes.Add(new BFRESAttribute(f, buffers));
            }


            //Find buffer size & stride

            RelocationTable RLT = new RelocationTable(f);

            for (int i = 0; i < bufferCount; i++)
            {
                f.seek(vertexBufferSizeOffset + ((i) * 0x10));
                VertexBufferSize = f.readInt();
                f.seek(vertexStrideSizeOffset + ((i) * 0x10));
                vertexStrideSize = f.readInt();

                //So these work by grabbing the RLT offset first and then adding the buffer offset. Then they keep adding each other by their buffer sizes
                if (i == 0)
                    DataOffset = (RLT.DataStart + BufferOffset);
                if (i > 0)
                    DataOffset = BufferOffsets[i - 1].DataOffsetData + BufferOffsets[i - 1].VertexBufferSizeData;
                if (DataOffset % 8 != 0)
                    DataOffset = DataOffset + (8 - (DataOffset % 8));

                BufferOffsets.Add(new BufferData { VertexBufferSizeData = VertexBufferSize, DataOffsetData = DataOffset, VertexStrideSizeData = vertexStrideSize });

                dataBuff = f.getSection(BufferOffsets[i].DataOffsetData, BufferOffsets[i].VertexBufferSizeData);
                Console.WriteLine("Data Offset = " + DataOffset + " Vertex Buffer Size =" + BufferOffsets[i].VertexBufferSizeData + " Index = " + i + " vertexStrideSize size =" + vertexStrideSize);


                //WriteByteArray(dataBuff, "dataBuff");

                BuffSizes.Add(new BufferSizes { BuffSizes = dataBuff });

            }



            Nodes.AddRange(attributes.ToArray());

            f.seek(BufferOffsets[0].DataOffsetData);
            myRender();

            f.seek(temp);
        }

        private class TempVertex
        {
            public float x = 0, y = 0, z = 0;
            public float nx = 0, ny = 0, nz = 0;
            public float r = 1, g = 1, b = 1, a = 1;
            public Vector2 uv0 = new Vector2();
            public float i1 = 0, i2 = 0, i3 = 0, i4 = 0; // can have 5
            public float w1 = 1, w2 = 1, w3 = 1, w4 = 1;
        }

        public List<BaseRenderData.Vertex> VertData = new List<BaseRenderData.Vertex>();
        public void myRender()
        {
           // List<Vertex> VertList = new List<Vertex>();

            GL.GenBuffers(1, out gl_vbo);
            for (int i = 0; i < vertCount; i++)
            {
                TempVertex vert = new TempVertex();

                try
                {

                    foreach (BFRESAttribute att in attributes)
                    {
                        DataOff = (BufferOffsets[att.bufferIndex].DataOffsetData) + (attributes[att.bufferIndex].bufferOffset);
                        FileData d = new FileData(new FileData(BuffSizes[att.bufferIndex].BuffSizes).getSection(0, -1));
                        d.Endian = Endianness.Little;
                        d.seek(att.bufferOffset + i * BufferOffsets[att.bufferIndex].VertexStrideSizeData);
                        switch (att.Text)
                        {
                            case "_p0":
                                if (att.format.Equals(0x518))
                                {
                                    vert.x = d.readFloat();
                                    vert.y = d.readFloat();
                                    vert.z = d.readFloat();
                                }
                                else if (att.format.Equals(0x515))
                                {
                                    vert.x = d.readHalfFloat();
                                    vert.y = d.readHalfFloat();
                                    vert.z = d.readHalfFloat();
                                    d.readHalfFloat(); //w
                                }
                                else
                                    Console.WriteLine("Unkown Format!! " + att.format.ToString());
                                break;
                            case "_n0":
                                if (att.format.Equals(0x20E))
                                {
                                    int normVal = (int)d.readInt();
                                    vert.nx = FileData.sign10Bit((normVal) & 0x3FF) / 511f;
                                    vert.ny = FileData.sign10Bit((normVal >> 10) & 0x3FF) / 511f;
                                    vert.nz = FileData.sign10Bit((normVal >> 20) & 0x3FF) / 511f;
                                    break;
                                }
                                else
                                    Console.WriteLine("Unkown Format!! " + att.format.ToString() + " " + att.Text);
                                break;
                            case "_i0":
                                if (att.format.Equals(0x20B))
                                {
                                    vert.i1 = d.readByte();
                                    vert.i2 = d.readByte();
                                    vert.i3 = d.readByte();
                                    vert.i4 = d.readByte();
                                }
                                else if (att.format.Equals(0x302))
                                {
                                    vert.i1 = d.readByte();
                                    vert.w1 = 1;
                                }
                                else if (att.format.Equals(0x309))
                                {
                                    vert.i1 = d.readByte();
                                    vert.i2 = d.readByte();
                                }
                                else if (att.format.Equals(0x30B))
                                {
                                    vert.i1 = d.readByte();
                                    vert.i2 = d.readByte();
                                    vert.i3 = d.readByte();
                                    vert.i4 = d.readByte();
                                }
                                else if (att.format.Equals(0x118))
                                {
                                    vert.i1 = d.readInt();
                                    vert.i2 = d.readInt();
                                    vert.i3 = d.readInt();
                                }
                                else if (att.format.Equals(0x117))
                                {
                                    vert.i1 = d.readInt();
                                    vert.i2 = d.readInt();
                                }
                                else if (att.format.Equals(0x115))
                                {
                                    vert.i1 = d.readShort();
                                    vert.i2 = d.readShort();
                                }
                                else
                                    Console.WriteLine("Unkown Format!! " + att.format.ToString() + " " + att.Text);
                                break;
                            case "_u0":
                                if (att.format.Equals(0x112))
                                {
                                    vert.uv0.X = ((float)d.readShort()) / 65535;
                                    vert.uv0.Y = ((float)d.readShort()) / 65535;
                                }
                                else if (att.format.Equals(0x109))
                                {
                                    vert.uv0.X = d.readByte() / (float)255;
                                    vert.uv0.Y = d.readByte() / (float)255;
                                }
                                else if (att.format.Equals(0x209))
                                {
                                    vert.uv0.X = d.readByte() / (float)127;
                                    vert.uv0.Y = d.readByte() / (float)127;
                                }
                                else if (att.format.Equals(0x212))
                                {
                                    vert.uv0.X = d.readShort() / (float)32767;
                                    vert.uv0.Y = d.readShort() / (float)32767;
                                }
                                else if (att.format.Equals(0x512))
                                {
                                    vert.uv0.X = d.readHalfFloat();
                                    vert.uv0.Y = d.readHalfFloat();
                                }
                                else if (att.format.Equals(0x517))
                                {
                                    vert.uv0.X = d.readFloat();
                                    vert.uv0.Y = d.readFloat();
                                }
                                else
                                    Console.WriteLine("Unkown Format!! " + att.format.ToString() + " " + att.Text);
                                break;
                            case "_w0":
                                if (att.format.Equals(0x102))
                                {
                                    vert.w1 = d.readByte();
                                }
                                else if (att.format.Equals(0x109))
                                {
                                    vert.w1 = d.readByte() / (float)255;
                                    vert.w2 = d.readByte() / (float)255;
                                }
                                else if (att.format.Equals(0x10B))
                                {
                                    vert.w1 = d.readByte() / (float)255;
                                    vert.w2 = d.readByte() / (float)255;
                                    vert.w3 = d.readByte() / (float)255;
                                    vert.w4 = d.readByte() / (float)255;
                                }
                                else if (att.format.Equals(0x112))
                                {
                                    vert.w1 = d.readShort() / (float)0xFFFF;
                                    vert.w2 = d.readShort() / (float)0xFFFF;
                                }
                                break;
                            default:
                                //d.skip(d.size());
                                // Console.WriteLine(Text + " Unknown type " + att.format.ToString("x") + " 0x");
                                break;
                        }
                    }

                    VertData.Add(new BaseRenderData.Vertex()
                    {
                        x = vert.x,
                        y = vert.y,
                        z = vert.z,
                        nx = vert.nx,
                        ny = vert.ny,
                        nz = vert.nz,
                        uv0 = vert.uv0,
                        i1 = vert.i1,
                        i2 = vert.i2,
                        i3 = vert.i3,
                        i4 = vert.i4,
                        w1 = vert.w1,
                        w2 = vert.w2,
                        w3 = vert.w3,
                        w4 = vert.w4,
                    });
                }
                catch(Exception ex)
                {
                    MessageBox.Show("A buffer broke :( \n\n" + ex, "Error");
                }
            }

            /*(Vertex item in data)
            {
                Console.WriteLine("X = " + item.x + " Y = " + item.y + " Z = " + item.z);
            }*/

           

            GL.BindBuffer(BufferTarget.ArrayBuffer, gl_vbo);
            GL.BufferData < BaseRenderData.Vertex >(BufferTarget.ArrayBuffer, (IntPtr)(VertData.Count * BaseRenderData.Vertex.Stride), VertData.ToArray(), BufferUsageHint.StaticDraw);
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);


            
        }
    }

  

    public class BFRESAttribute : TreeNode
    {
        public int bufferIndex, bufferOffset;
        public int format;

 

        public static Dictionary<int, int> formatStrideMultiplyer = new Dictionary<int, int>()
        {
            {4, 4 },
            {7, 2 },
            {10, 4 },
            {0x100, 4 },
            {0x104, 4 },
            {0x10A, 4 },
            {0x10C, 1 },
            {2061, 1 },
            {2063, 2 },
            {2065, 1 },
            {2067, 1 },
            {0x20A, 4 },
            {0x20B, 4 },
            {0x30B, 4 },
        };

        public BFRESAttribute(FileData f, List<BFRESBuffer> buffers)
        {
            Text = f.readString(f.readOffset() + 2, -1);
            f.skip(4); //padding
            f.Endian = Endianness.Big;
            format = f.readShort();
            f.skip(2); //paddingpad
            f.Endian = Endianness.Little;
            bufferOffset = f.readShort();
            bufferIndex = f.readShort();


            Nodes.Add("Format = " + format.ToString("x"));



            //Text += " 0x" + format.ToString("x");

            //Console.WriteLine(Text + " " + bufferOffset.ToString("x") + " " + format + " " + buffers[bufferIndex].dataOffset.ToString("x") + " " + buffers[bufferIndex].stride);
            Console.WriteLine(Text + " type " + format.ToString("x") + " Buffer Index = " + bufferIndex + " Buffr Offset = " + bufferOffset);
            /*FileData d = new FileData(new FileData(buffers[bufferIndex].data).getSection(bufferOffset, -1));
            while (d.pos() < d.size())
            {
                switch (format)
                {
                    case 0x004:
                        data.Add(d.readByte() / 255f);
                        break;
                    case 0x007: data.Add(d.readShort() / 0xFFFF); break;
                    case 0x00A: data.Add(d.readByte() / 255f); break;
                    case 0x100: data.Add(d.readByte()); break;
                    case 0x104: data.Add(d.readByte()); break;
                    case 0x10A: data.Add(d.readByte()); break;
                    case 0x10C: data.Add(d.readFloat()); break;
                    case 0x20A: data.Add(((sbyte)d.readByte()) / 128); break;
                    case 0x20B:
                        uint normVal = (uint)d.readInt();
                        data.Add(((normVal & 0x3FC00000) >> 22) / 511f);
                        data.Add(((normVal & 0x000FF000) >> 12) / 511f);
                        data.Add(((normVal & 0x000003FC) >> 2) / 511f);
                        data.Add(1);
                        break;
                    case 0x80D: data.Add(d.readFloat()); break;
                    case 0x80F: data.Add(d.readHalfFloat()); break;
                    case 0x811: data.Add(d.readFloat()); break;
                    case 0x813: data.Add(d.readFloat()); break;
                    default:
                        d.skip(d.size());
                        Console.WriteLine(Text + " Unknown type " + format.ToString("x") + " 0x" + (bufferOffset + buffers[bufferIndex].dataOffset).ToString("x"));
                        break;
                }
            }*/
        }
    }

    public class BFRESBuffer : TreeNode
    {
        public int size, stride;
        public int dataOffset;
        public byte[] data;

        public int gl_vbo;

    
    }

    #endregion

    public class FSHP : TreeNode
    {
        public int fvtxindex;
        public int fvtxOffset;
        public int fmatIndex;
        public List<LODModel> lodModels = new List<LODModel>();
        public int singleBind;

        public FSHP(FileData f)
        {
            ImageKey = "polygon";
            SelectedImageKey = "polygon";

            if (!f.readString(4).Equals("FSHP"))
                throw new Exception("Error reading Mesh Shape");
            f.skip(12); // padding
            Text = f.readString(f.readOffset() + 2, -1);
            f.skip(4); // padding
            fvtxOffset = f.readOffset();
            f.skip(4); // padding
            int lodOffset = f.readOffset();
            f.skip(4); // padding
            int fsklIndexArrayOffset = f.readOffset();
            f.skip(20); // padding
            int boundingBoxOffset = f.readOffset();
            f.skip(4); // padding
            int BoundingRadiusOffset = f.readOffset();
            f.skip(12); // padding
            int flags = f.readInt();
            int index = f.readShort();
            fmatIndex = f.readShort();
            singleBind = (short)f.readShort();
            fvtxindex = f.readShort();
            int SkinBoneIndexCount = f.readShort();
            int VertexSkinCount = f.readByte();
            int lodCount = f.readByte();
            int visGroupCount = f.readInt();
            int fsklarraycount = f.readShort();

           

            // level of detail models
            //IndexGroup fmdlGroup = new IndexGroup(f);
            for (int i = 0; i < lodCount; i++)
            {
                f.seek(lodOffset + (i * 56));
                var baseShape = new BaseRenderData.shape()
                {
                    name = Text,
                    Index = index,
                    FvtxIndex = fvtxindex,
                    FmatIndex = fmatIndex,
                };

                lodModels.Add(new LODModel(f, baseShape));
            }
            Nodes.AddRange(lodModels.ToArray());

            // visibility group

        }

    }

    public class LODModel : TreeNode
    {
        public int faceType, indexBufferOffset, skip;
        public List<int> faces = new List<int>();
        public DrawElementsType type = DrawElementsType.UnsignedShort;
        public List<BaseRenderData.shape> ShapeData = new List<BaseRenderData.shape>();
        public ushort[] data;
        public uint[] dataui;
        public int fcount;

        public LODModel(FileData f, BaseRenderData.shape baseShape)
        {
            Text = "DetailLevel";
            int subMeshArrayOffset = f.readOffset();
            f.skip(4); // padding
            int unk = f.readOffset();
            f.skip(4); // padding
            int unk2 = f.readOffset();
            f.skip(4); // padding
            indexBufferOffset = f.readOffset();
            f.skip(4); // padding
            int faceOffset = f.readOffset(); // is indexes
            int PrimativeFormat = f.readInt();
            faceType = f.readInt();
            fcount = f.readInt();
            int visgroup = f.readShort();
            int subMeshCount = f.readShort();

            int temp = f.pos();

            RelocationTable RLT = new RelocationTable(f);

            indexBufferOffset = RLT.DataStart + faceOffset;

            f.seek(indexBufferOffset);
            data = new ushort[fcount];
            dataui = new uint[fcount];

            for (int i = 0; i < fcount; i++)
            {
                if (faceType == 1)
                {
                    data[i] = (ushort)f.readShort();
                    //  Console.WriteLine(data[i]);

                    //Add to main renderer.
                    ShapeData.Add(new BaseRenderData.shape()
                    {
                        face = data[i],
                        FmatIndex = baseShape.FmatIndex,
                        name = baseShape.name,
                        Index = baseShape.Index,
                        FvtxIndex = baseShape.FvtxIndex,
                    });
                }
                 
   
                else
                if (faceType == 2)
                {
                    dataui[i] = (uint)f.readInt();
                    type = DrawElementsType.UnsignedInt;
                }
                else
                    throw new Exception("Unknown face types " + faceType);
            }

            f.seek(temp);
        }
    }


    public class FMAT : TreeNode
    {
        public int sectionindex;
        public List<TextureSelecter> tex = new List<TextureSelecter>();


        public FMAT(FileData f)
        {
            ImageKey = "material";
            SelectedImageKey = "material";

            if (!f.readString(4).Equals("FMAT"))
                throw new Exception("Error reading Material");

            f.skip(8); //Header Length
            f.skip(4); //padding
            Text = f.readString(f.readOffset() + 2, -1);
            f.skip(4); //padding
            int renderInfoOffset = f.readOffset();
            f.skip(4); //padding
            int RenderInfoDictOffset = f.readOffset();
            f.skip(4); //padding
            int shaderAssignOffset = f.readOffset();
            f.skip(4); //padding
            int Unk1Off = f.readOffset();
            f.skip(4); //padding
            int textureRefArrayOffset = f.readOffset();
            f.skip(4); //padding
            int Unk2Off = f.readOffset();
            f.skip(4); //padding
            int SamplerListOffset = f.readOffset();
            f.skip(4); //padding
            int samplerDictOffset = f.readOffset();
            f.skip(4); //padding
            int shaderParamArrayOffset = f.readOffset();
            f.skip(4); //padding
            int shaderParamDictOffset = f.readOffset();
            f.skip(4); //padding
            int sourceParamDataOffset = f.readOffset();
            f.skip(4); //padding
            int UserDataOffset = f.readOffset();
            f.skip(4); //padding
            int UserDataDict = f.readOffset();
            f.skip(4); //padding
            int VolatileFlagOffset = f.readOffset();
            f.skip(4); //padding
            int UserOffset = f.readOffset();
            f.skip(4); //padding
            int SamplerSlotOffset = f.readOffset();
            f.skip(4); //padding
            int TextureSlotOffset = f.readOffset();
            f.skip(4); //padding
            int MaterialFlags = f.readInt();
            sectionindex = f.readShort();
            int renderInfoCount = f.readShort();
            int textureRefCount = f.readByte();
            int SamplerCount = f.readByte();
            int shaderParamVolatileCount = f.readShort();
            int sourceParamDataSize = f.readShort();
            int rawParamDataSize = f.readShort();
            int UserDataCount = f.readShort();
            f.skip(4); //padding


            f.seek(textureRefArrayOffset);
            for(int i = 0; i < textureRefCount; i++)
            {
                tex.Add(new TextureSelecter(f));
            }
        }
    }

    public class TextureSelecter : TreeNode
    {

        public TextureSelecter(FileData f)
        {
            Text = f.readString(f.readOffset() + 2, -1);
            f.skip(4); //padding

            
        }

    }

}
