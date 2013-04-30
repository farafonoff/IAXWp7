namespace Ozeki.Media.Codec.Speex.Implementation
{
    using System;

    internal class Filters
    {
        private int last_pitch;
        private float[] last_pitch_gain;
        private float smooth_gain;
        private float[] xx;

        public Filters()
        {
            float num;
            this.last_pitch_gain = new float[3];
            this.xx = new float[0x400];
            this.last_pitch = 0;
            this.last_pitch_gain[2] = num = 0f;
            this.last_pitch_gain[0] = this.last_pitch_gain[1] = num;
            this.smooth_gain = 1f;
        }

        public static void Bw_lpc(float gamma, float[] lpc_in, float[] lpc_out, int order)
        {
            float num = 1f;
            for (int i = 0; i < (order + 1); i++)
            {
                lpc_out[i] = num * lpc_in[i];
                num *= gamma;
            }
        }

        public void Comb_filter(float[] exc, int esi, float[] new_exc, int nsi, int nsf, int pitch, float[] pitch_gain, float comb_gain)
        {
            int num;
            float num3 = 0f;
            float num4 = 0f;
            float num8 = 0f;
            for (num = esi; num < (esi + nsf); num++)
            {
                num3 += exc[num] * exc[num];
            }
            num8 = 0.5f * Math.Abs((float) (((((pitch_gain[0] + pitch_gain[1]) + pitch_gain[2]) + this.last_pitch_gain[0]) + this.last_pitch_gain[1]) + this.last_pitch_gain[2]));
            if (num8 > 1.3f)
            {
                comb_gain *= 1.3f / num8;
            }
            if (num8 < 0.5f)
            {
                comb_gain *= 2f * num8;
            }
            float num6 = 1f / ((float) nsf);
            float num7 = 0f;
            num = 0;
            for (int i = esi; num < nsf; i++)
            {
                num7 += num6;
                new_exc[nsi + num] = (exc[i] + ((comb_gain * num7) * (((pitch_gain[0] * exc[(i - pitch) + 1]) + (pitch_gain[1] * exc[i - pitch])) + (pitch_gain[2] * exc[(i - pitch) - 1])))) + ((comb_gain * (1f - num7)) * (((this.last_pitch_gain[0] * exc[(i - this.last_pitch) + 1]) + (this.last_pitch_gain[1] * exc[i - this.last_pitch])) + (this.last_pitch_gain[2] * exc[(i - this.last_pitch) - 1])));
                num++;
            }
            this.last_pitch_gain[0] = pitch_gain[0];
            this.last_pitch_gain[1] = pitch_gain[1];
            this.last_pitch_gain[2] = pitch_gain[2];
            this.last_pitch = pitch;
            for (num = nsi; num < (nsi + nsf); num++)
            {
                num4 += new_exc[num] * new_exc[num];
            }
            float num5 = (float) Math.Sqrt((double) (num3 / (0.1f + num4)));
            if (num5 < 0.5f)
            {
                num5 = 0.5f;
            }
            if (num5 > 1f)
            {
                num5 = 1f;
            }
            for (num = nsi; num < (nsi + nsf); num++)
            {
                this.smooth_gain = (0.96f * this.smooth_gain) + (0.04f * num5);
                new_exc[num] *= this.smooth_gain;
            }
        }

        public static void Filter_mem2(float[] x, int xs, float[] num, float[] den, int N, int ord, float[] mem, int ms)
        {
            for (int i = 0; i < N; i++)
            {
                float num4 = x[xs + i];
                x[xs + i] = (num[0] * num4) + mem[ms];
                float num5 = x[xs + i];
                for (int j = 0; j < (ord - 1); j++)
                {
                    mem[ms + j] = (mem[(ms + j) + 1] + (num[j + 1] * num4)) - (den[j + 1] * num5);
                }
                mem[(ms + ord) - 1] = (num[ord] * num4) - (den[ord] * num5);
            }
        }

        public static void Filter_mem2(float[] x, int xs, float[] num, float[] den, float[] y, int ys, int N, int ord, float[] mem, int ms)
        {
            for (int i = 0; i < N; i++)
            {
                float num4 = x[xs + i];
                y[ys + i] = (num[0] * num4) + mem[0];
                float num5 = y[ys + i];
                for (int j = 0; j < (ord - 1); j++)
                {
                    mem[ms + j] = (mem[(ms + j) + 1] + (num[j + 1] * num4)) - (den[j + 1] * num5);
                }
                mem[(ms + ord) - 1] = (num[ord] * num4) - (den[ord] * num5);
            }
        }

        public void Fir_mem_up(float[] x, float[] a, float[] y, int N, int M, float[] mem)
        {
            int num;
            for (num = 0; num < (N / 2); num++)
            {
                this.xx[2 * num] = x[((N / 2) - 1) - num];
            }
            for (num = 0; num < (M - 1); num += 2)
            {
                this.xx[N + num] = mem[num + 1];
            }
            for (num = 0; num < N; num += 4)
            {
                float num4;
                float num5;
                float num6;
                float num3 = num4 = num5 = num6 = 0f;
                float num7 = this.xx[(N - 4) - num];
                for (int i = 0; i < M; i += 4)
                {
                    float num9 = a[i];
                    float num10 = a[i + 1];
                    float num8 = this.xx[((N - 2) + i) - num];
                    num3 += num9 * num8;
                    num4 += num10 * num8;
                    num5 += num9 * num7;
                    num6 += num10 * num7;
                    num9 = a[i + 2];
                    num10 = a[i + 3];
                    num7 = this.xx[(N + i) - num];
                    num3 += num9 * num7;
                    num4 += num10 * num7;
                    num5 += num9 * num8;
                    num6 += num10 * num8;
                }
                y[num] = num3;
                y[num + 1] = num4;
                y[num + 2] = num5;
                y[num + 3] = num6;
            }
            for (num = 0; num < (M - 1); num += 2)
            {
                mem[num + 1] = this.xx[num];
            }
        }

        public static void Fir_mem2(float[] x, int xs, float[] num, float[] y, int ys, int N, int ord, float[] mem)
        {
            for (int i = 0; i < N; i++)
            {
                float num4 = x[xs + i];
                y[ys + i] = (num[0] * num4) + mem[0];
                for (int j = 0; j < (ord - 1); j++)
                {
                    mem[j] = mem[j + 1] + (num[j + 1] * num4);
                }
                mem[ord - 1] = num[ord] * num4;
            }
        }

        public static void Iir_mem2(float[] x, int xs, float[] den, float[] y, int ys, int N, int ord, float[] mem)
        {
            for (int i = 0; i < N; i++)
            {
                y[ys + i] = x[xs + i] + mem[0];
                for (int j = 0; j < (ord - 1); j++)
                {
                    mem[j] = mem[j + 1] - (den[j + 1] * y[ys + i]);
                }
                mem[ord - 1] = -den[ord] * y[ys + i];
            }
        }

        public static void Qmf_decomp(float[] xx_0, float[] aa, float[] y1, float[] y2, int N, int M, float[] mem)
        {
            int num;
            float[] numArray = new float[M];
            float[] numArray2 = new float[(N + M) - 1];
            int num5 = M - 1;
            int num4 = M >> 1;
            for (num = 0; num < M; num++)
            {
                numArray[(M - num) - 1] = aa[num];
            }
            for (num = 0; num < (M - 1); num++)
            {
                numArray2[num] = mem[(M - num) - 2];
            }
            for (num = 0; num < N; num++)
            {
                numArray2[(num + M) - 1] = xx_0[num];
            }
            num = 0;
            for (int i = 0; num < N; i++)
            {
                y1[i] = 0f;
                y2[i] = 0f;
                for (int j = 0; j < num4; j++)
                {
                    y1[i] += numArray[j] * (numArray2[num + j] + numArray2[(num5 + num) - j]);
                    y2[i] -= numArray[j] * (numArray2[num + j] - numArray2[(num5 + num) - j]);
                    j++;
                    y1[i] += numArray[j] * (numArray2[num + j] + numArray2[(num5 + num) - j]);
                    y2[i] += numArray[j] * (numArray2[num + j] - numArray2[(num5 + num) - j]);
                }
                num += 2;
            }
            for (num = 0; num < (M - 1); num++)
            {
                mem[num] = xx_0[(N - num) - 1];
            }
        }

        public static void Residue_percep_zero(float[] xx_0, int xxs, float[] ak, float[] awk1, float[] awk2, float[] y, int N, int ord)
        {
            float[] mem = new float[ord];
            Filter_mem2(xx_0, xxs, ak, awk1, y, 0, N, ord, mem, 0);
            for (int i = 0; i < ord; i++)
            {
                mem[i] = 0f;
            }
            Fir_mem2(y, 0, awk2, y, 0, N, ord, mem);
        }

        public static void Syn_percep_zero(float[] xx_0, int xxs, float[] ak, float[] awk1, float[] awk2, float[] y, int N, int ord)
        {
            float[] mem = new float[ord];
            Filter_mem2(xx_0, xxs, awk1, ak, y, 0, N, ord, mem, 0);
            for (int i = 0; i < ord; i++)
            {
                mem[i] = 0f;
            }
            Iir_mem2(y, 0, awk2, y, 0, N, ord, mem);
        }
    }
}

