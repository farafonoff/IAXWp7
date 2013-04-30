namespace Ozeki.Media.Codec.Speex.Implementation
{
    using System;

    internal class LbrLspQuant : LspQuant
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
                float num2 = 1f / (((0.15f + qlsp[num]) - qlsp[num - 1]) * ((0.15f + qlsp[num]) - qlsp[num - 1]));
                float num3 = 1f / (((0.15f + qlsp[num + 1]) - qlsp[num]) * ((0.15f + qlsp[num + 1]) - qlsp[num]));
                weight[num] = (num2 > num3) ? num2 : num3;
            }
            for (num = 0; num < order; num++)
            {
                float? nullable = new float?((float) ((0.25 * num) + 0.25));
                qlsp[num] -= nullable.Value;
            }
            for (num = 0; num < order; num++)
            {
                qlsp[num] *= 256f;
            }
            int data = LspQuant.Lsp_quant(qlsp, 0, Codebook_Constants.cdbk_nb, 0x40, order);
            bits.Pack(data, 6);
            for (num = 0; num < order; num++)
            {
                qlsp[num] *= 2f;
            }
            data = LspQuant.Lsp_weight_quant(qlsp, 0, weight, 0, Codebook_Constants.cdbk_nb_low1, 0x40, 5);
            bits.Pack(data, 6);
            data = LspQuant.Lsp_weight_quant(qlsp, 5, weight, 5, Codebook_Constants.cdbk_nb_high1, 0x40, 5);
            bits.Pack(data, 6);
            for (num = 0; num < order; num++)
            {
                float? nullable2 = 0.0019531f;
                qlsp[num] *= nullable2.Value;
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
                lsp[i] = (0.25f * i) + 0.25f;
            }
            base.UnpackPlus(lsp, Codebook_Constants.cdbk_nb, bits, 0.0039062f, 10, 0);
            base.UnpackPlus(lsp, Codebook_Constants.cdbk_nb_low1, bits, 0.0019531f, 5, 0);
            base.UnpackPlus(lsp, Codebook_Constants.cdbk_nb_high1, bits, 0.0019531f, 5, 5);
        }
    }
}

