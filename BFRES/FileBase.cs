using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using OpenTK;

namespace BFRES
{
    class FileBase : TreeNode
    {
        public FileData data;


        public void Save(string fname)
        {
            File.WriteAllBytes(fname, data.b);
        }

        public static TreeNode ReadFileBase(FileData f)
        {
            if (f.fname.EndsWith(".vert"))
            {
                return new IVTX(f);
            }
            if (f.Magic.Equals("SARC"))
            {
                return new SARC(f);
            }
            if (f.Magic.Equals("Yaz0"))
            {
                return ReadFileBase(Decompress.YAZ0(f));
            }
            if (f.Magic.Equals("FRES"))
            {
                return new BFRES(f) { data = f };
            }
            if (f.Magic.Equals("BNTX"))
            {
                return new BNTXData(f) { data = f };
            }
            return new FileBase() { Text = f.fname, data = f };
        }

    }
}
