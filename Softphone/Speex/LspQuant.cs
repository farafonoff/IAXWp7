namespace Ozeki.Media.Codec.Speex.Implementation
{
    using System;

    internal abstract class LspQuant
    {
        protected const int MAX_LSP_SIZE = 20;

        protected internal LspQuant()
        {
        }

        protected internal static int Lsp_quant(float[] x, int xs, int[] cdbk, int nbVec, int nbDim)
        {
            int num2;
            float num5 = 0f;
            int num6 = 0;
            int num7 = 0;
            for (int i = 0; i < nbVec; i++)
            {
                float num3 = 0f;
                num2 = 0;
                while (num2 < nbDim)
                {
                    float num4 = x[xs + num2] - cdbk[num7++];
                    num3 += num4 * num4;
                    num2++;
                }
                if ((num3 < num5) || (i == 0))
                {
                    num5 = num3;
                    num6 = i;
                }
            }
            for (num2 = 0; num2 < nbDim; num2++)
            {
                x[xs + num2] -= cdbk[(num6 * nbDim) + num2];
            }
            return num6;
        }

        protected internal static int Lsp_weight_quant(float[] x, int xs, float[] weight, int ws, int[] cdbk, int nbVec, int nbDim)
        {
            int num2;
            float num5 = 0f;
            int num6 = 0;
            int num7 = 0;
            for (int i = 0; i < nbVec; i++)
            {
                float num3 = 0f;
                num2 = 0;
                while (num2 < nbDim)
                {
                    float num4 = x[xs + num2] - cdbk[num7++];
                    num3 += (weight[ws + num2] * num4) * num4;
                    num2++;
                }
                if ((num3 < num5) || (i == 0))
                {
                    num5 = num3;
                    num6 = i;
                }
            }
            for (num2 = 0; num2 < nbDim; num2++)
            {
                x[xs + num2] -= cdbk[(num6 * nbDim) + num2];
            }
            return num6;
        }

        public abstract void Quant(float[] lsp, float[] qlsp, int order, Bits bits);
        protected internal void UnpackPlus(float[] lsp, int[] tab, Bits bits, float k, int ti, int li)
        {
            int num = bits.Unpack(6);
            for (int i = 0; i < ti; i++)
            {
                lsp[i + li] += k * tab[(num * ti) + i];
            }
        }

        public abstract void Unquant(float[] lsp, int order, Bits bits);
    }
}

