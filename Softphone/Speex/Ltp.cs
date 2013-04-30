namespace Ozeki.Media.Codec.Speex.Implementation
{
    using System;

    internal abstract class Ltp
    {
        protected Ltp()
        {
        }

        protected internal static float Inner_prod(float[] x, int xs, float[] y, int ys, int len)
        {
            float num2 = 0f;
            float num3 = 0f;
            float num4 = 0f;
            float num5 = 0f;
            for (int i = 0; i < len; i += 4)
            {
                num2 += x[xs + i] * y[ys + i];
                num3 += x[(xs + i) + 1] * y[(ys + i) + 1];
                num4 += x[(xs + i) + 2] * y[(ys + i) + 2];
                num5 += x[(xs + i) + 3] * y[(ys + i) + 3];
            }
            return (((num2 + num3) + num4) + num5);
        }

        protected internal static void Open_loop_nbest_pitch(float[] sw, int swIdx, int start, int end, int len, int[] pitch, float[] gain, int N)
        {
            int num;
            float[] numArray = new float[N];
            float[] numArray2 = new float[(end - start) + 1];
            float[] numArray3 = new float[(end - start) + 2];
            float[] numArray4 = new float[(end - start) + 1];
            for (num = 0; num < N; num++)
            {
                numArray[num] = -1f;
                gain[num] = 0f;
                pitch[num] = start;
            }
            numArray3[0] = Inner_prod(sw, swIdx - start, sw, swIdx - start, len);
            float num4 = Inner_prod(sw, swIdx, sw, swIdx, len);
            for (num = start; num <= end; num++)
            {
                numArray3[(num - start) + 1] = (numArray3[num - start] + (sw[(swIdx - num) - 1] * sw[(swIdx - num) - 1])) - (sw[((swIdx - num) + len) - 1] * sw[((swIdx - num) + len) - 1]);
                if (numArray3[(num - start) + 1] < 1f)
                {
                    numArray3[(num - start) + 1] = 1f;
                }
            }
            for (num = start; num <= end; num++)
            {
                numArray2[num - start] = 0f;
                numArray4[num - start] = 0f;
            }
            for (num = start; num <= end; num++)
            {
                numArray2[num - start] = Inner_prod(sw, swIdx, sw, swIdx - num, len);
                numArray4[num - start] = (numArray2[num - start] * numArray2[num - start]) / (numArray3[num - start] + 1f);
            }
            for (num = start; num <= end; num++)
            {
                if (numArray4[num - start] > numArray[N - 1])
                {
                    float num5 = numArray2[num - start] / (numArray3[num - start] + 10f);
                    float num6 = (float) Math.Sqrt((double) ((num5 * numArray2[num - start]) / (num4 + 10f)));
                    if (num6 > num5)
                    {
                        num6 = num5;
                    }
                    if (num6 < 0f)
                    {
                        num6 = 0f;
                    }
                    for (int i = 0; i < N; i++)
                    {
                        if (numArray4[num - start] > numArray[i])
                        {
                            for (int j = N - 1; j > i; j--)
                            {
                                numArray[j] = numArray[j - 1];
                                pitch[j] = pitch[j - 1];
                                gain[j] = gain[j - 1];
                            }
                            numArray[i] = numArray4[num - start];
                            pitch[i] = num;
                            gain[i] = num6;
                            break;
                        }
                    }
                }
            }
        }

        public abstract int Quant(float[] target, float[] sw, int sws, float[] ak, float[] awk1, float[] awk2, float[] exc, int es, int start, int end, float pitch_coef, int p, int nsf, Bits bits, float[] exc2, int e2s, float[] r, int complexity);
        public abstract int Unquant(float[] exc, int es, int start, float pitch_coef, int nsf, float[] gain_val, Bits bits, int count_lost, int subframe_offset, float last_pitch_gain);
    }
}

