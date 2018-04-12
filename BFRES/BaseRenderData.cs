using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;

namespace BFRES
{
    public class BaseRenderData
    {
        public List<Vertex> data = new List<Vertex>();
        public List<shape> PolygonO = new List<shape>();
        public List<FMAT> mats = new List<FMAT>();
        public List<BRTI> textures = new List<BRTI>();

        public class shape
        {
            public float face;
            public string name;
            public int Index;
            public int FvtxIndex;
            public int FmatIndex;
        }

        public struct Vertex
        {
            public float x, y, z;
            public float nx, ny, nz;
            public Vector2 uv0;
            public float i1, i2, i3, i4; // can have 5
            public float w1, w2, w3, w4;
            public const int Stride = 4 * 16;

            public Vector3h NormalVec
            {
                get { return new Vector3h(nx, ny, nz); }
            }

            public Vector3h Vec
            {
                get { return new Vector3h(x, y, z); }
            }
        }

        public void Join(BaseRenderData rnd)
        {
            var datalen = data.Count;
            var polyLen = PolygonO.Count;

            data.AddRange(rnd.data);
            PolygonO.AddRange(rnd.PolygonO);
            for (int i = polyLen; i < PolygonO.Count; i++)
            {
                PolygonO[i].face += datalen; //offset
            }
        }
    }
}
