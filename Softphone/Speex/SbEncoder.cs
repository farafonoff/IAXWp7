namespace Ozeki.Media.Codec.Speex.Implementation
{
    using System;

    internal class SbEncoder : SbCodec, IEncoder
    {
        protected internal float abr_count;
        protected internal float abr_drift;
        protected internal float abr_drift2;
        protected internal int abr_enabled;
        private float[] autocorr;
        private float[] buf;
        private float[] bw_lpc1;
        private float[] bw_lpc2;
        protected internal int complexity;
        private float[] h0_mem;
        private float[] interp_lpc;
        private float[] interp_lsp;
        private float[] lagWindow;
        protected internal IEncoder lowenc;
        private float[] lsp;
        private float[] mem_sp2;
        private float[] mem_sw;
        protected internal int nb_modes;
        private static readonly int[] NB_QUALITY_MAP = new int[] { 1, 8, 2, 3, 4, 5, 5, 6, 6, 7, 7 };
        private float[] old_lsp;
        private float[] rc;
        protected internal float relative_quality;
        private float[] res;
        protected internal int sampling_rate;
        protected internal int submodeSelect;
        private float[] swBuf;
        private float[] target;
        private bool uwb;
        private static readonly int[] UWB_QUALITY_MAP = new int[] { 0, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 };
        protected internal int vad_enabled;
        protected internal int vbr_enabled;
        protected internal float vbr_quality;
        private static readonly int[] WB_QUALITY_MAP = new int[] { 1, 1, 1, 1, 1, 1, 2, 2, 3, 3, 4 };
        private float[] window;
        private float[] x1d;

        public SbEncoder(bool ultraWide) : base(ultraWide)
        {
            if (ultraWide)
            {
                this.Uwbinit();
            }
            else
            {
                this.Wbinit();
            }
        }

        public virtual int Encode(Bits bits, float[] ins0)
        {
            int num;
            int num2;
            Filters.Qmf_decomp(ins0, Codebook_Constants.h0, base.x0d, this.x1d, base.fullFrameSize, 0x40, this.h0_mem);
            this.lowenc.Encode(bits, base.x0d);
            for (num = 0; num < (base.windowSize - base.frameSize); num++)
            {
                base.high[num] = base.high[base.frameSize + num];
            }
            for (num = 0; num < base.frameSize; num++)
            {
                base.high[(base.windowSize - base.frameSize) + num] = this.x1d[num];
            }
            Array.Copy(base.excBuf, base.frameSize, base.excBuf, 0, base.bufSize - base.frameSize);
            float[] piGain = this.lowenc.PiGain;
            float[] exc = this.lowenc.Exc;
            float[] innov = this.lowenc.Innov;
            if (this.lowenc.Mode == 0)
            {
                num2 = 1;
            }
            else
            {
                num2 = 0;
            }
            num = 0;
            while (num < base.windowSize)
            {
                this.buf[num] = base.high[num] * this.window[num];
                num++;
            }
            Lpc.Autocorr(this.buf, this.autocorr, base.lpcSize + 1, base.windowSize);
            this.autocorr[0]++;
            this.autocorr[0] *= base.lpc_floor;
            for (num = 0; num < (base.lpcSize + 1); num++)
            {
                this.autocorr[num] *= this.lagWindow[num];
            }
            Lpc.Wld(base.lpc, this.autocorr, this.rc, base.lpcSize);
            Array.Copy(base.lpc, 0, base.lpc, 1, base.lpcSize);
            base.lpc[0] = 1f;
            if ((Lsp.Lpc2lsp(base.lpc, base.lpcSize, this.lsp, 15, 0.2f) != base.lpcSize) && (Lsp.Lpc2lsp(base.lpc, base.lpcSize, this.lsp, 11, 0.02f) != base.lpcSize))
            {
                for (num = 0; num < base.lpcSize; num++)
                {
                    this.lsp[num] = (float) Math.Cos((3.1415926535897931 * (num + 1)) / ((double) (base.lpcSize + 1)));
                }
            }
            num = 0;
            while (num < base.lpcSize)
            {
                this.lsp[num] = (float) Math.Acos((double) this.lsp[num]);
                num++;
            }
            float num5 = 0f;
            for (num = 0; num < base.lpcSize; num++)
            {
                num5 += (this.old_lsp[num] - this.lsp[num]) * (this.old_lsp[num] - this.lsp[num]);
            }
            if (((this.vbr_enabled != 0) || (this.vad_enabled != 0)) && (num2 == 0))
            {
                float num6 = 0f;
                float num7 = 0f;
                if (this.abr_enabled != 0)
                {
                    float num9 = 0f;
                    if ((this.abr_drift2 * this.abr_drift) > 0f)
                    {
                        num9 = (-1E-05f * this.abr_drift) / (1f + this.abr_count);
                        if (num9 > 0.1f)
                        {
                            num9 = 0.1f;
                        }
                        if (num9 < -0.1f)
                        {
                            num9 = -0.1f;
                        }
                    }
                    this.vbr_quality += num9;
                    if (this.vbr_quality > 10f)
                    {
                        this.vbr_quality = 10f;
                    }
                    if (this.vbr_quality < 0f)
                    {
                        this.vbr_quality = 0f;
                    }
                }
                num = 0;
                while (num < base.frameSize)
                {
                    num6 += base.x0d[num] * base.x0d[num];
                    num7 += base.high[num] * base.high[num];
                    num++;
                }
                float num8 = (float) Math.Log((double) ((1f + num7) / (1f + num6)));
                this.relative_quality = this.lowenc.RelativeQuality;
                if (num8 < -4f)
                {
                    num8 = -4f;
                }
                if (num8 > 2f)
                {
                    num8 = 2f;
                }
                if (this.vbr_enabled == 0)
                {
                    int submodeSelect;
                    if (this.relative_quality < 2.0)
                    {
                        submodeSelect = 1;
                    }
                    else
                    {
                        submodeSelect = this.submodeSelect;
                    }
                    base.submodeID = submodeSelect;
                }
                else
                {
                    int index = this.nb_modes - 1;
                    this.relative_quality += 1f * (num8 + 2f);
                    if (this.relative_quality < -1f)
                    {
                        this.relative_quality = -1f;
                    }
                    while (index != 0)
                    {
                        float num12;
                        int num11 = (int) Math.Floor((double) this.vbr_quality);
                        if (num11 == 10)
                        {
                            num12 = Ozeki.Media.Codec.Speex.Implementation.Vbr.hb_thresh[index][num11];
                        }
                        else
                        {
                            num12 = ((this.vbr_quality - num11) * Ozeki.Media.Codec.Speex.Implementation.Vbr.hb_thresh[index][num11 + 1]) + (((1 + num11) - this.vbr_quality) * Ozeki.Media.Codec.Speex.Implementation.Vbr.hb_thresh[index][num11]);
                        }
                        if (this.relative_quality >= num12)
                        {
                            break;
                        }
                        index--;
                    }
                    this.Mode = index;
                    if (this.abr_enabled != 0)
                    {
                        int bitRate = this.BitRate;
                        this.abr_drift += bitRate - this.abr_enabled;
                        this.abr_drift2 = (0.95f * this.abr_drift2) + (0.05f * (bitRate - this.abr_enabled));
                        this.abr_count++;
                    }
                }
            }
            bits.Pack(1, 1);
            if (num2 != 0)
            {
                bits.Pack(0, 3);
            }
            else
            {
                bits.Pack(base.submodeID, 3);
            }
            if ((num2 != 0) || (base.submodes[base.submodeID] == null))
            {
                for (num = 0; num < base.frameSize; num++)
                {
                    base.excBuf[base.excIdx + num] = this.swBuf[num] = 0f;
                }
                for (num = 0; num < base.lpcSize; num++)
                {
                    this.mem_sw[num] = 0f;
                }
                base.first = 1;
                Filters.Iir_mem2(base.excBuf, base.excIdx, base.interp_qlpc, base.high, 0, base.subframeSize, base.lpcSize, base.mem_sp);
                base.filters.Fir_mem_up(base.x0d, Codebook_Constants.h0, base.y0, base.fullFrameSize, 0x40, base.g0_mem);
                base.filters.Fir_mem_up(base.high, Codebook_Constants.h1, base.y1, base.fullFrameSize, 0x40, base.g1_mem);
                for (num = 0; num < base.fullFrameSize; num++)
                {
                    ins0[num] = 2f * (base.y0[num] - base.y1[num]);
                }
                if (num2 != 0)
                {
                    return 0;
                }
                return 1;
            }
            base.submodes[base.submodeID].LsqQuant.Quant(this.lsp, base.qlsp, base.lpcSize, bits);
            if (base.first != 0)
            {
                for (num = 0; num < base.lpcSize; num++)
                {
                    this.old_lsp[num] = this.lsp[num];
                }
                num = 0;
                while (num < base.lpcSize)
                {
                    base.old_qlsp[num] = base.qlsp[num];
                    num++;
                }
            }
            float[] mem = new float[base.lpcSize];
            float[] y = new float[base.subframeSize];
            float[] numArray2 = new float[base.subframeSize];
            for (int i = 0; i < base.nbSubframes; i++)
            {
                float num24;
                float num25 = 0f;
                float num26 = 0f;
                int num22 = base.subframeSize * i;
                int xs = num22;
                int ys = base.excIdx + num22;
                int num21 = num22;
                int num20 = num22;
                float num16 = (1f + i) / ((float) base.nbSubframes);
                num = 0;
                while (num < base.lpcSize)
                {
                    this.interp_lsp[num] = ((1f - num16) * this.old_lsp[num]) + (num16 * this.lsp[num]);
                    num++;
                }
                num = 0;
                while (num < base.lpcSize)
                {
                    base.interp_qlsp[num] = ((1f - num16) * base.old_qlsp[num]) + (num16 * base.qlsp[num]);
                    num++;
                }
                Lsp.Enforce_margin(this.interp_lsp, base.lpcSize, 0.05f);
                Lsp.Enforce_margin(base.interp_qlsp, base.lpcSize, 0.05f);
                num = 0;
                while (num < base.lpcSize)
                {
                    this.interp_lsp[num] = (float) Math.Cos((double) this.interp_lsp[num]);
                    num++;
                }
                num = 0;
                while (num < base.lpcSize)
                {
                    base.interp_qlsp[num] = (float) Math.Cos((double) base.interp_qlsp[num]);
                    num++;
                }
                base.m_lsp.Lsp2lpc(this.interp_lsp, this.interp_lpc, base.lpcSize);
                base.m_lsp.Lsp2lpc(base.interp_qlsp, base.interp_qlpc, base.lpcSize);
                Filters.Bw_lpc(base.gamma1, this.interp_lpc, this.bw_lpc1, base.lpcSize);
                Filters.Bw_lpc(base.gamma2, this.interp_lpc, this.bw_lpc2, base.lpcSize);
                float num23 = num24 = 0f;
                num16 = 1f;
                base.pi_gain[i] = 0f;
                num = 0;
                while (num <= base.lpcSize)
                {
                    num24 += num16 * base.interp_qlpc[num];
                    num16 = -num16;
                    base.pi_gain[i] += base.interp_qlpc[num];
                    num++;
                }
                num23 = piGain[i];
                num23 = 1f / (Math.Abs(num23) + 0.01f);
                num24 = 1f / (Math.Abs(num24) + 0.01f);
                float num17 = Math.Abs((float) (0.01f + num24)) / (0.01f + Math.Abs(num23));
                Filters.Fir_mem2(base.high, xs, base.interp_qlpc, base.excBuf, ys, base.subframeSize, base.lpcSize, this.mem_sp2);
                num = 0;
                while (num < base.subframeSize)
                {
                    num25 += base.excBuf[ys + num] * base.excBuf[ys + num];
                    num++;
                }
                if (base.submodes[base.submodeID].Innovation == null)
                {
                    num = 0;
                    while (num < base.subframeSize)
                    {
                        num26 += innov[num22 + num] * innov[num22 + num];
                        num++;
                    }
                    float num27 = num25 / (0.01f + num26);
                    num27 = (float) Math.Sqrt((double) num27);
                    num27 *= num17;
                    int data = (int) Math.Floor((double) (10.5 + (8.0 * Math.Log(num27 + 0.0001))));
                    if (data < 0)
                    {
                        data = 0;
                    }
                    if (data > 0x1f)
                    {
                        data = 0x1f;
                    }
                    bits.Pack(data, 5);
                    num27 = (float) (0.1 * Math.Exp(((double) data) / 9.4));
                    num27 /= num17;
                }
                else
                {
                    num = 0;
                    while (num < base.subframeSize)
                    {
                        num26 += exc[num22 + num] * exc[num22 + num];
                        num++;
                    }
                    float num29 = (float) ((Math.Sqrt((double) (1f + num25)) * num17) / Math.Sqrt((double) ((1f + num26) * base.subframeSize)));
                    int num32 = (int) Math.Floor((double) (0.5 + (3.7 * (Math.Log((double) num29) + 2.0))));
                    if (num32 < 0)
                    {
                        num32 = 0;
                    }
                    if (num32 > 15)
                    {
                        num32 = 15;
                    }
                    bits.Pack(num32, 4);
                    num29 = (float) Math.Exp((0.27027027027027023 * num32) - 2.0);
                    float num30 = (num29 * ((float) Math.Sqrt((double) (1f + num26)))) / num17;
                    float num31 = 1f / num30;
                    num = 0;
                    while (num < base.subframeSize)
                    {
                        base.excBuf[ys + num] = 0f;
                        num++;
                    }
                    base.excBuf[ys] = 1f;
                    Filters.Syn_percep_zero(base.excBuf, ys, base.interp_qlpc, this.bw_lpc1, this.bw_lpc2, y, base.subframeSize, base.lpcSize);
                    num = 0;
                    while (num < base.subframeSize)
                    {
                        base.excBuf[ys + num] = 0f;
                        num++;
                    }
                    num = 0;
                    while (num < base.lpcSize)
                    {
                        mem[num] = base.mem_sp[num];
                        num++;
                    }
                    Filters.Iir_mem2(base.excBuf, ys, base.interp_qlpc, base.excBuf, ys, base.subframeSize, base.lpcSize, mem);
                    num = 0;
                    while (num < base.lpcSize)
                    {
                        mem[num] = this.mem_sw[num];
                        num++;
                    }
                    Filters.Filter_mem2(base.excBuf, ys, this.bw_lpc1, this.bw_lpc2, this.res, num21, base.subframeSize, base.lpcSize, mem, 0);
                    num = 0;
                    while (num < base.lpcSize)
                    {
                        mem[num] = this.mem_sw[num];
                        num++;
                    }
                    Filters.Filter_mem2(base.high, xs, this.bw_lpc1, this.bw_lpc2, this.swBuf, num20, base.subframeSize, base.lpcSize, mem, 0);
                    num = 0;
                    while (num < base.subframeSize)
                    {
                        this.target[num] = this.swBuf[num20 + num] - this.res[num21 + num];
                        num++;
                    }
                    num = 0;
                    while (num < base.subframeSize)
                    {
                        base.excBuf[ys + num] = 0f;
                        num++;
                    }
                    num = 0;
                    while (num < base.subframeSize)
                    {
                        this.target[num] *= num31;
                        num++;
                    }
                    num = 0;
                    while (num < base.subframeSize)
                    {
                        numArray2[num] = 0f;
                        num++;
                    }
                    base.submodes[base.submodeID].Innovation.Quantify(this.target, base.interp_qlpc, this.bw_lpc1, this.bw_lpc2, base.lpcSize, base.subframeSize, numArray2, 0, y, bits, (this.complexity + 1) >> 1);
                    num = 0;
                    while (num < base.subframeSize)
                    {
                        base.excBuf[ys + num] += numArray2[num] * num30;
                        num++;
                    }
                    if (base.submodes[base.submodeID].DoubleCodebook != 0)
                    {
                        float[] numArray7 = new float[base.subframeSize];
                        num = 0;
                        while (num < base.subframeSize)
                        {
                            numArray7[num] = 0f;
                            num++;
                        }
                        num = 0;
                        while (num < base.subframeSize)
                        {
                            this.target[num] *= 2.5f;
                            num++;
                        }
                        base.submodes[base.submodeID].Innovation.Quantify(this.target, base.interp_qlpc, this.bw_lpc1, this.bw_lpc2, base.lpcSize, base.subframeSize, numArray7, 0, y, bits, (this.complexity + 1) >> 1);
                        num = 0;
                        while (num < base.subframeSize)
                        {
                            numArray7[num] *= (float) (num30 * 0.4);
                            num++;
                        }
                        num = 0;
                        while (num < base.subframeSize)
                        {
                            base.excBuf[ys + num] += numArray7[num];
                            num++;
                        }
                    }
                }
                num = 0;
                while (num < base.lpcSize)
                {
                    mem[num] = base.mem_sp[num];
                    num++;
                }
                Filters.Iir_mem2(base.excBuf, ys, base.interp_qlpc, base.high, xs, base.subframeSize, base.lpcSize, base.mem_sp);
                Filters.Filter_mem2(base.high, xs, this.bw_lpc1, this.bw_lpc2, this.swBuf, num20, base.subframeSize, base.lpcSize, this.mem_sw, 0);
            }
            base.filters.Fir_mem_up(base.x0d, Codebook_Constants.h0, base.y0, base.fullFrameSize, 0x40, base.g0_mem);
            base.filters.Fir_mem_up(base.high, Codebook_Constants.h1, base.y1, base.fullFrameSize, 0x40, base.g1_mem);
            for (num = 0; num < base.fullFrameSize; num++)
            {
                ins0[num] = 2f * (base.y0[num] - base.y1[num]);
            }
            for (num = 0; num < base.lpcSize; num++)
            {
                this.old_lsp[num] = this.lsp[num];
            }
            for (num = 0; num < base.lpcSize; num++)
            {
                base.old_qlsp[num] = base.qlsp[num];
            }
            base.first = 0;
            return 1;
        }

        protected override void Init(int frameSize, int subframeSize, int lpcSize, int bufSize, float foldingGain)
        {
            base.Init(frameSize, subframeSize, lpcSize, bufSize, foldingGain);
            this.complexity = 3;
            this.vbr_enabled = 0;
            this.vad_enabled = 0;
            this.abr_enabled = 0;
            this.vbr_quality = 8f;
            this.submodeSelect = base.submodeID;
            this.x1d = new float[frameSize];
            this.h0_mem = new float[0x40];
            this.buf = new float[base.windowSize];
            this.swBuf = new float[frameSize];
            this.res = new float[frameSize];
            this.target = new float[subframeSize];
            this.window = Misc.Window(base.windowSize, subframeSize);
            this.lagWindow = Misc.LagWindow(lpcSize, base.lag_factor);
            this.rc = new float[lpcSize];
            this.autocorr = new float[lpcSize + 1];
            this.lsp = new float[lpcSize];
            this.old_lsp = new float[lpcSize];
            this.interp_lsp = new float[lpcSize];
            this.interp_lpc = new float[lpcSize + 1];
            this.bw_lpc1 = new float[lpcSize + 1];
            this.bw_lpc2 = new float[lpcSize + 1];
            this.mem_sp2 = new float[lpcSize];
            this.mem_sw = new float[lpcSize];
            this.abr_count = 0f;
        }

        private void Uwbinit()
        {
            this.lowenc = new SbEncoder(false);
            this.Init(320, 80, 8, 0x500, 0.7f);
            this.uwb = true;
            this.nb_modes = 2;
            this.sampling_rate = 0x7d00;
        }

        private void Wbinit()
        {
            this.lowenc = new NbEncoder();
            this.Init(160, 40, 8, 640, 0.9f);
            this.uwb = false;
            this.nb_modes = 5;
            this.sampling_rate = 0x3e80;
        }

        public virtual int Abr
        {
            get
            {
                return this.abr_enabled;
            }
            set
            {
                this.lowenc.Vbr = true;
                this.abr_enabled = (value != 0) ? 1 : 0;
                this.vbr_enabled = 1;
                int num = 10;
                int num3 = value;
                while (num >= 0)
                {
                    this.Quality = num;
                    if (this.BitRate <= num3)
                    {
                        break;
                    }
                    num--;
                }
                float num4 = num;
                if (num4 < 0f)
                {
                    num4 = 0f;
                }
                this.VbrQuality = num4;
                this.abr_count = 0f;
                this.abr_drift = 0f;
                this.abr_drift2 = 0f;
            }
        }

        public virtual int BitRate
        {
            get
            {
                if (base.submodes[base.submodeID] != null)
                {
                    return (this.lowenc.BitRate + ((this.sampling_rate * base.submodes[base.submodeID].BitsPerFrame) / base.frameSize));
                }
                return (this.lowenc.BitRate + ((this.sampling_rate * 4) / base.frameSize));
            }
            set
            {
                for (int i = 10; i >= 0; i--)
                {
                    this.Quality = i;
                    if (this.BitRate <= value)
                    {
                        return;
                    }
                }
            }
        }

        public virtual int Complexity
        {
            get
            {
                return this.complexity;
            }
            set
            {
                if (value < 0)
                {
                    value = 0;
                }
                if (value > 10)
                {
                    value = 10;
                }
                this.complexity = value;
            }
        }

        public bool Dtx
        {
            get
            {
                return (base.dtx_enabled == 1);
            }
            set
            {
                base.dtx_enabled = value ? 1 : 0;
            }
        }

        public virtual int EncodedFrameSize
        {
            get
            {
                int num = SbCodec.SB_FRAME_SIZE[base.submodeID];
                return (num + this.lowenc.EncodedFrameSize);
            }
        }

        public virtual int LookAhead
        {
            get
            {
                return (((2 * this.lowenc.LookAhead) + 0x40) - 1);
            }
        }

        public virtual int Mode
        {
            get
            {
                return base.submodeID;
            }
            set
            {
                if (value < 0)
                {
                    value = 0;
                }
                base.submodeID = this.submodeSelect = value;
            }
        }

        public virtual int Quality
        {
            set
            {
                if (value < 0)
                {
                    value = 0;
                }
                if (value > 10)
                {
                    value = 10;
                }
                if (this.uwb)
                {
                    this.lowenc.Quality = value;
                    this.Mode = UWB_QUALITY_MAP[value];
                }
                else
                {
                    this.lowenc.Mode = NB_QUALITY_MAP[value];
                    this.Mode = WB_QUALITY_MAP[value];
                }
            }
        }

        public virtual float RelativeQuality
        {
            get
            {
                return this.relative_quality;
            }
        }

        public virtual int SamplingRate
        {
            get
            {
                return this.sampling_rate;
            }
            set
            {
                this.sampling_rate = value;
                this.lowenc.SamplingRate = value;
            }
        }

        public virtual bool Vad
        {
            get
            {
                return (this.vad_enabled != 0);
            }
            set
            {
                this.vad_enabled = value ? 1 : 0;
            }
        }

        public virtual bool Vbr
        {
            get
            {
                return (this.vbr_enabled != 0);
            }
            set
            {
                this.vbr_enabled = value ? 1 : 0;
                this.lowenc.Vbr = value;
            }
        }

        public virtual float VbrQuality
        {
            get
            {
                return this.vbr_quality;
            }
            set
            {
                this.vbr_quality = value;
                float num = value + 0.6f;
                if (num > 10f)
                {
                    num = 10f;
                }
                this.lowenc.VbrQuality = num;
                int num2 = (int) Math.Floor((double) (0.5 + value));
                if (num2 > 10)
                {
                    num2 = 10;
                }
                this.Quality = num2;
            }
        }
    }
}

