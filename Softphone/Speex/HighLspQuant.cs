namespace Ozeki.Media.Codec.Speex.Implementation
{
    using System;

    internal class HighLspQuant : LspQuant
    {
        public sealed override void Quant(float[] lsp, float[] qlsp, int order, Bits bits)
        {
            int num;
            float[] weight = new float[20];
            for (num = 0; num < order; num++)
            {
                qlsp[num] = lsp[num];
            }
            weight[0] = 1f / (qlsp[1] - qlsp[0]);
            weight[order - 1] = 1f / (qlsp[order - 1] - qlsp[order - 2]);
            for (num = 1; num < (order - 1); num++)
            {
                weight[num] = Math.Max((float) (1f / (qlsp[num] - qlsp[num - 1])), (float) (1f / (qlsp[num + 1] - qlsp[num])));
            }
            for (num = 0; num < order; num++)
            {
                qlsp[num] -= (0.3125f * num) + 0.75f;
            }
            for (num = 0; num < order; num++)
            {
                qlsp[num] *= 256f;
            }
            int data = LspQuant.Lsp_quant(qlsp, 0, Codebook_Constants.high_lsp_cdbk, 0x40, order);
            bits.Pack(data, 6);
            for (num = 0; num < order; num++)
            {
                qlsp[num] *= 2f;
            }
            data = LspQuant.Lsp_weight_quant(qlsp, 0, weight, 0, Codebook_Constants.high_lsp_cdbk2, 0x40, order);
            bits.Pack(data, 6);
            for (num = 0; num < order; num++)
            {
                qlsp[num] *= 0.0019531f;
            }
            for (num = 0; num < order; num++)
            {
                qlsp[num] = lsp[num] - qlsp[num];
            }
        }

        public sealed override void Unquant(float[] lsp, int order, Bits bits)
        {
            for (int i = 0; i < order; i++)
            {
                lsp[i] = (0.3125f * i) + 0.75f;
            }
            base.UnpackPlus(lsp, Codebook_Constants.high_lsp_cdbk, bits, 0.0039062f, order, 0);
            base.UnpackPlus(lsp, Codebook_Constants.high_lsp_cdbk2, bits, 0.0019531f, order, 0);
        }
    }
}

