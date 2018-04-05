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
    public enum TrackType
    {
        XSCA = 0x4,
        YSCA = 0x8,
        ZSCA = 0xC,
        XPOS = 0x10,
        YPOS = 0x14,
        ZPOS = 0x18,
        XROT = 0x20,
        YROT = 0x24,
        ZROT = 0x28,
    }

    public class FSKA : AnimationNode
    {
        public int HeaderLength1;
        public int HeaderLength2;

        public int frameCount;
        public int boneCount;
        public int flags;
        public int curveCount;
        public int bakeSize;
        public int BoneAnimArrayOffset;
        public int SkeletonOffset;

        public FSKA(FileData f)
        {
            ImageKey = "animation";
            SelectedImageKey = "animation";

            if (!f.readString(4).Equals("FSKA"))
                throw new Exception("Error reading Skeletal Animation");
            HeaderLength1 = f.readInt();
            HeaderLength2 = f.readInt();
            f.skip(4); //padding
            Text = f.readString(f.readOffset() + 2, -1);
            f.skip(4); //padding
            int FilePath = f.readOffset();
            f.skip(12); //padding
            int unk3 = f.readInt();
            f.skip(4); //padding
            BoneAnimArrayOffset = f.readOffset(); // offset to start of base values
            f.skip(4); //padding
            SkeletonOffset = f.readOffset();
            f.skip(12); //padding
            flags = f.readInt();
            frameCount = f.readInt();
            curveCount = f.readInt();
            bakeSize = f.readInt();
            boneCount = f.readShort();
            int userDataCount = f.readShort();
            f.skip(4); //padding
            f.seek(BoneAnimArrayOffset);
            for (int i = 0; i < boneCount - 1; i++)
            {
                Nodes.Add(new FSKANode(f));
            }


        }

        public override void nextFrame(Skeleton s)
        {
            foreach (TreeNode node in Nodes)
            {
                FSKANode key = (FSKANode)(node);
                // find bone
                foreach (TreeNode b in s.Nodes)
                {
                    if (((Bone)b).Text.Equals(key.Text))
                    {
                        Bone bone = ((Bone)b);
                        if (frame == 0)
                        {
                            ((Bone)b).pos = key.pos;
                            ((Bone)b).rot = key.rot;
                            ((Bone)b).sca = key.sca;
                        }
                        foreach (FSKATrack track in key.tracks)
                        {
                            // get left and right key
                            FSKAKey left = track.GetLeft(frame);
                            FSKAKey right = track.GetRight(frame);
                            float value;

                            //float value = Interpolate.Herp(left.unk1, right.unk1, left.unk2, left.unk4, frame - left.frame, right.frame - left.frame);
                            value = Interpolate.interHermite(frame, left.frame, right.frame, 0, 0, left.unk1, right.unk1);


                            Vector3 pos = bone.pos, rot = bone.rot, sca = bone.sca;

                            // interpolate the value and apply
                            switch (track.flag)
                            {
                                case (int)TrackType.XPOS: pos.X = value; break;
                                case (int)TrackType.YPOS: pos.Y = value; break;
                                case (int)TrackType.ZPOS: pos.Z = value; break;
                                case (int)TrackType.XROT: rot.X = value; break;
                                case (int)TrackType.YROT: rot.Y = value; break;
                                case (int)TrackType.ZROT: rot.Z = value; break;
                                case (int)TrackType.XSCA: sca.X = value; break;
                                case (int)TrackType.YSCA: sca.Y = value; break;
                                case (int)TrackType.ZSCA: sca.Z = value; break;
                            }
                            bone.pos = pos;
                            bone.rot = rot;
                            bone.sca = sca;
                        }
                        break;
                    }
                }
            }
            frame++;
            if (frame > frameCount)
                frame = 0;
            s.Update();
        }
   
    public override void Render(Matrix4 v)
        {

        }
    }

    public class FSKANode : TreeNode
    {
        public int flags;
        public int flags2;
        public int stride;
        public int BeginRotate;
        public int BeginTranslate;
        public int offBase;
        public int trackCount;
        public int trackFlag;
        public int offTrack;

        public Vector3 sca, rot, pos;
        public List<FSKATrack> tracks = new List<FSKATrack>();

        public FSKANode(FileData f)
        {
            ImageKey = "bone";
            SelectedImageKey = "bone";
            Text = f.readString(f.readOffset() + 2, -1);
            f.skip(4); // padding
            offTrack = f.readOffset();
            f.skip(4); // padding
            offBase = f.readOffset();
            f.skip(4); // padding
            flags = f.readInt();
            BeginRotate = f.readByte();
            BeginTranslate = f.readByte();
            trackCount = f.readByte();
            stride = f.readByte();
            f.skip(8); // padding

            trackFlag = (flags & 0x0000FF00) >> 8;

            int temp = f.pos();

            // offset 1 is base positions
            //Console.WriteLine(off1.ToString("x"));
            if (offBase != 0)
            {
                f.seek(offBase);
                sca = new Vector3(f.readFloat(), f.readFloat(), f.readFloat());
                rot = new Vector3(f.readFloat(), f.readFloat(), f.readFloat());
                f.skip(4); // for quaternion, but 1.0 if eul
                pos = new Vector3(f.readFloat(), f.readFloat(), f.readFloat());
            }
            else
            {
                sca = new Vector3(1, 1, 1);
                rot = new Vector3(0, 0, 0);
                f.skip(4); // for quaternion, but 1.0 if eul
                pos = new Vector3(0, 0, 0);

            }



            f.seek(offTrack);
            for (int tr = 0; tr < trackCount; tr++)
            {
                FSKATrack t = (new FSKATrack()
                {
                    offset = f.pos(),
                    offtolastKeys = f.readOffset(),
                    padding1 = f.readInt(),
                    offtolastData = f.readOffset(),
                    padding2 = f.readInt(),
                    type = (short)f.readShort(),
                    keyCount = (short)f.readShort(),
                    flag = f.readInt(), //targetOffset
                    unk2 = f.readInt(), //StartFrame
                    frameCount = f.readFloat(), //EndFrame
                    scale = f.readFloat(),
                    init = f.readFloat(), //union
                    unkf3 = f.readFloat(), //DataDelta
                    padding3 = f.readInt(),

                });
                tracks.Add(t);

                if (t.type != 0x2 && t.type != 0x5 && t.type != 0x6 && t.type != 0x9 && t.type != 0xA)
                    Console.WriteLine(Text + " " + t.type.ToString("x"));

                int tem = f.pos();
                // bone section
                f.seek(t.offtolastKeys);
                int[] frames = new int[t.keyCount];
                for (int i = 0; i < t.keyCount; i++)
                    if (t.type == 0x1 || t.type == 0x5 || t.type == 0x9)
                        frames[i] = f.readShort() >> 5;
                    else
                        frames[i] = f.readByte();
                f.align(4);

                float tanscale = t.unkf3;
                if (tanscale == 0)
                    tanscale = 1;
                f.seek(t.offtolastData);
                for (int i = 0; i < t.keyCount; i++)
                    switch (t.type)
                    {
                        case 0x0:
                            t.keys.Add(new FSKAKey()
                            {
                                frame = frames[i],
                                unk1 = t.init + ((f.readFloat() * t.scale)),
                                unk2 = f.readFloat(),
                                unk3 = f.readFloat(),
                                unk4 = f.readFloat(),
                            });
                            break;
                        case 0x1:
                            t.keys.Add(new FSKAKey()
                            {
                                frame = frames[i],
                                unk1 = t.init + ((f.readShort() * t.scale)),
                                unk2 = f.readShort(),
                                unk3 = f.readShort(),
                                unk4 = f.readShort(),
                            });
                            break;
                        case 0x2:
                            t.keys.Add(new FSKAKey()
                            {
                                frame = frames[i],
                                unk1 = t.init + ((f.readFloat() * t.scale)),
                                unk2 = f.readFloat(),
                                unk3 = f.readFloat(),
                                unk4 = f.readFloat(),
                            });
                            break;
                        case 0x5:
                            t.keys.Add(new FSKAKey()
                            {
                                frame = frames[i],
                                unk1 = t.init + (((short)f.readShort() * t.scale)),
                                unk2 = t.unkf3 + (((short)f.readShort() * t.scale)),
                                unk3 = t.unkf3 + (((short)f.readShort() * t.scale)),
                                unk4 = t.unkf3 + (((short)f.readShort() * t.scale))
                            });
                            break;
                        case 0x6:
                            t.keys.Add(new FSKAKey()
                            {
                                frame = frames[i],
                                unk1 = t.init + (((short)f.readShort() * t.scale)),
                                unk2 = t.unkf3 + (((short)f.readShort() / (float)0x7FFF)),
                                unk3 = t.unkf3 + (((short)f.readShort() / (float)0x7FFF)),
                                unk4 = t.unkf3 + (((short)f.readShort() / (float)0x7FFF))
                            });
                            break;
                        case 0x9:
                            t.keys.Add(new FSKAKey()
                            {
                                frame = frames[i],
                                unk1 = t.init + (((sbyte)f.readByte() * t.scale)),
                                unk2 = t.unkf3 + (((sbyte)f.readByte() * t.scale)),
                                unk3 = t.unkf3 + (((sbyte)f.readByte() * t.scale)),
                                unk4 = t.unkf3 + (((sbyte)f.readByte() * t.scale))
                            });
                            break;
                        case 0xA:
                            t.keys.Add(new FSKAKey()
                            {
                                frame = frames[i],
                                unk1 = t.init + (((sbyte)f.readByte() * t.scale)),
                                unk2 = t.unkf3 + (((sbyte)f.readByte() * t.scale)),
                                unk3 = t.unkf3 + (((sbyte)f.readByte() * t.scale)),
                                unk4 = t.unkf3 + (((sbyte)f.readByte() * t.scale))
                            });
                            break;
                        default:
                            break;
                    }

                f.seek(tem);
            }

            f.seek(temp);
        }

        public void Display(ListView list)
        {
            list.Visible = true;
            list.Items.Clear();
            list.Items.Add(Convert.ToString(trackFlag, 2) + " Track Count:" + trackCount);
            list.Items.Add(pos.ToString() + " " + rot.ToString() + " " + sca.ToString());
            foreach (FSKATrack t in tracks)
            {
                string s = "";

                s += "Type = " + t.type + " key: " + t.keyCount + " Flag:" + (TrackType)t.flag + " " + t.unk2 + " fcount: " + t.frameCount
                    + " Scale = " + t.scale + "Union = " + t.init + " " + t.unkf3
                    + " " + t.offtolastKeys.ToString("x") + " " + t.offtolastData.ToString("x");

                list.Items.Add(s);

                s = t.offset.ToString("x");
                list.Items.Add(s);

                foreach (FSKAKey key in t.keys)
                {
                    s = "";

                    s += key.frame + " " + key.unk1 + " " + key.unk2 + " " + key.unk3 + " " + key.unk4;

                    list.Items.Add(s);
                }
            }
        }
    }

    public class FSKATrack
    {
        public short type;
        public short keyCount;
        public int flag;
        public int unk2;
        public int padding1;
        public int padding2;
        public int padding3;
        public float frameCount;
        public float scale, init, unkf3;
        public int offtolastKeys, offtolastData;
        public List<FSKAKey> keys = new List<FSKAKey>();

        public int offset;

        public FSKAKey GetLeft(int frame)
        {
            FSKAKey prev = keys[0];

            for (int i = 0; i < keys.Count - 1; i++)
            {
                FSKAKey key = keys[i];
                if (key.frame > frame && prev.frame <= frame)
                    break;
                prev = key;
            }

            return prev;
        }
        public FSKAKey GetRight(int frame)
        {
            FSKAKey cur = keys[0];
            FSKAKey prev = keys[0];

            for (int i = 1; i < keys.Count; i++)
            {
                FSKAKey key = keys[i];
                cur = key;
                if (key.frame > frame && prev.frame <= frame)
                    break;
                prev = key;
            }

            return cur;
        }
    }

  
      


    public class FSKAKey
    {
        public int frame;
        public float unk1, unk2, unk3, unk4;

        public int offset;
    }

}
