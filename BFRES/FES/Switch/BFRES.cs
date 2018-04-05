using System;
using System.IO;
using System.Drawing;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using OpenTK;
using OpenTK.Graphics.OpenGL;


namespace BFRES
{
    class BFRES : FileBase
    {
        public string ModelName = "";

        public static string vs = @"
#version 330

in vec3 _p0;
in vec3 _n0;
in vec2 _u0;
in vec2 _u1;
in vec4 _c1;
in vec4 _i0;
in vec4 _w0;


out vec4 n;
out vec2 u0;
out vec2 u1;
out vec4 c0;


uniform mat4 modelview;
uniform mat4[100] bones;
uniform int[100] bonematch;
uniform int single;

vec4 skin()
{
    ivec4 b = ivec4(_i0);
    vec4 pos = vec4(_p0, 1.0);

    vec4 trans = bones[bonematch[b.x]] * pos * _w0.x;
    trans += bones[bonematch[b.y]] * pos *_w0.y;
    trans += bones[bonematch[b.z]] * pos *_w0.z;
    if(_w0.w < 1)
        trans += bones[bonematch[b.w]] * pos *_w0.w;
        
    return trans;
}

void main()
{
    n = vec4(_n0.xyz, 1);
    u0 = _u0;
    u1 = _u1;

    gl_Position = modelview * skin();
}";

        public static string fs = @"
#version 330

in vec4 n;
in vec2 u0;
in vec2 u1;



uniform sampler2D tex;

void fresnel()
{
    
}

void main()
{
gl_FragData[0] = texture2D(tex, u0);
}";


        public static Shader shader = null;

        public BFRES(FileData f)
        {
            ImageKey = "fres";
            SelectedImageKey = "fres";
            if(shader == null)
            {
                shader = new Shader();
                shader.vertexShader(vs);
                shader.fragmentShader(fs);
                shader.addAttribute("_p0", false);
                shader.addAttribute("_n0", false);
                shader.addAttribute("_u0", false);
                shader.addAttribute("_u1", false);
                shader.addAttribute("_u2", false);
                shader.addAttribute("_t0", false);
                shader.addAttribute("_b0", false);
                shader.addAttribute("_w0", false);
                shader.addAttribute("_i0", false); 
                shader.addAttribute("modelview", true);
                shader.addAttribute("bones", true);
                shader.addAttribute("bonematch", true);
                shader.addAttribute("single", true);
                shader.addAttribute("tex", true);
            }
            Text = f.fname;
            Tag = f;
            Read(f);
        }

        public static int verNumA, verNumB, verNumC, verNumD, EMBDict;

