namespace Ozeki.Media.Codec.Speex.Implementation
{
    using System;

    internal class SplitShapeSearch : CodebookSearch
    {
        private float[] e;
        private float[] E;
        private int have_sign;
        private int[] ind;
        private const int MAX_COMPLEXITY = 10;
        private int nb_subvect;
        private int[,] nind;
        private float[][] nt;
        private int[,] oind;
        private float[][] ot;
        private float[] r2;
        private int shape_bits;
        private int[] shape_cb;
        private int shape_cb_size;
        private int[] signs;
        private int subframesize;
        private int subvect_size;
        private float[] t;

        public SplitShapeSearch(int subframesize_0, int subvect_size_1, int nb_subvect_2, int[] shape_cb_3, int shape_bits_4, int have_sign_5)
        {
            this.subframesize = subframesize_0;
            this.subvect_size = subvect_size_1;
            this.nb_subvect = nb_subvect_2;
            this.shape_cb = shape_cb_3;
            this.shape_bits = shape_bits_4;
            this.have_sign = have_sign_5;
            this.ind = new int[nb_subvect_2];
            this.signs = new int[nb_subvect_2];
            this.shape_cb_size = ((int) 1) << shape_bits_4;
            this.ot = this.CreateJaggedArray<float>(10, subframesize_0);
            this.nt = this.CreateJaggedArray<float>(10, subframesize_0);
            this.oind = new int[10, nb_subvect_2];
            this.nind = new int[10, nb_subvect_2];
            this.t = new float[subframesize_0];
            this.e = new float[subframesize_0];
            this.r2 = new float[subframesize_0];
            this.E = new float[this.shape_cb_size];
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

        public sealed override void Quantify(float[] target, float[] ak, float[] awk1, float[] awk2, int p, int nsf, float[] exc, int es, float[] r, Bits bits, int complexity)
        {
            int num2;
            int num3;
            int n = complexity;
            if (n > 10)
            {
                n = 10;
            }
            float[] codebook = new float[this.shape_cb_size * this.subvect_size];
            int[] nbest = new int[n];
            float[] numArray5 = new float[n];
            float[] numArray2 = new float[n];
            float[] numArray3 = new float[n];
            int[] numArray6 = new int[n];
            int[] numArray7 = new int[n];
            int index = 0;
            while (index < n)
            {
                for (num2 = 0; num2 < this.nb_subvect; num2++)
                {
                    this.nind[index, num2] = this.oind[index, num2] = -1;
                }
                index++;
            }
            num2 = 0;
            while (num2 < n)
            {
                index = 0;
                while (index < nsf)
                {
                    this.ot[num2][index] = target[index];
                    index++;
                }
                num2++;
            }
            for (index = 0; index < this.shape_cb_size; index++)
            {
                int num8 = index * this.subvect_size;
                int num9 = index * this.subvect_size;
                num2 = 0;
                while (num2 < this.subvect_size)
                {
                    codebook[num8 + num2] = 0f;
                    num3 = 0;
                    while (num3 <= num2)
                    {
                        codebook[num8 + num2] += (0.03125f * this.shape_cb[num9 + num3]) * r[num2 - num3];
                        num3++;
                    }
                    num2++;
                }
                this.E[index] = 0f;
                for (num2 = 0; num2 < this.subvect_size; num2++)
                {
                    this.E[index] += codebook[num8 + num2] * codebook[num8 + num2];
                }
            }
            num2 = 0;
            while (num2 < n)
            {
                numArray3[num2] = 0f;
                num2++;
            }
            for (index = 0; index < this.nb_subvect; index++)
            {
                int num4;
                int num5;
                int offset = index * this.subvect_size;
                num2 = 0;
                while (num2 < n)
                {
                    numArray2[num2] = 2.147484E+09f;
                    num2++;
                }
                num2 = 0;
                while (num2 < n)
                {
                    numArray6[num2] = numArray7[num2] = 0;
                    num2++;
                }
                num2 = 0;
                while (num2 < n)
                {
                    float num11 = 0f;
                    num4 = offset;
                    while (num4 < (offset + this.subvect_size))
                    {
                        num11 += this.ot[num2][num4] * this.ot[num2][num4];
                        num4++;
                    }
                    num11 *= 0.5f;
                    if (this.have_sign != 0)
                    {
                        VQ.Nbest_sign(this.ot[num2], offset, codebook, this.subvect_size, this.shape_cb_size, this.E, n, nbest, numArray5);
                    }
                    else
                    {
                        VQ.Nbest(this.ot[num2], offset, codebook, this.subvect_size, this.shape_cb_size, this.E, n, nbest, numArray5);
                    }
                    for (num3 = 0; num3 < n; num3++)
                    {
                        float num12 = (numArray3[num2] + numArray5[num3]) + num11;
                        if (num12 < numArray2[n - 1])
                        {
                            num4 = 0;
                            while (num4 < n)
                            {
                                if (num12 < numArray2[num4])
                                {
                                    num5 = n - 1;
                                    while (num5 > num4)
                                    {
                                        numArray2[num5] = numArray2[num5 - 1];
                                        numArray6[num5] = numArray6[num5 - 1];
                                        numArray7[num5] = numArray7[num5 - 1];
                                        num5--;
                                    }
                                    numArray2[num4] = num12;
                                    numArray6[num5] = nbest[num3];
                                    numArray7[num5] = num2;
                                    break;
                                }
                                num4++;
                            }
                        }
                    }
                    if (index == 0)
                    {
                        break;
                    }
                    num2++;
                }
                num2 = 0;
                while (num2 < n)
                {
                    int num6;
                    num4 = (index + 1) * this.subvect_size;
                    while (num4 < nsf)
                    {
                        this.nt[num2][num4] = this.ot[numArray7[num2]][num4];
                        num4++;
                    }
                    num4 = 0;
                    while (num4 < this.subvect_size)
                    {
                        float num15 = 1f;
                        int num14 = numArray6[num2];
                        if (num14 >= this.shape_cb_size)
                        {
                            num15 = -1f;
                            num14 -= this.shape_cb_size;
                        }
                        num6 = this.subvect_size - num4;
                        float num13 = (num15 * 0.03125f) * this.shape_cb[(num14 * this.subvect_size) + num4];
                        num5 = 0;
                        for (int i = offset + this.subvect_size; num5 < (nsf - (this.subvect_size * (index + 1))); i++)
                        {
                            this.nt[num2][i] -= num13 * r[num5 + num6];
                            num5++;
                        }
                        num4++;
                    }
                    for (num6 = 0; num6 < this.nb_subvect; num6++)
                    {
                        this.nind[num2, num6] = this.oind[numArray7[num2], num6];
                    }
                    this.nind[num2, index] = numArray6[num2];
                    num2++;
                }
                float[][] ot = this.ot;
                this.ot = this.nt;
                this.nt = ot;
                num2 = 0;
                while (num2 < n)
                {
                    for (num4 = 0; num4 < this.nb_subvect; num4++)
                    {
                        this.oind[num2, num4] = this.nind[num2, num4];
                    }
                    num2++;
                }
                num2 = 0;
                while (num2 < n)
                {
                    numArray3[num2] = numArray2[num2];
                    num2++;
                }
            }
            for (index = 0; index < this.nb_subvect; index++)
            {
                this.ind[index] = this.nind[0, index];
                bits.Pack(this.ind[index], this.shape_bits + this.have_sign);
            }
            for (index = 0; index < this.nb_subvect; index++)
            {
                float num18 = 1f;
                int num17 = this.ind[index];
                if (num17 >= this.shape_cb_size)
                {
                    num18 = -1f;
                    num17 -= this.shape_cb_size;
                }
                num2 = 0;
                while (num2 < this.subvect_size)
                {
                    this.e[(this.subvect_size * index) + num2] = (num18 * 0.03125f) * this.shape_cb[(num17 * this.subvect_size) + num2];
                    num2++;
                }
            }
            for (num2 = 0; num2 < nsf; num2++)
            {
                exc[es + num2] += this.e[num2];
            }
            Filters.Syn_percep_zero(this.e, 0, ak, awk1, awk2, this.r2, nsf, p);
            for (num2 = 0; num2 < nsf; num2++)
            {
                target[num2] -= this.r2[num2];
            }
        }

        public sealed override void Unquantify(float[] exc, int es, int nsf, Bits bits)
        {
            int num;
            for (num = 0; num < this.nb_subvect; num++)
            {
                if (this.have_sign != 0)
                {
                    this.signs[num] = bits.Unpack(1);
                }
                else
                {
                    this.signs[num] = 0;
                }
                this.ind[num] = bits.Unpack(this.shape_bits);
            }
            for (num = 0; num < this.nb_subvect; num++)
            {
                float num3 = 1f;
                if (this.signs[num] != 0)
                {
                    num3 = -1f;
                }
                for (int i = 0; i < this.subvect_size; i++)
                {
                    exc[(es + (this.subvect_size * num)) + i] += (num3 * 0.03125f) * this.shape_cb[(this.ind[num] * this.subvect_size) + i];
                }
            }
        }
    }
}

