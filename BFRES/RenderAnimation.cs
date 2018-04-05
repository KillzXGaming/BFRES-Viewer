using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BFRES
{
    public abstract class AnimationNode : RenderableNode
    {
        public int frame;

        abstract public void nextFrame(Skeleton s);

    }

    class Interpolate
    {
        public static float Lerp(float L, float R, float W)
        {
            return L * (1 - W) + R * W;
        }

        public static float Herp(float L, float R, float LS, float RS, float Diff, float W)
        {
            float W1 = W - 1;

            float Result;

            Result = L + (L - R) * (2 * W - 3) * W * W;
            Result += (Diff * W1) * (LS * W1 + RS * W);

            return Result;
        }

        public static float interHermite(float frame, float frame1, float frame2, float outslope, float inslope, float val1, float val2)
        {
            float distance = frame - frame1;
            float invDuration = 1f / (frame2 - frame1);
            float t = distance * invDuration;
            float t1 = t - 1f;
            return (val1 + ((((val1 - val2) * ((2f * t) - 3f)) * t) * t)) + ((distance * t1) * ((t1 * outslope) + (t * inslope)));
        }
    }
}
