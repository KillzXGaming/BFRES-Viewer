using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BFRES
{
    public class IndexGroup
    {
        public List<string> names = new List<string>();
        public List<int> dataOffsets = new List<int>();
        
        public List<TreeNode> nodes = new List<TreeNode>();

        public IndexGroup()
        {

        }

        public class BSTNode
        {
            public string name = "";
            public BSTNode left = null, right = null;
            public TreeNode node;

            public void insert(TreeNode n)
            {
            }
        }

        public void Save(FileOutput o, FileOutput h, FileOutput s, FileOutput d)
        {
            o.writeInt(8 + 16 * (nodes.Count + 1));
            o.writeInt(nodes.Count);

            BSTNode root = new BSTNode();
            foreach (TreeNode n in nodes)
            {

            }

            // create root node
            o.writeShort(0xFFFF);o.writeShort(0xFFFF);
            o.writeShort(0x01);
            o.writeShort(0x00);
            o.writeInt(0);
            o.writeInt(0);

            // make this into binary search tree
            foreach (TreeNode n in nodes)
            {
                o.writeInt(0); // search value
                o.writeShort(0x00);
                o.writeShort(0x01);
                o.writeOffset(s.getStringOffset(n.Text), s);
                o.writeOffset(h.size(), h);
            }
        }

        public IndexGroup(FileData f)
        {
            int length = f.readInt();
            int count = f.readInt();
            f.skip(16); // skip root node
            //Console.WriteLine("Index Group--------------------------------");
            //Console.Write(-1 + " " + Convert.ToString(f.readInt(),2) + " " + f.readShort() + " " + f.readShort());
            //f.skip(8);
            for (int i = 0; i < count; i++)
            {
                //Console.Write(i + " " +Convert.ToString(f.readInt(), 2) + " " + f.readShort() + " " + f.readShort());
                f.skip(4); // search value
                f.skip(4); // left and right index
                int noff = f.readInt();
                string name = "";
                if (noff > 0)
                    name = f.readString(noff + 2, -1);
                int dataOffset = f.readOffset();
                names.Add(name);
                dataOffsets.Add(dataOffset);
                //Console.Write(" " + name + "\n");
            }
        }
    }
}
