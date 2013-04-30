namespace Ozeki.Media.Codec.Speex.Implementation
{
    using System;

    internal class SbDecoder : SbCodec, IDecoder
    {
        protected internal bool enhanced;
        private float[] innov2;
        protected internal IDecoder lowdec;
        protected internal Stereo stereo;

        public SbDecoder(bool ultraWide) : base(ultraWide)
        {
            this.stereo = new Stereo();
            this.enhanced = true;
            if (ultraWide)
            {
                this.Uwbinit();
            }
            else
            {
                this.Wbinit();
            }
        }

        public virtual int Decode(Bits bits, float[] xout)
        {
            int num4 = this.lowdec.Decode(bits, base.x0d);
            if (num4 != 0)
            {
                return num4;
            }
            bool dtx = this.lowdec.Dtx;
            if (bits == null)
            {
                this.DecodeLost(xout, dtx);
                return 0;
            }
            if (bits.Peek() != 0)
            {
                int num3 = bits.Unpack(1);
                base.submodeID = bits.Unpack(3);
            }
            else
            {
                base.submodeID = 0;
            }
            int index = 0;
            while (index < base.frameSize)
            {
                base.excBuf[index] = 0f;
                index++;
            }
            if (base.submodes[base.submodeID] == null)
            {
                if (dtx)
                {
                    this.DecodeLost(xout, true);
                    return 0;
                }
                for (index = 0; index < base.frameSize; index++)
                {
                    base.excBuf[index] = 0f;
                }
                base.first = 1;
                Filters.Iir_mem2(base.excBuf, base.excIdx, base.interp_qlpc, base.high, 0, base.frameSize, base.lpcSize, base.mem_sp);
                base.filters.Fir_mem_up(base.x0d, Codebook_Constants.h0, base.y0, base.fullFrameSize, 0x40, base.g0_mem);
                base.filters.Fir_mem_up(base.high, Codebook_Constants.h1, base.y1, base.fullFrameSize, 0x40, base.g1_mem);
                for (index = 0; index < base.fullFrameSize; index++)
                {
                    xout[index] = 2f * (base.y0[index] - base.y1[index]);
                }
                return 0;
            }
            float[] piGain = this.lowdec.PiGain;
            float[] exc = this.lowdec.Exc;
            float[] innov = this.lowdec.Innov;
            base.submodes[base.submodeID].LsqQuant.Unquant(base.qlsp, base.lpcSize, bits);
            if (base.first != 0)
            {
                index = 0;
                while (index < base.lpcSize)
                {
                    base.old_qlsp[index] = base.qlsp[index];
                    index++;
                }
            }
            for (int i = 0; i < base.nbSubframes; i++)
            {
                float num7 = 0f;
                float num8 = 0f;
                float num9 = 0f;
                int es = base.subframeSize * i;
                float num5 = (1f + i) / ((float) base.nbSubframes);
                index = 0;
                while (index < base.lpcSize)
                {
                    base.interp_qlsp[index] = ((1f - num5) * base.old_qlsp[index]) + (num5 * base.qlsp[index]);
                    index++;
                }
                Lsp.Enforce_margin(base.interp_qlsp, base.lpcSize, 0.05f);
                index = 0;
                while (index < base.lpcSize)
                {
                    base.interp_qlsp[index] = (float) Math.Cos((double) base.interp_qlsp[index]);
                    index++;
                }
                base.m_lsp.Lsp2lpc(base.interp_qlsp, base.interp_qlpc, base.lpcSize);
                if (this.enhanced)
                {
                    float gamma = base.submodes[base.submodeID].LpcEnhK1;
                    float num12 = base.submodes[base.submodeID].LpcEnhK2;
                    float num13 = gamma - num12;
                    Filters.Bw_lpc(gamma, base.interp_qlpc, base.awk1, base.lpcSize);
                    Filters.Bw_lpc(num12, base.interp_qlpc, base.awk2, base.lpcSize);
                    Filters.Bw_lpc(num13, base.interp_qlpc, base.awk3, base.lpcSize);
                }
                num5 = 1f;
                base.pi_gain[i] = 0f;
                index = 0;
                while (index <= base.lpcSize)
                {
                    num9 += num5 * base.interp_qlpc[index];
                    num5 = -num5;
                    base.pi_gain[i] += base.interp_qlpc[index];
                    index++;
                }
                num8 = piGain[i];
                num8 = 1f / (Math.Abs(num8) + 0.01f);
                num9 = 1f / (Math.Abs(num9) + 0.01f);
                float num6 = Math.Abs((float) (0.01f + num9)) / (0.01f + Math.Abs(num8));
                index = es;
                while (index < (es + base.subframeSize))
                {
                    base.excBuf[index] = 0f;
                    index++;
                }
                if (base.submodes[base.submodeID].Innovation == null)
                {
                    float num14 = (float) Math.Exp((bits.Unpack(5) - 10.0) / 8.0);
                    num14 /= num6;
                    index = es;
                    while (index < (es + base.subframeSize))
                    {
                        base.excBuf[index] = (base.foldingGain * num14) * innov[index];
                        index++;
                    }
                }
                else
                {
                    int num18 = bits.Unpack(4);
                    index = es;
                    while (index < (es + base.subframeSize))
                    {
                        num7 += exc[index] * exc[index];
                        index++;
                    }
                    float num16 = (float) Math.Exp((double) ((0.2702703f * num18) - 2f));
                    float num17 = (num16 * ((float) Math.Sqrt((double) (1f + num7)))) / num6;
                    base.submodes[base.submodeID].Innovation.Unquantify(base.excBuf, es, base.subframeSize, bits);
                    index = es;
                    while (index < (es + base.subframeSize))
                    {
                        base.excBuf[index] *= num17;
                        index++;
                    }
                    if (base.submodes[base.submodeID].DoubleCodebook != 0)
                    {
                        index = 0;
                        while (index < base.subframeSize)
                        {
                            this.innov2[index] = 0f;
                            index++;
                        }
                        base.submodes[base.submodeID].Innovation.Unquantify(this.innov2, 0, base.subframeSize, bits);
                        index = 0;
                        while (index < base.subframeSize)
                        {
                            this.innov2[index] *= num17 * 0.4f;
                            index++;
                        }
                        index = 0;
                        while (index < base.subframeSize)
                        {
                            base.excBuf[es + index] += this.innov2[index];
                            index++;
                        }
                    }
                }
                index = es;
                while (index < (es + base.subframeSize))
                {
                    base.high[index] = base.excBuf[index];
                    index++;
                }
                if (this.enhanced)
                {
                    Filters.Filter_mem2(base.high, es, base.awk2, base.awk1, base.subframeSize, base.lpcSize, base.mem_sp, base.lpcSize);
                    Filters.Filter_mem2(base.high, es, base.awk3, base.interp_qlpc, base.subframeSize, base.lpcSize, base.mem_sp, 0);
                }
                else
                {
                    index = 0;
                    while (index < base.lpcSize)
                    {
                        base.mem_sp[base.lpcSize + index] = 0f;
                        index++;
                    }
                    Filters.Iir_mem2(base.high, es, base.interp_qlpc, base.high, es, base.subframeSize, base.lpcSize, base.mem_sp);
                }
            }
            base.filters.Fir_mem_up(base.x0d, Codebook_Constants.h0, base.y0, base.fullFrameSize, 0x40, base.g0_mem);
            base.filters.Fir_mem_up(base.high, Codebook_Constants.h1, base.y1, base.fullFrameSize, 0x40, base.g1_mem);
            for (index = 0; index < base.fullFrameSize; index++)
            {
                xout[index] = 2f * (base.y0[index] - base.y1[index]);
            }
            for (index = 0; index < base.lpcSize; index++)
            {
                base.old_qlsp[index] = base.qlsp[index];
            }
            base.first = 0;
            return 0;
        }

        public int DecodeLost(float[] xout, bool dtx)
        {
            int num;
            int submodeID = 0;
            if (dtx)
            {
                submodeID = base.submodeID;
                base.submodeID = 1;
            }
            else
            {
                Filters.Bw_lpc(0.99f, base.interp_qlpc, base.interp_qlpc, base.lpcSize);
            }
            base.first = 1;
            base.awk1 = new float[base.lpcSize + 1];
            base.awk2 = new float[base.lpcSize + 1];
            base.awk3 = new float[base.lpcSize + 1];
            if (this.enhanced)
            {
                float num3;
                float num4;
                if (base.submodes[base.submodeID] != null)
                {
                    num3 = base.submodes[base.submodeID].LpcEnhK1;
                    num4 = base.submodes[base.submodeID].LpcEnhK2;
                }
                else
                {
                    num3 = num4 = 0.7f;
                }
                float gamma = num3 - num4;
                Filters.Bw_lpc(num3, base.interp_qlpc, base.awk1, base.lpcSize);
                Filters.Bw_lpc(num4, base.interp_qlpc, base.awk2, base.lpcSize);
                Filters.Bw_lpc(gamma, base.interp_qlpc, base.awk3, base.lpcSize);
            }
            if (!dtx)
            {
                for (num = 0; num < base.frameSize; num++)
                {
                    float? nullable = 0.9f;
                    base.excBuf[base.excIdx + num] *= nullable.Value;
                }
            }
            num = 0;
            while (num < base.frameSize)
            {
                base.high[num] = base.excBuf[base.excIdx + num];
                num++;
            }
            if (this.enhanced)
            {
                Filters.Filter_mem2(base.high, 0, base.awk2, base.awk1, base.high, 0, base.frameSize, base.lpcSize, base.mem_sp, base.lpcSize);
                Filters.Filter_mem2(base.high, 0, base.awk3, base.interp_qlpc, base.high, 0, base.frameSize, base.lpcSize, base.mem_sp, 0);
            }
            else
            {
                for (num = 0; num < base.lpcSize; num++)
                {
                    base.mem_sp[base.lpcSize + num] = 0f;
                }
                Filters.Iir_mem2(base.high, 0, base.interp_qlpc, base.high, 0, base.frameSize, base.lpcSize, base.mem_sp);
            }
            base.filters.Fir_mem_up(base.x0d, Codebook_Constants.h0, base.y0, base.fullFrameSize, 0x40, base.g0_mem);
            base.filters.Fir_mem_up(base.high, Codebook_Constants.h1, base.y1, base.fullFrameSize, 0x40, base.g1_mem);
            for (num = 0; num < base.fullFrameSize; num++)
            {
                xout[num] = 2f * (base.y0[num] - base.y1[num]);
            }
            if (dtx)
            {
                base.submodeID = submodeID;
            }
            return 0;
        }

        public virtual void DecodeStereo(float[] data, int frameSize)
        {
            this.stereo.Decode(data, frameSize);
        }

        protected override void Init(int frameSize, int subframeSize, int lpcSize, int bufSize, float foldingGain)
        {
            base.Init(frameSize, subframeSize, lpcSize, bufSize, foldingGain);
            base.excIdx = 0;
            this.innov2 = new float[subframeSize];
        }

        private void Uwbinit()
        {
            this.lowdec = new SbDecoder(false);
            this.lowdec.PerceptualEnhancement = this.enhanced;
            this.Init(320, 80, 8, 0x500, 0.5f);
        }

        private void Wbinit()
        {
            this.lowdec = new NbDecoder();
            this.lowdec.PerceptualEnhancement = this.enhanced;
            this.Init(160, 40, 8, 640, 0.7f);
        }

        public virtual bool PerceptualEnhancement
        {
            get
            {
                return this.enhanced;
            }
            set
            {
                this.enhanced = value;
            }
        }
    }
}

