namespace Ozeki.Media.Codec.Speex.Implementation
{
    using System;

    internal class Lsp
    {
        private float[] pw = new float[0x2a];

        public static float Cheb_poly_eva(float[] coef, float x, int m)
        {
            int index = m >> 1;
            float[] numArray = new float[index + 1];
            numArray[0] = 1f;
            numArray[1] = x;
            float num2 = coef[index] + (coef[index - 1] * x);
            x *= 2f;
            for (int i = 2; i <= index; i++)
            {
                numArray[i] = (x * numArray[i - 1]) - numArray[i - 2];
                num2 += coef[index - i] * numArray[i];
            }
            return num2;
        }

        public static void Enforce_margin(float[] lsp, int len, float margin)
        {
            if (lsp[0] < margin)
            {
                lsp[0] = margin;
            }
            if (lsp[len - 1] > (3.141593f - margin))
            {
                lsp[len - 1] = 3.141593f - margin;
            }
            for (int i = 1; i < (len - 1); i++)
            {
                if (lsp[i] < (lsp[i - 1] + margin))
                {
                    lsp[i] = lsp[i - 1] + margin;
                }
                if (lsp[i] > (lsp[i + 1] - margin))
                {
                    lsp[i] = 0.5f * ((lsp[i] + lsp[i + 1]) - margin);
                }
            }
        }

        public static int Lpc2lsp(float[] a, int lpcrdr, float[] freq, int nb, float delta)
        {
            int num9;
            float x = 0f;
            int num18 = 0;
            int num12 = 1;
            int num11 = lpcrdr / 2;
            float[] numArray = new float[num11 + 1];
            float[] numArray2 = new float[num11 + 1];
            int index = 0;
            int num15 = 0;
            int num16 = index;
            int num17 = num15;
            numArray2[index++] = 1f;
            numArray[num15++] = 1f;
            for (num9 = 1; num9 <= num11; num9++)
            {
                numArray2[index++] = (a[num9] + a[(lpcrdr + 1) - num9]) - numArray2[num16++];
                numArray[num15++] = (a[num9] - a[(lpcrdr + 1) - num9]) + numArray[num17++];
            }
            index = 0;
            num15 = 0;
            for (num9 = 0; num9 < num11; num9++)
            {
                numArray2[index] = 2f * numArray2[index];
                numArray[num15] = 2f * numArray[num15];
                index++;
                num15++;
            }
            index = 0;
            num15 = 0;
            float num6 = 0f;
            float num5 = 1f;
            for (int i = 0; i < lpcrdr; i++)
            {
                float[] numArray3;
                if ((i % 2) != 0)
                {
                    numArray3 = numArray;
                }
                else
                {
                    numArray3 = numArray2;
                }
                float num = Cheb_poly_eva(numArray3, num5, lpcrdr);
                num12 = 1;
                while ((num12 == 1) && (num6 >= -1.0))
                {
                    float num19 = (float) (delta * (1.0 - ((0.9 * num5) * num5)));
                    if (Math.Abs(num) < 0.2)
                    {
                        float? nullable = 0.5f;
                        num19 *= nullable.Value;
                    }
                    num6 = num5 - num19;
                    float num2 = Cheb_poly_eva(numArray3, num6, lpcrdr);
                    float num8 = num2;
                    float num4 = num6;
                    if ((num2 * num) < 0.0)
                    {
                        num18++;
                        float num3 = num;
                        for (int j = 0; j <= nb; j++)
                        {
                            x = (num5 + num6) / 2f;
                            num3 = Cheb_poly_eva(numArray3, x, lpcrdr);
                            if ((num3 * num) > 0.0)
                            {
                                num = num3;
                                num5 = x;
                            }
                            else
                            {
                                num2 = num3;
                                num6 = x;
                            }
                        }
                        freq[i] = x;
                        num5 = x;
                        num12 = 0;
                    }
                    else
                    {
                        num = num8;
                        num5 = num4;
                    }
                }
            }
            return num18;
        }

        public void Lsp2lpc(float[] freq, float[] ak, int lpcrdr)
        {
            int index = 0;
            int num11 = lpcrdr / 2;
            int num = 0;
            while (num < ((4 * num11) + 2))
            {
                this.pw[num] = 0f;
                num++;
            }
            float num5 = 1f;
            float num6 = 1f;
            for (int i = 0; i <= lpcrdr; i++)
            {
                float num3;
                float num4;
                int num12 = 0;
                num = 0;
                while (num < num11)
                {
                    int num7 = num * 4;
                    int num8 = num7 + 1;
                    int num9 = num8 + 1;
                    index = num9 + 1;
                    num3 = (num5 - ((2f * freq[num12]) * this.pw[num7])) + this.pw[num8];
                    num4 = (num6 - ((2f * freq[num12 + 1]) * this.pw[num9])) + this.pw[index];
                    this.pw[num8] = this.pw[num7];
                    this.pw[index] = this.pw[num9];
                    this.pw[num7] = num5;
                    this.pw[num9] = num6;
                    num5 = num3;
                    num6 = num4;
                    num++;
                    num12 += 2;
                }
                num3 = num5 + this.pw[index + 1];
                num4 = num6 - this.pw[index + 2];
                ak[i] = (num3 + num4) * 0.5f;
                this.pw[index + 1] = num5;
                this.pw[index + 2] = num6;
                num5 = 0f;
                num6 = 0f;
            }
        }
    }
}

