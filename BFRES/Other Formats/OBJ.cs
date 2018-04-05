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

        public static void Export( string FileName, int modelIndex)
        {
            //Get shape and vertex data

            Console.WriteLine("FMDL Index Selected = " + modelIndex);

            BaseRenderData model = new BaseRenderData();

            using (System.IO.StreamWriter f = new System.IO.StreamWriter(FileName))
            {

                Console.WriteLine("Polygon List Count = " + model.data.Count);

                foreach (var shp in model.PolygonO)
                {
                    f.Write(shp.name);
                    f.Write(shp.face);
                    f.Write("TEST");
                }
       
                if (model.PolygonO.Count == 0)
                {
                    MessageBox.Show("Object Empty???", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);

                }
            }
        }
    }
}
