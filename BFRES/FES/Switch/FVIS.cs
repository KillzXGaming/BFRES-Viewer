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
    class FVIS : TreeNode
    {
        public FVIS(FileData f)
        {
            ImageKey = "visual animation";
            SelectedImageKey = "visual animation";

            f.skip(4); // MAGIC FBVS
            f.skip(12); // unk
            Text = f.readString(f.readOffset() + 2, -1);


        }

    }
}
