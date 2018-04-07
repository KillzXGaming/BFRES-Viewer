using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Windows.Forms;

namespace BFRES
{
    public class OBJ
    {
        public static void Import(string FileName)
        {

        }

        public static void Export( string FileName, BaseRenderData model)
        {
            //Get shape and vertex data            
            using (System.IO.StreamWriter f = new System.IO.StreamWriter(FileName))
            {

                foreach (var v in model.data)
                {
                    f.WriteLine($"v {v.x} {v.y} {v.z}");
                }

                var d = model.PolygonO;
                for (int i = 0; i < d.Count; i++)
                {
                    f.WriteLine($"f {d[i++].face + 1} {d[i++].face + 1} {d[i].face + 1}");
                }


            }
        }
    }
}
