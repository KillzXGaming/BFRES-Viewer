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
        }

    }
}
