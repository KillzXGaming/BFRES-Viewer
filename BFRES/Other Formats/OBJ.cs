using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Windows.Forms;
using OpenTK;

namespace BFRES
{
    public class OBJ
    {
        public static void Import(string FileName)
        {

        }

        public static void Export(string FileName, BaseRenderData model)
        {
            List<Vector3h> VerticesN = new List<Vector3h>(); //a lot of normals are often shared
            
            List<string> ExportTextures = new List<string>();
            using (System.IO.StreamWriter f = new System.IO.StreamWriter(FileName))
            {
                f.WriteLine($"mtllib {Path.GetFileNameWithoutExtension(FileName)}.mtl");

                List<string> vn = new List<string>();
                foreach (var v in model.data)
                {
                    f.WriteLine($"v {v.x} {v.y} {v.z}");
                    f.WriteLine($"vt {v.uv0.X } {1 - v.uv0.Y}");
                    
                    if (!VerticesN.Contains(v.NormalVec))
                    {
                        f.WriteLine($"vn {v.nx} {v.ny} {v.nz}");
                        VerticesN.Add(v.NormalVec);
                    }
                }

                var d = model.PolygonO;
                int currentMath = d[0].FmatIndex;
                f.WriteLine($"usemtl {model.mats[currentMath].tex[0].Text}");
                ExportTextures.Add(model.mats[currentMath].tex[0].Text);
                for (int i = 0; i < d.Count; i++)
                {
                    if (currentMath != d[i].FmatIndex)
                    {
                        currentMath = d[i].FmatIndex;
                        f.WriteLine($"usemtl {model.mats[currentMath].tex[0].Text}");
                        if (!ExportTextures.Contains(model.mats[currentMath].tex[0].Text)) ExportTextures.Add(model.mats[currentMath].tex[0].Text);
                    }

                    int[] verts = new int[3] { (int)d[i++].face, (int)d[i++].face, (int)d[i].face };
                    int[] normals = new int[3] {
                        VerticesN.IndexOf(model.data[verts[0]].NormalVec),
                        VerticesN.IndexOf(model.data[verts[1]].NormalVec),
                        VerticesN.IndexOf(model.data[verts[2]].NormalVec)
                    };

                    f.WriteLine($"f {verts[0] + 1}/{verts[0] + 1}/{normals[0] + 1} {verts[1] + 1}/{verts[1] + 1}/{normals[1] + 1} {verts[2] + 1}/{verts[2] + 1}/{normals[2] + 1}");
                }
            }

            string textureFolder = Path.GetFileNameWithoutExtension(FileName) + "_tex";
            using (System.IO.StreamWriter f = new System.IO.StreamWriter(FileName.Substring(0,FileName.Length - 3) + "mtl"))
            {
                foreach (string MatName in ExportTextures)
                {
                    f.WriteLine($"newmtl {MatName}");
                    f.WriteLine($"Ka 0.000000 0.000000 0.000000");
                    f.WriteLine($"Kd 1.000000 1.000000 1.000000");
                    f.WriteLine($"Ks 0.330000 0.330000 0.330000");
                    if (TextureListContains(model.textures,MatName)) f.WriteLine($"map_Kd {textureFolder}/{MatName}.bmp\n");
                }               
            }

            if (model.textures.Count > 0) Directory.CreateDirectory($"{Path.GetDirectoryName(FileName)}/{textureFolder}");
            foreach (var tex in model.textures)
            {
                if (!ExportTextures.Contains(tex.Text)) continue;
                tex.tex.ExportAsImage($"{Path.GetDirectoryName(FileName)}/{textureFolder}/{tex.Text}.bmp");
            }
        }

        static bool TextureListContains(List<BRTI> l, string name)
        {
            foreach (var tex in l) if (tex.Text == name) return true;
            return false;
        }
    }
}
