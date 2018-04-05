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
    public class ExternalFiles : TreeNode
    {
        List<BRTI> textures = new List<BRTI>();

        List<BNTX> BNTX = new List<BNTX>();
        List<BFSHA> BFSHA = new List<BFSHA>();
        string EmMagic;

        public static int DataOffset;


        public ExternalFiles(FileData f)
        {
            ImageKey = "external file";
            SelectedImageKey = "external file";

            DataOffset = f.readOffset();
            f.skip(4); //padding
            int Size = f.readInt();
            f.skip(4); //padding

            Console.WriteLine("DataOffset " + DataOffset);


            f.seek(DataOffset);
            EmMagic = f.readString(f.pos(), 4);


            if (EmMagic.Equals("BNTX")) //Textures
            {
                BNTX.Add(new BNTX(f));
                Console.WriteLine("Found BNTX");
            }
            if (EmMagic.Equals("FSHA")) //Shader
            {
                BFSHA.Add(new BFSHA(f));
                Console.WriteLine("Found Shader");
            }

            Nodes.AddRange(BNTX.ToArray());
            Nodes.AddRange(BFSHA.ToArray());
        }

    }
}
