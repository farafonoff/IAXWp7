namespace Ozeki.Media.Codec.Speex.Implementation
{
    using System;

    internal class NoiseSearch : CodebookSearch
    {
        public sealed override void Quantify(float[] target, float[] ak, float[] awk1, float[] awk2, int p, int nsf, float[] exc, int es, float[] r, Bits bits, int complexity)
        {
            int num;
            float[] y = new float[nsf];
            Filters.Residue_percep_zero(target, 0, ak, awk1, awk2, y, nsf, p);
            for (num = 0; num < nsf; num++)
            {
                exc[es + num] += y[num];
            }
            for (num = 0; num < nsf; num++)
            {
                target[num] = 0f;
            }
        }

        public sealed override void Unquantify(float[] exc, int es, int nsf, Bits bits)
        {
            for (int i = 0; i < nsf; i++)
            {
                exc[es + i] += (float) (3.0 * (new Random().NextDouble() - 0.5));
            }
        }
    }
}