        // note: offsets are self relative
        public void Read(FileData f)
        {
            data = f;
            f.seek(4); // magic check
            f.skip(4); // Padding
            f.Endian = Endianness.Little;
            verNumD = f.readByte();
            verNumC = f.readByte();
            verNumB = f.readByte();
            verNumA = f.readByte();
            f.Endian = Endianness.Big;

            Console.WriteLine("Version = " + verNumA + "." + verNumB + "." + verNumC + "." + verNumD);

            if (f.readShort() == 0xFEFF)
                f.Endian = Endianness.Big;
            else f.Endian = Endianness.Little;
            f.skip(2); // sizHeader
            f.skip(4); // FileNameOffsetToString
                f.skip(4); // file alignment usuallt 0x00002000
            int RelocationTableOffset = f.readOffset();
            int BfresSize = f.readOffset();

            /*Note, alignment is for gpu addresses so not important for this*/

            Text = f.readString(f.readOffset() + 2, -1);
            // 
            Console.WriteLine("Reading " + ModelName);

            f.skip(4); // Padding
            int FMDLOffset = f.readOffset();
            f.skip(4); // Padding
            int FMDLDict = f.readOffset();
            f.skip(4); // Padding
            int FSKAOffset = f.readOffset();
            f.skip(4); // Padding
            int FSKADict = f.readOffset();
            f.skip(4); // Padding
            int FMAAOffset = f.readOffset();
            f.skip(4); // Padding
            int FMAADict = f.readOffset();
            f.skip(4); // Padding
            int FVISOffset = f.readOffset();
            f.skip(4); // Padding
            int FVISDict = f.readOffset();
            f.skip(4); // Padding
            int FSHUOffset = f.readOffset();
            f.skip(4); // Padding
            int FSHUDict = f.readOffset();
            f.skip(4); // Padding
            int FSCNOffset = f.readOffset();
            f.skip(4); // Padding
            int FSCNDict = f.readOffset();
            f.skip(4); // Padding
            int BuffMemPool = f.readOffset();
            f.skip(4); // Padding
            int BuffMemPoolInfo = f.readOffset();
            f.skip(4); // Padding
            int EMBOffset = f.readOffset();
            f.skip(4); // Padding
            EMBDict = f.readOffset();
            f.skip(12); // Padding
            int StringTableOffset = f.readOffset();
            f.skip(4); // Padding
            int unk11 = f.readOffset();
            int FMDLCount = f.readShort();
            int FSKACount = f.readShort();
            int FMAACount = f.readShort();
            int FVISCount = f.readShort();
            int FSHUCount = f.readShort();
            int FSCNCount = f.readShort();
            int EMBCount = f.readShort();
            f.skip(12); // Padding
            Console.WriteLine("EMBDict" + BFRES.EMBDict);

            // INDEX GROUPS

            //This is pretty messy atm. Makes sure offsets don't = 0. 

            TreeNode modelGroup = new TreeNode();
            modelGroup.Text = "Models";
            modelGroup.ImageKey = "folder";
            modelGroup.SelectedImageKey = "folder";
            Nodes.Add(modelGroup);
            if (FMDLOffset != 0)
            {
                f.seek(FMDLDict);
                IndexGroup fmdlGroup = new IndexGroup(f);

                for (int i = 0; i < FMDLCount; i++)
                {
                    f.seek(FMDLOffset + (i * 120));
                    modelGroup.Nodes.Add(new FMDL(f));
                }
            }

            TreeNode animGroup = new TreeNode();
            animGroup.Text = "Skeleton Animations";
            animGroup.ImageKey = "folder";
            animGroup.SelectedImageKey = "folder";
            Nodes.Add(animGroup);
            if (FSKAOffset != 0)
            {
                f.seek(FSKADict);
                IndexGroup fskaGroup = new IndexGroup(f);
                for (int i = 0; i < FSKACount; i++)
                {
                    f.seek(FSKAOffset + (i * 96));
                    animGroup.Nodes.Add(new FSKA(f));
                }
            }

            TreeNode MAAGroup = new TreeNode();
            MAAGroup.Text = "Material Animations";
            MAAGroup.ImageKey = "folder";
            MAAGroup.SelectedImageKey = "folder";
            Nodes.Add(MAAGroup);
            if (FMAAOffset != 0)
            {
                f.seek(FMAAOffset);
                IndexGroup FMAAGroup = new IndexGroup(f);

                for (int i = 0; i < FMAACount; i++)
                {
                    f.seek(FMAAOffset + (i * 120));
                    MAAGroup.Nodes.Add(new FMAA(f));
                }

            }
            TreeNode VISGroup = new TreeNode();
            VISGroup.Text = "Visual Animations";
            VISGroup.ImageKey = "folder";
            VISGroup.SelectedImageKey = "folder";
            Nodes.Add(VISGroup);
            if (FVISOffset != 0)
            {
                f.seek(FVISDict);
                IndexGroup FVISGroup = new IndexGroup(f);
                for (int i = 0; i < FVISCount; i++)
                {
                    f.seek(FVISOffset + (i * 104));
                    VISGroup.Nodes.Add(new FVIS(f));
                }
            }
            TreeNode SHUGroup = new TreeNode();
            SHUGroup.Text = "Shape Animations";
            SHUGroup.ImageKey = "folder";
            SHUGroup.SelectedImageKey = "folder";
            Nodes.Add(SHUGroup);
            if (FSHUOffset != 0)
            {
                f.seek(FSHUDict);
                IndexGroup FSHUGroup = new IndexGroup(f);

                for (int i = 0; i < FSHUCount; i++)
                {
                    f.seek(FMAAOffset + (i * 104));
                    SHUGroup.Nodes.Add(new FSHU(f));
                }
            }
            TreeNode SCNGroup = new TreeNode();
            SCNGroup.Text = "Scene Animations";
            SCNGroup.ImageKey = "folder";
            SCNGroup.SelectedImageKey = "folder";
            Nodes.Add(SCNGroup);
            if (FSCNOffset != 0)
            {
                f.seek(FSCNDict);
                IndexGroup FSCNGroup = new IndexGroup(f);

                for (int i = 0; i < FSCNCount; i++)
                {
                    f.seek(FSCNOffset + (i * 120));
                    SCNGroup.Nodes.Add(new FSCN(f));
                }
            }
            TreeNode embGroup = new TreeNode();
            embGroup.Text = "Embedded Files";
            embGroup.ImageKey = "folder";
            embGroup.SelectedImageKey = "folder";
            Nodes.Add(embGroup);
            if (EMBOffset != 0)
            {
                f.seek(EMBDict);
                IndexGroup fembGroup = new IndexGroup(f);
                for (int i = 0; i < EMBCount; i++)
                {

                    f.seek(EMBOffset + (i * 16));
                   embGroup.Nodes.Add(new ExternalFiles(f) { Text = fembGroup.names[i] });
                }
            }
        }

