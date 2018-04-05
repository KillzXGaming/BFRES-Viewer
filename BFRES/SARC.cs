using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BFRES
{
    class SARC : FileBase
    {
        public SARC(FileData f)
        {
            ImageKey = "sarc";
            SelectedImageKey = "sarc";
            Text = f.fname;
            Console.WriteLine(f.fname);
            data = f;
            Read(f);
        }

        public void Read(FileData f)
        {
            f.skip(4); // magic check
            f.skip(2); // headerlength
            if (f.readShort() == 0xFEFF)
                f.Endian = Endianness.Big;
            else f.Endian = Endianness.Little;

            f.skip(4); // filesize
            int dataOffset = f.readInt();
            f.skip(4); // always 0x01000000

            // SFAT Header
            f.skip(4); // SFAT
            f.skip(2); // header size
            int nodeCount = f.readShort();
            f.skip(4); // hash multiplyer always 0x65

            // before nodes get strings
            int stringoff = f.pos() + 16 * nodeCount + 8;

            // nodes
            for (int i = 0; i < nodeCount; i++)
            {
                uint hash = (uint)f.readInt();
                byte flag = (byte)f.readByte();
                string name = f.readString(stringoff + f.readThree()*4, -1);
                int nodeStart = f.readInt();
                int size = f.readInt() - nodeStart;

                Nodes.Add(FileBase.ReadFileBase(new FileData(f.getSection(nodeStart + dataOffset, size), name)));
            }
        }

        public void Save()
        {

        }

    }
}
