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
    public class BFSHA : TreeNode
    {
        List<ShaderModel> ShaderMdl = new List<ShaderModel>();

        public int StaticOptionCount
        {
            get;
            set;
        }


        public BFSHA(FileData f)
        {
            int temp = f.pos();
            f.skip(8); //Magic
            int Version = f.readInt();
            int ByteOrderMark = f.readShort();
            int HeaderSize = f.readShort();
            int NameOffset = f.readInt();
            int PathOffset = f.readInt();
            int RelocationTableOffset = f.readInt();
            int FileSize = f.readInt();
            f.skip(16);
            int ShaderModel = f.readInt();
            f.skip(20);
            int ModelArrayOffset = f.readInt();
            f.skip(4); //padding
            int ModelIndexArray = f.readInt();
            f.skip(28);
            StaticOptionCount = f.readShort();

            f.seek(ModelArrayOffset + temp);
            ShaderMdl.Add(new ShaderModel(f));


            Nodes.AddRange(ShaderMdl.ToArray());
        }
    }
    public class ShaderModel : TreeNode
    {
        

        public ShaderModel(FileData f)
        {
            ImageKey = "shader model";
            Text = f.readString(f.readOffset() + ExternalFiles.DataOffset + 2, -1);
            f.skip(4); //padding
            int StaticShaderOptionsOffset = f.readInt();
            f.skip(156);
            int StaticShaderOptionsCount = f.readShort();

            f.seek(StaticShaderOptionsOffset + ExternalFiles.DataOffset);
            for (int i = 1; i <= StaticShaderOptionsCount; i++)
            {
                new StaticShaderOptions(f);
            }
                
        }

        static List<String> matnames = new List<string>();

        class StaticShaderOptions : FileBase
        {
            public string StaticOptionName
            {
                get;
                set;
            }
            public int StaticOptionIndex
            {
                get;
                set;
            }
            public string ChoiceValue
            {
                get;
                set;
            }
            public string DefaultChoice
            {
                get;
                set;
            }
            public StaticShaderOptions(FileData f)
            {


                StaticOptionName = f.readString(f.readOffset() + ExternalFiles.DataOffset + 2, -1);
                f.skip(36);

                Console.WriteLine(StaticOptionName);

                matnames.Add(StaticOptionName);

                ShaderSettingsWindow table = new ShaderSettingsWindow(matnames.ToArray());


            }
        }
    }
 
 
}