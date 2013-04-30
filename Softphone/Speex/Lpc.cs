namespace Ozeki.Media.Codec.Speex.Implementation
{
    using System;

    internal class Lpc
    {
        public static void Autocorr(float[] x, float[] ac, int lag, int n)
        {
            while (lag-- > 0)
            {
                int index = lag;
                float num = 0f;
                while (index < n)
                {
                    num += x[index] * x[index - lag];
                    index++;
                }
                ac[lag] = num;
            }
        }

        public static float Wld(float[] lpc, float[] ac, float[] xref, int p)
        {
            int num;
            float num4 = ac[0];
            if (ac[0] == 0f)
            {
                for (num = 0; num < p; num++)
                {
                    xref[num] = 0f;
                }
                return 0f;
            }
            for (num = 0; num < p; num++)
            {
                float num3 = -ac[num + 1];
                int index = 0;
                while (index < num)
                {
                    num3 -= lpc[index] * ac[num - index];
                    index++;
                }
                xref[num] = num3 /= num4;
                lpc[num] = num3;
                index = 0;
                while (index < (num / 2))
                {
                    float num5 = lpc[index];
                    lpc[index] += num3 * lpc[(num - 1) - index];
                    lpc[(num - 1) - index] += num3 * num5;
                    index++;
                }
                if ((num % 2) != 0)
                {
                    lpc[index] += lpc[index] * num3;
                }
                float? nullable = 1f;
                num4 *= nullable.Value - (num3 * num3);
            }
            return num4;
        }
    }
}

