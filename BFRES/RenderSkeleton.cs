using System;
using System.Drawing;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace BFRES
{
    public class Skeleton : RenderableNode
    {
        public override void Render(Matrix4 v)
        {
            GL.PointSize(5f);
            GL.Color3(Color.Red);
            GL.Begin(PrimitiveType.Points);
            foreach(TreeNode node in Nodes)
            {
                if(node is Bone)
                {
                    Bone bone = (Bone)node;
                    GL.Vertex3(Vector3.Transform(Vector3.Zero, bone.transform));
                }
            }
            GL.End();

            GL.LineWidth(2.5f);
            GL.Begin(PrimitiveType.Lines);
            foreach (TreeNode node in Nodes)
            {
                if (node is Bone)
                {
                    Bone bone = (Bone)node;
                    if(bone.p1 != -1)
                    {
                        GL.Color3(Color.DarkBlue);
                        GL.Vertex3(Vector3.Transform(Vector3.Zero, bone.transform));
                        GL.Color3(Color.LightBlue);
                        GL.Vertex3(Vector3.Transform(Vector3.Zero, ((Bone)Nodes[bone.p1]).transform));
                    }
                }
            }
            GL.End();
        }

        Matrix4[] bone;

        public void Reset()
        {
            foreach (TreeNode node in Nodes)
            {
                if (node is Bone)
                {
                    Bone bone = (Bone)node;
                    bone.transform = Matrix4.CreateScale(bone.sca) 
                        * Matrix4.CreateFromQuaternion(FromEulerAngles(bone.rot))
                        * Matrix4.CreateTranslation(bone.pos);
                    if(bone.p1 != -1)
                        bone.transform *= ((Bone)Nodes[bone.p1]).transform;
                    // no idea how other parents work
                    bone.invert = bone.transform.Inverted();
                }
            }
            bone = new Matrix4[Nodes.Count];
        }

        public void Update()
        {
            foreach (TreeNode node in Nodes)
            {
                if (node is Bone)
                {
                    Bone bone = (Bone)node;
                    bone.transform = Matrix4.CreateScale(bone.sca)
                        * Matrix4.CreateFromQuaternion(FromEulerAngles(bone.rot))
                        * Matrix4.CreateTranslation(bone.pos);
                    if (bone.p1 != -1)
                        bone.transform *= ((Bone)Nodes[bone.p1]).transform;
                }
            }
            bone = new Matrix4[Nodes.Count];
        }

        public void PrintDepth()
        {
            Stack<Bone> q = new Stack<Bone>();
            q.Push((Bone)Nodes[0]);
            int i = 0;
            while(q.Count > 0)
            {
                Bone b = q.Pop();
                for (int n = Nodes.Count-1; n >= 0; n--) 
                    if(((Bone)Nodes[n]).p1 == b.id)
                    {
                        q.Push((Bone)Nodes[n]);
                    }
                Console.WriteLine(i++ + " " + b.Text);
            }
        }

        public static Quaternion FromEulerAngles(Vector3 rot)
        {
            {
                float z = rot.Z;
                float y = rot.Y;
                float x = rot.X;
                Quaternion xRotation = Quaternion.FromAxisAngle(Vector3.UnitX, x);
                Quaternion yRotation = Quaternion.FromAxisAngle(Vector3.UnitY, y);
                Quaternion zRotation = Quaternion.FromAxisAngle(Vector3.UnitZ, z);

                Quaternion q = (zRotation * yRotation * xRotation);

                if (q.W < 0)
                    q *= -1;
                
                return q;
            }
        }

        public Matrix4[] getBoneTransforms()
        {
            for (int i = 0; i < Nodes.Count; i++)
            {
                bone[i] = ((Bone)Nodes[i]).invert * ((Bone)Nodes[i]).transform;
            }

            return bone;
        }
    }

    public class Bone : TreeNode
    {
        public Vector3 pos = new Vector3(),
            rot = new Vector3(),
            sca = new Vector3();

        public int p1, p2, p3, p4, id;

        public Matrix4 transform = new Matrix4(), invert;

        public Bone()
        {
            ImageKey = "bone";
            SelectedImageKey = "bone";
        }
    }

    
}
