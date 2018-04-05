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
    class FSHU : TreeNode
    {
        public FSHU(FileData f)
        {
            ImageKey = "shape animation";
            SelectedImageKey = "shape animation";

            f.skip(4); // MAGIC
            f.skip(12); // padding
            Text = f.readString(f.readOffset() + 2, -1);


        }

    }
}
