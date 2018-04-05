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
    class FMAA: TreeNode
    {
        public FMAA(FileData f)
        {
            ImageKey = "material animation";
            SelectedImageKey = "material animation";

            f.skip(4); // MAGIC
            f.skip(12); // padding
            Text = f.readString(f.readOffset() + 2, -1);

            /*
            This section stores a few things. 
           - Texture Patern Animations
           - SRT Animations
           - Color Animations
           - Shader Parameter Animations
           - Bone Visabilty Animations
             */





        }

    }
}
