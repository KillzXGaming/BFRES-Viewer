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
    public class RelocationTable
    {
        public int DataStart;

        public RelocationTable(FileData f)
        {
            //Read relocation table first so we can read the buffer!
            f.seek(0x18);
            int RTLOffset = f.readOffset();

            f.seek(RTLOffset);
            f.skip(0x030);
            DataStart = f.readOffset();
        }

    }
}
