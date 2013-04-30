namespace Ozeki.Media.Codec.Speex.Implementation
{
    using System;

    internal class Vbr
    {
        private float accum_sum = 0f;
        private float average_energy = 0f;
        private int consec_noise;
        private float energy_alpha = 0.1f;
        public static readonly float[][] hb_thresh = new float[][] { new float[] { -1f, -1f, -1f, -1f, -1f, -1f, -1f, -1f, -1f, -1f, -1f }, new float[] { -1f, -1f, -1f, -1f, -1f, -1f, -1f, -1f, -1f, -1f, -1f }, new float[] { 11f, 11f, 9.5f, 8.5f, 7.5f, 6f, 5f, 3.9f, 3f, 2f, 1f }, new float[] { 11f, 11f, 11f, 11f, 11f, 9.5f, 8.7f, 7.8f, 7f, 6.5f, 4f }, new float[] { 11f, 11f, 11f, 11f, 11f, 11f, 11f, 11f, 9.8f, 7.5f, 5.5f } };
        private float last_energy = 1f;
        private float[] last_log_energy;
        private float last_pitch_coef = 0f;
        private float last_quality = 0f;
        private const int MIN_ENERGY = 0x1770;
        public static readonly float[][] nb_thresh = new float[][] { new float[] { -1f, -1f, -1f, -1f, -1f, -1f, -1f, -1f, -1f, -1f, -1f }, new float[] { 3.5f, 2.5f, 2f, 1.2f, 0.5f, 0f, -0.5f, -0.7f, -0.8f, -0.9f, -1f }, new float[] { 10f, 6.5f, 5.2f, 4.5f, 3.9f, 3.5f, 3f, 2.5f, 2.3f, 1.8f, 1f }, new float[] { 11f, 8.8f, 7.5f, 6.5f, 5f, 3.9f, 3.9f, 3.9f, 3.5f, 3f, 1f }, new float[] { 11f, 11f, 9.9f, 9f, 8f, 7f, 6.5f, 6f, 5f, 4f, 2f }, new float[] { 11f, 11f, 11f, 11f, 9.5f, 9f, 8f, 7f, 6.5f, 5f, 3f }, new float[] { 11f, 11f, 11f, 11f, 11f, 11f, 9.5f, 8.5f, 8f, 6.5f, 4f }, new float[] { 11f, 11f, 11f, 11f, 11f, 11f, 11f, 11f, 9.8f, 7.5f, 5.5f }, new float[] { 8f, 5f, 3.7f, 3f, 2.5f, 2f, 1.8f, 1.5f, 1f, 0f, 0f } };
        private float noise_accum = ((float) (0.05 * Math.Pow(6000.0, 0.30000001192092896)));
        private float noise_accum_count = 0.05f;
        private float noise_level;
        private const float NOISE_POW = 0.3f;
        private float soft_pitch = 0f;
        public static readonly float[][] uhb_thresh = new float[][] { new float[] { -1f, -1f, -1f, -1f, -1f, -1f, -1f, -1f, -1f, -1f, -1f }, new float[] { 3.9f, 2.5f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, -1f } };
        private const int VBR_MEMORY_SIZE = 5;

        public Vbr()
        {
            this.noise_level = this.noise_accum / this.noise_accum_count;
            this.consec_noise = 0;
            this.last_log_energy = new float[5];
            for (int i = 0; i < 5; i++)
            {
                this.last_log_energy[i] = (float) Math.Log(6000.0);
            }
        }

        public float Analysis(float[] sig, int len, int pitch, float pitch_coef)
        {
            int num;
            float num2 = 0f;
            float num3 = 0f;
            float num4 = 0f;
            float num5 = 7f;
            float num7 = 0f;
            for (num = 0; num < (len >> 1); num++)
            {
                num3 += sig[num] * sig[num];
            }
            for (num = len >> 1; num < len; num++)
            {
                num4 += sig[num] * sig[num];
            }
            num2 = num3 + num4;
            float num6 = (float) Math.Log((double) (num2 + 6000f));
            for (num = 0; num < 5; num++)
            {
                num7 += (num6 - this.last_log_energy[num]) * (num6 - this.last_log_energy[num]);
            }
            num7 /= 150f;
            if (num7 > 1f)
            {
                num7 = 1f;
            }
            float num8 = (3f * (pitch_coef - 0.4f)) * Math.Abs((float) (pitch_coef - 0.4f));
            this.average_energy = ((1f - this.energy_alpha) * this.average_energy) + (this.energy_alpha * num2);
            this.noise_level = this.noise_accum / this.noise_accum_count;
            float num9 = (float) Math.Pow((double) num2, 0.30000001192092896);
            if ((this.noise_accum_count < 0.06f) && (num2 > 6000f))
            {
                this.noise_accum = 0.05f * num9;
            }
            if (((((num8 < 0.3f) && (num7 < 0.2f)) && (num9 < (1.2f * this.noise_level))) || (((num8 < 0.3f) && (num7 < 0.05f)) && (num9 < (1.5f * this.noise_level)))) || ((((num8 < 0.4f) && (num7 < 0.05f)) && (num9 < (1.2f * this.noise_level))) || ((num8 < 0f) && (num7 < 0.05f))))
            {
                float num10;
                this.consec_noise++;
                if (num9 > (3f * this.noise_level))
                {
                    num10 = 3f * this.noise_level;
                }
                else
                {
                    num10 = num9;
                }
                if (this.consec_noise >= 4)
                {
                    this.noise_accum = (0.95f * this.noise_accum) + (0.05f * num10);
                    this.noise_accum_count = (0.95f * this.noise_accum_count) + 0.05f;
                }
            }
            else
            {
                this.consec_noise = 0;
            }
            if ((num9 < this.noise_level) && (num2 > 6000f))
            {
                this.noise_accum = (0.95f * this.noise_accum) + (0.05f * num9);
                this.noise_accum_count = (0.95f * this.noise_accum_count) + 0.05f;
            }
            if (num2 < 30000f)
            {
                num5 -= 0.7f;
                if (num2 < 10000f)
                {
                    num5 -= 0.7f;
                }
                if (num2 < 3000f)
                {
                    num5 -= 0.7f;
                }
            }
            else
            {
                float num11 = (float) Math.Log((double) ((num2 + 1f) / (1f + this.last_energy)));
                float num12 = (float) Math.Log((double) ((num2 + 1f) / (1f + this.average_energy)));
                if (num12 < -5f)
                {
                    num12 = -5f;
                }
                if (num12 > 2f)
                {
                    num12 = 2f;
                }
                if (num12 > 0f)
                {
                    num5 += 0.6f * num12;
                }
                if (num12 < 0f)
                {
                    num5 += 0.5f * num12;
                }
                if (num11 > 0f)
                {
                    if (num11 > 5f)
                    {
                        num11 = 5f;
                    }
                    num5 += 0.5f * num11;
                }
                if (num4 > (1.6f * num3))
                {
                    num5 += 0.5f;
                }
            }
            this.last_energy = num2;
            this.soft_pitch = (0.6f * this.soft_pitch) + (0.4f * pitch_coef);
            num5 += (float) (2.2000000476837158 * ((pitch_coef - 0.4) + (this.soft_pitch - 0.4)));
            if (num5 < this.last_quality)
            {
                num5 = (0.5f * num5) + (0.5f * this.last_quality);
            }
            if (num5 < 4f)
            {
                num5 = 4f;
            }
            if (num5 > 10f)
            {
                num5 = 10f;
            }
            if (this.consec_noise >= 3)
            {
                num5 = 4f;
            }
            if (this.consec_noise != 0)
            {
                num5 -= (float) (1.0 * (Math.Log(3.0 + this.consec_noise) - Math.Log(3.0)));
            }
            if (num5 < 0f)
            {
                num5 = 0f;
            }
            if (num2 < 60000f)
            {
                if (this.consec_noise > 2)
                {
                    num5 -= (float) (0.5 * (Math.Log(3.0 + this.consec_noise) - Math.Log(3.0)));
                }
                if ((num2 < 10000f) && (this.consec_noise > 2))
                {
                    num5 -= (float) (0.5 * (Math.Log(3.0 + this.consec_noise) - Math.Log(3.0)));
                }
                if (num5 < 0f)
                {
                    num5 = 0f;
                }
                num5 += (float) (0.3 * Math.Log(((double) num2) / 60000.0));
            }
            if (num5 < -1f)
            {
                num5 = -1f;
            }
            this.last_pitch_coef = pitch_coef;
            this.last_quality = num5;
            for (num = 4; num > 0; num--)
            {
                this.last_log_energy[num] = this.last_log_energy[num - 1];
            }
            this.last_log_energy[0] = num6;
            return num5;
        }
    }
}

