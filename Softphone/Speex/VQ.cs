namespace Ozeki.Media.Codec.Speex.Implementation
{
    using System;

    internal class VQ
    {
        public static int Index(float ins0, float[] codebook, int entries)
        {
            float num2 = 0f;
            int num3 = 0;
            for (int i = 0; i < entries; i++)
            {
                float num4 = ins0 - codebook[i];
                num4 *= num4;
                if ((i == 0) || (num4 < num2))
                {
                    num2 = num4;
                    num3 = i;
                }
            }
            return num3;
        }

        public static int Index(float[] ins0, float[] codebook, int len, int entries)
        {
            int num3 = 0;
            float num4 = 0f;
            int num5 = 0;
            for (int i = 0; i < entries; i++)
            {
                float num6 = 0f;
                for (int j = 0; j < len; j++)
                {
                    float num7 = ins0[j] - codebook[num3++];
                    num6 += num7 * num7;
                }
                if ((i == 0) || (num6 < num4))
                {
                    num4 = num6;
                    num5 = i;
                }
            }
            return num5;
        }

        public static void Nbest(float[] ins0, int offset, float[] codebook, int len, int entries, float[] E, int N, int[] nbest, float[] best_dist)
        {
            int num4 = 0;
            int num5 = 0;
            for (int i = 0; i < entries; i++)
            {
                float num6 = 0.5f * E[i];
                for (int j = 0; j < len; j++)
                {
                    num6 -= ins0[offset + j] * codebook[num4++];
                }
                if ((i < N) || (num6 < best_dist[N - 1]))
                {
                    int index = N - 1;
                    while ((index >= 1) && ((index > num5) || (num6 < best_dist[index - 1])))
                    {
                        best_dist[index] = best_dist[index - 1];
                        nbest[index] = nbest[index - 1];
                        index--;
                    }
                    best_dist[index] = num6;
                    nbest[index] = i;
                    num5++;
                }
            }
        }

        public static void Nbest_sign(float[] ins0, int offset, float[] codebook, int len, int entries, float[] E, int N, int[] nbest, float[] best_dist)
        {
            int num4 = 0;
            int num6 = 0;
            for (int i = 0; i < entries; i++)
            {
                int num5;
                float num7 = 0f;
                for (int j = 0; j < len; j++)
                {
                    num7 -= ins0[offset + j] * codebook[num4++];
                }
                if (num7 > 0f)
                {
                    num5 = 1;
                    num7 = -num7;
                }
                else
                {
                    num5 = 0;
                }
                num7 += 0.5f * E[i];
                if ((i < N) || (num7 < best_dist[N - 1]))
                {
                    int index = N - 1;
                    while ((index >= 1) && ((index > num6) || (num7 < best_dist[index - 1])))
                    {
                        best_dist[index] = best_dist[index - 1];
                        nbest[index] = nbest[index - 1];
                        index--;
                    }
                    best_dist[index] = num7;
                    nbest[index] = i;
                    num6++;
                    if (num5 != 0)
                    {
                        nbest[index] += entries;
                    }
                }
            }
        }
    }
}

