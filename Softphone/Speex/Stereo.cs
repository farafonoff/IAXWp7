namespace Ozeki.Media.Codec.Speex.Implementation
{
    using System;

    internal class Stereo
    {
        private float balance = 1f;
        private float e_ratio = 0.5f;
        private static readonly float[] e_ratio_quant = new float[] { 0.25f, 0.315f, 0.397f, 0.5f };
        private float smooth_left = 1f;
        private float smooth_right = 1f;
        private const int SPEEX_INBAND_STEREO = 9;

        public void Decode(float[] data, int frameSize)
        {
            int num;
            float num2 = 0f;
            for (num = frameSize - 1; num >= 0; num--)
            {
                num2 += data[num] * data[num];
            }
            float num5 = num2 / this.e_ratio;
            float num3 = (num5 * this.balance) / (1f + this.balance);
            float num4 = num5 - num3;
            num3 = (float) Math.Sqrt((double) (num3 / (num2 + 0.01f)));
            num4 = (float) Math.Sqrt((double) (num4 / (num2 + 0.01f)));
            for (num = frameSize - 1; num >= 0; num--)
            {
                float num6 = data[num];
                this.smooth_left = (0.98f * this.smooth_left) + (0.02f * num3);
                this.smooth_right = (0.98f * this.smooth_right) + (0.02f * num4);
                data[2 * num] = this.smooth_left * num6;
                data[(2 * num) + 1] = this.smooth_right * num6;
            }
        }

        public static void Encode(Bits bits, float[] data, int frameSize)
        {
            float num3 = 0f;
            float num4 = 0f;
            float num5 = 0f;
            for (int i = 0; i < frameSize; i++)
            {
                num3 += data[2 * i] * data[2 * i];
                num4 += data[(2 * i) + 1] * data[(2 * i) + 1];
                data[i] = 0.5f * (data[2 * i] + data[(2 * i) + 1]);
                num5 += data[i] * data[i];
            }
            float num6 = (num3 + 1f) / (num4 + 1f);
            float num7 = num5 / ((1f + num3) + num4);
            bits.Pack(14, 5);
            bits.Pack(9, 4);
            num6 = (float) (4.0 * Math.Log((double) num6));
            if (num6 > 0f)
            {
                bits.Pack(0, 1);
            }
            else
            {
                bits.Pack(1, 1);
            }
            num6 = (float) Math.Floor((double) (0.5f + Math.Abs(num6)));
            if (num6 > 30f)
            {
                num6 = 31f;
            }
            bits.Pack((int) num6, 5);
            int num2 = VQ.Index(num7, e_ratio_quant, 4);
            bits.Pack(num2, 2);
        }

        public void Init(Bits bits)
        {
            float num = 1f;
            if (bits.Unpack(1) != 0)
            {
                num = -1f;
            }
            int index = bits.Unpack(5);
            this.balance = (float) Math.Exp((num * 0.25) * index);
            index = bits.Unpack(2);
            this.e_ratio = e_ratio_quant[index];
        }
    }
}