        public void Rebuild(string fname)
        {
            FileOutput o = new FileOutput();
            FileOutput h = new FileOutput();
            FileOutput d = new FileOutput();
            FileOutput s = new FileOutput();

            // bfres header
            o.writeString("FRES");
            o.writeByte(verNumA);
            o.writeByte(verNumB);
            o.writeByte(verNumC);
            o.writeByte(verNumD);
            o.writeShort(0xFEFF); // endianness
            o.writeShort(0x0010);// version number? 0x0010
            int fileSize = o.size();
            o.writeInt(0);// file length
            o.writeInt(0x00002000);// file alignment usuallt 0x00002000
            o.writeOffset(s.getStringOffset(Text), s);

            int stringTableSize = o.size();
            o.writeInt(0);
            int stringTableOffset = o.size();
            o.writeInt(0);
            
            o.writeInt(0);o.writeInt(0);o.writeInt(0);o.writeInt(0);
            o.writeInt(0);o.writeInt(0);o.writeInt(0);o.writeInt(0);
            o.writeInt(0);o.writeInt(0);o.writeInt(0); o.writeInt(0);

            o.writeShort(0); o.writeShort(0); o.writeShort(0); o.writeShort(0);
            o.writeShort(0); o.writeShort(0); o.writeShort(0); o.writeShort(0);
            o.writeShort(0); o.writeShort(0); o.writeShort(0); o.writeShort(0);

            foreach (TreeNode n in Nodes)
            {
                if(n.Text.Equals("FMDLs"))
                {
                    o.writeIntAt(o.size(), 0x20);
                    o.writeShortAt(n.Nodes.Count, 0x50);

                    IndexGroup group = new IndexGroup();
                    // create an index group and save it  
                    foreach(FMDL mdl in n.Nodes)
                    {
                        group.nodes.Add(mdl);
                    }
                    group.Save(o, h, s, d);
                }
            }

            o.writeOutput(h);
            o.writeIntAt(o.size(), stringTableOffset);
            o.writeIntAt(s.size(), stringTableSize);
            o.writeOutput(s);
            o.writeOutput(d);
            o.writeIntAt(o.size(), fileSize);
            o.save(fname);
        }
        
    }
}
