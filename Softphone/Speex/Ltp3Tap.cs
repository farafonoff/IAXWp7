namespace Ozeki.Media.Codec.Speex.Implementation
{
    using System;

    internal class Ltp3Tap : Ltp
    {
        private float[][] e;
        private float[] gain = new float[3];
        private int gain_bits;
        private int[] gain_cdbk;
        private int pitch_bits;

        public Ltp3Tap(int[] gain_cdbk_0, int gain_bits_1, int pitch_bits_2)
        {
            this.gain_cdbk = gain_cdbk_0;
            this.gain_bits = gain_bits_1;
            this.pitch_bits = pitch_bits_2;
            this.e = this.CreateJaggedArray<float>(3, 0x80);
        }

        private T[][] CreateJaggedArray<T>(int dim1, int dim2)
        {
            T[][] localArray = new T[dim1][];
            for (int i = 0; i < dim1; i++)
            {
                localArray[i] = new T[dim2];
                Array.Clear(localArray[i], 0, dim2);
            }
            return localArray;
        }

        private float Pitch_gain_search_3tap(float[] target, float[] ak, float[] awk1, float[] awk2, float[] exc, int es, int pitch, int p, int nsf, Bits bits, float[] exc2, int e2s, float[] r, int[] cdbk_index)
        {
            int num;
            int num2;
            float[] numArray2 = new float[3];
            float[][] numArray3 = this.CreateJaggedArray<float>(3, 3);
            int num3 = ((int) 1) << this.gain_bits;
            float[][] numArray = this.CreateJaggedArray<float>(3, nsf);
            this.e = this.CreateJaggedArray<float>(3, nsf);
            for (num = 2; num >= 0; num--)
            {
                int num6 = (pitch + 1) - num;
                num2 = 0;
                while (num2 < nsf)
                {
                    if ((num2 - num6) < 0)
                    {
                        this.e[num][num2] = exc2[(e2s + num2) - num6];
                    }
                    else if (((num2 - num6) - pitch) < 0)
                    {
                        this.e[num][num2] = exc2[((e2s + num2) - num6) - pitch];
                    }
                    else
                    {
                        this.e[num][num2] = 0f;
                    }
                    num2++;
                }
                if (num == 2)
                {
                    Filters.Syn_percep_zero(this.e[num], 0, ak, awk1, awk2, numArray[num], nsf, p);
                }
                else
                {
                    num2 = 0;
                    while (num2 < (nsf - 1))
                    {
                        numArray[num][num2 + 1] = numArray[num + 1][num2];
                        num2++;
                    }
                    numArray[num][0] = 0f;
                    num2 = 0;
                    while (num2 < nsf)
                    {
                        numArray[num][num2] += this.e[num][0] * r[num2];
                        num2++;
                    }
                }
            }
            for (num = 0; num < 3; num++)
            {
                numArray2[num] = Ltp.Inner_prod(numArray[num], 0, target, 0, nsf);
            }
            for (num = 0; num < 3; num++)
            {
                for (num2 = 0; num2 <= num; num2++)
                {
                    numArray3[num][num2] = numArray3[num2][num] = Ltp.Inner_prod(numArray[num], 0, numArray[num2], 0, nsf);
                }
            }
            float[] numArray4 = new float[9];
            int index = 0;
            int num8 = 0;
            float num9 = 0f;
            numArray4[0] = numArray2[2];
            numArray4[1] = numArray2[1];
            numArray4[2] = numArray2[0];
            numArray4[3] = numArray3[1][2];
            numArray4[4] = numArray3[0][1];
            numArray4[5] = numArray3[0][2];
            numArray4[6] = numArray3[2][2];
            numArray4[7] = numArray3[1][1];
            numArray4[8] = numArray3[0][0];
            for (num = 0; num < num3; num++)
            {
                float num10 = 0f;
                index = 3 * num;
                float num11 = (0.015625f * this.gain_cdbk[index]) + 0.5f;
                float num12 = (0.015625f * this.gain_cdbk[index + 1]) + 0.5f;
                float num13 = (0.015625f * this.gain_cdbk[index + 2]) + 0.5f;
                num10 += numArray4[0] * num11;
                num10 += numArray4[1] * num12;
                num10 += numArray4[2] * num13;
                num10 -= (numArray4[3] * num11) * num12;
                num10 -= (numArray4[4] * num13) * num12;
                num10 -= (numArray4[5] * num13) * num11;
                num10 -= ((0.5f * numArray4[6]) * num11) * num11;
                num10 -= ((0.5f * numArray4[7]) * num12) * num12;
                num10 -= ((0.5f * numArray4[8]) * num13) * num13;
                if ((num10 > num9) || (num == 0))
                {
                    num9 = num10;
                    num8 = num;
                }
            }
            this.gain[0] = (0.015625f * this.gain_cdbk[num8 * 3]) + 0.5f;
            this.gain[1] = (0.015625f * this.gain_cdbk[(num8 * 3) + 1]) + 0.5f;
            this.gain[2] = (0.015625f * this.gain_cdbk[(num8 * 3) + 2]) + 0.5f;
            cdbk_index[0] = num8;
            for (num = 0; num < nsf; num++)
            {
                exc[es + num] = ((this.gain[0] * this.e[2][num]) + (this.gain[1] * this.e[1][num])) + (this.gain[2] * this.e[0][num]);
            }
            float num4 = 0f;
            float num5 = 0f;
            for (num = 0; num < nsf; num++)
            {
                num4 += target[num] * target[num];
            }
            for (num = 0; num < nsf; num++)
            {
                num5 += (((target[num] - (this.gain[2] * numArray[0][num])) - (this.gain[1] * numArray[1][num])) - (this.gain[0] * numArray[2][num])) * (((target[num] - (this.gain[2] * numArray[0][num])) - (this.gain[1] * numArray[1][num])) - (this.gain[0] * numArray[2][num]));
            }
            return num5;
        }

        public sealed override int Quant(float[] target, float[] sw, int sws, float[] ak, float[] awk1, float[] awk2, float[] exc, int es, int start, int end, float pitch_coef, int p, int nsf, Bits bits, float[] exc2, int e2s, float[] r, int complexity)
        {
            int num;
            int[] numArray = new int[1];
            int pitch = 0;
            int data = 0;
            int num5 = 0;
            float num7 = -1f;
            int n = complexity;
            if (n > 10)
            {
                n = 10;
            }
            int[] numArray3 = new int[n];
            float[] gain = new float[n];
            if ((n == 0) || (end < start))
            {
                bits.Pack(0, this.pitch_bits);
                bits.Pack(0, this.gain_bits);
                for (num = 0; num < nsf; num++)
                {
                    exc[es + num] = 0f;
                }
                return start;
            }
            float[] numArray2 = new float[nsf];
            if (n > ((end - start) + 1))
            {
                n = (end - start) + 1;
            }
            Ltp.Open_loop_nbest_pitch(sw, sws, start, end, nsf, numArray3, gain, n);
            for (num = 0; num < n; num++)
            {
                pitch = numArray3[num];
                int index = 0;
                while (index < nsf)
                {
                    exc[es + index] = 0f;
                    index++;
                }
                float num6 = this.Pitch_gain_search_3tap(target, ak, awk1, awk2, exc, es, pitch, p, nsf, bits, exc2, e2s, r, numArray);
                if ((num6 < num7) || (num7 < 0f))
                {
                    for (index = 0; index < nsf; index++)
                    {
                        numArray2[index] = exc[es + index];
                    }
                    num7 = num6;
                    num5 = pitch;
                    data = numArray[0];
                }
            }
            bits.Pack(num5 - start, this.pitch_bits);
            bits.Pack(data, this.gain_bits);
            for (num = 0; num < nsf; num++)
            {
                exc[es + num] = numArray2[num];
            }
            return pitch;
        }

        public sealed override int Unquant(float[] exc, int es, int start, float pitch_coef, int nsf, float[] gain_val, Bits bits, int count_lost, int subframe_offset, float last_pitch_gain)
        {
            int num;
            int num2 = bits.Unpack(this.pitch_bits) + start;
            int num3 = bits.Unpack(this.gain_bits);
            this.gain[0] = (0.015625f * this.gain_cdbk[num3 * 3]) + 0.5f;
            this.gain[1] = (0.015625f * this.gain_cdbk[(num3 * 3) + 1]) + 0.5f;
            this.gain[2] = (0.015625f * this.gain_cdbk[(num3 * 3) + 2]) + 0.5f;
            if ((count_lost != 0) && (num2 > subframe_offset))
            {
                float num4 = Math.Abs(this.gain[1]);
                float num5 = (count_lost < 4) ? last_pitch_gain : (0.4f * last_pitch_gain);
                if (num5 > 0.95f)
                {
                    num5 = 0.95f;
                }
                if (this.gain[0] > 0f)
                {
                    num4 += this.gain[0];
                }
                else
                {
                    num4 -= 0.5f * this.gain[0];
                }
                if (this.gain[2] > 0f)
                {
                    num4 += this.gain[2];
                }
                else
                {
                    num4 -= 0.5f * this.gain[0];
                }
                if (num4 > num5)
                {
                    float num6 = num5 / num4;
                    for (num = 0; num < 3; num++)
                    {
                        this.gain[num] *= num6;
                    }
                }
            }
            gain_val[0] = this.gain[0];
            gain_val[1] = this.gain[1];
            gain_val[2] = this.gain[2];
            for (num = 0; num < 3; num++)
            {
                int num10 = (num2 + 1) - num;
                int num8 = nsf;
                if (num8 > num10)
                {
                    num8 = num10;
                }
                int num9 = nsf;
                if (num9 > (num10 + num2))
                {
                    num9 = num10 + num2;
                }
                int index = 0;
                while (index < num8)
                {
                    this.e[num][index] = exc[(es + index) - num10];
                    index++;
                }
                index = num8;
                while (index < num9)
                {
                    this.e[num][index] = exc[((es + index) - num10) - num2];
                    index++;
                }
                for (index = num9; index < nsf; index++)
                {
                    this.e[num][index] = 0f;
                }
            }
            for (num = 0; num < nsf; num++)
            {
                exc[es + num] = ((this.gain[0] * this.e[2][num]) + (this.gain[1] * this.e[1][num])) + (this.gain[2] * this.e[0][num]);
            }
            return num2;
        }
    }
}

