namespace Ozeki.Media.Codec.Speex.Implementation
{
    using System;

    internal class Misc
    {
        public static float[] LagWindow(int lpcSize, float lagFactor)
        {
            float[] numArray = new float[lpcSize + 1];
            for (int i = 0; i < (lpcSize + 1); i++)
            {
                numArray[i] = (float) Math.Exp((-0.5 * ((6.2831853071795862 * lagFactor) * i)) * ((6.2831853071795862 * lagFactor) * i));
            }
            return numArray;
        }

        public static float[] Window(int windowSize, int subFrameSize)
        {
            int num;
            int num2 = (subFrameSize * 7) / 2;
            int num3 = (subFrameSize * 5) / 2;
            float[] numArray = new float[windowSize];
            for (num = 0; num < num2; num++)
            {
                numArray[num] = (float) (0.54 - (0.46 * Math.Cos((3.1415926535897931 * num) / ((double) num2))));
            }
            for (num = 0; num < num3; num++)
            {
                numArray[num2 + num] = (float) (0.54 + (0.46 * Math.Cos((3.1415926535897931 * num) / ((double) num3))));
            }
            return numArray;
        }
    }
}

