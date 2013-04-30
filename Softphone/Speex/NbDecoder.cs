namespace Ozeki.Media.Codec.Speex.Implementation
{
    using System;

    internal class NbDecoder : NbCodec, IDecoder
    {
        private int count_lost;
        protected internal bool enhanced;
        protected internal Inband inband;
        private float[] innov2;
        private float last_ol_gain;
        private int last_pitch;
        private float last_pitch_gain;
        private float[] pitch_gain_buf;
        private int pitch_gain_buf_idx;
        protected internal Random random = new Random();
        protected internal Stereo stereo = new Stereo();

        public NbDecoder()
        {
            this.inband = new Inband(this.stereo);
            this.enhanced = true;
        }

        public virtual int Decode(Bits bits, float[] xout)
        {
            int num;
            int num4 = 0;
            float[] numArray = new float[3];
            float num6 = 0f;
            float num7 = 0f;
            int num8 = 40;
            float num9 = 0f;
            float num10 = 0f;
            if ((bits == null) && (base.dtx_enabled != 0))
            {
                base.submodeID = 0;
            }
            else
            {
                int num5;
                if (bits == null)
                {
                    this.DecodeLost(xout);
                    return 0;
                }
                do
                {
                    if (bits.BitsRemaining() < 5)
                    {
                        return -1;
                    }
                    if (bits.Unpack(1) != 0)
                    {
                        num5 = bits.Unpack(3);
                        int n = SbCodec.SB_FRAME_SIZE[num5];
                        if (n < 0)
                        {
                            throw new InvalidFormatException("Invalid sideband mode encountered (1st sideband): " + num5);
                        }
                        n -= 4;
                        bits.Advance(n);
                        if (bits.Unpack(1) != 0)
                        {
                            num5 = bits.Unpack(3);
                            n = SbCodec.SB_FRAME_SIZE[num5];
                            if (n < 0)
                            {
                                throw new InvalidFormatException("Invalid sideband mode encountered. (2nd sideband): " + num5);
                            }
                            n -= 4;
                            bits.Advance(n);
                            if (bits.Unpack(1) != 0)
                            {
                                throw new InvalidFormatException("More than two sideband layers found");
                            }
                        }
                    }
                    if (bits.BitsRemaining() < 4)
                    {
                        return 1;
                    }
                    num5 = bits.Unpack(4);
                    switch (num5)
                    {
                        case 15:
                            return 1;

                        case 14:
                            this.inband.SpeexInbandRequest(bits);
                            break;

                        case 13:
                            this.inband.UserInbandRequest(bits);
                            break;

                        default:
                            if (num5 > 8)
                            {
                                throw new InvalidFormatException("Invalid mode encountered: " + num5);
                            }
                            break;
                    }
                }
                while (num5 > 8);
                base.submodeID = num5;
            }
            Array.Copy(base.frmBuf, base.frameSize, base.frmBuf, 0, base.bufSize - base.frameSize);
            Array.Copy(base.excBuf, base.frameSize, base.excBuf, 0, base.bufSize - base.frameSize);
            if (base.submodes[base.submodeID] == null)
            {
                Filters.Bw_lpc(0.93f, base.interp_qlpc, base.lpc, 10);
                float num12 = 0f;
                for (num = 0; num < base.frameSize; num++)
                {
                    num12 += base.innov[num] * base.innov[num];
                }
                num12 = (float) Math.Sqrt((double) (num12 / ((float) base.frameSize)));
                for (num = base.excIdx; num < (base.excIdx + base.frameSize); num++)
                {
                    base.excBuf[num] = (float) ((3f * num12) * (this.random.NextDouble() - 0.5));
                }
                base.first = 1;
                Filters.Iir_mem2(base.excBuf, base.excIdx, base.lpc, base.frmBuf, base.frmIdx, base.frameSize, base.lpcSize, base.mem_sp);
                xout[0] = base.frmBuf[base.frmIdx] + (base.preemph * base.pre_mem);
                for (num = 1; num < base.frameSize; num++)
                {
                    xout[num] = base.frmBuf[base.frmIdx + num] + (base.preemph * xout[num - 1]);
                }
                base.pre_mem = xout[base.frameSize - 1];
                this.count_lost = 0;
                return 0;
            }
            base.submodes[base.submodeID].LsqQuant.Unquant(base.qlsp, base.lpcSize, bits);
            if (this.count_lost != 0)
            {
                float num13 = 0f;
                for (num = 0; num < base.lpcSize; num++)
                {
                    num13 += Math.Abs((float) (base.old_qlsp[num] - base.qlsp[num]));
                }
                float num14 = (float) (0.6 * Math.Exp(-0.2 * num13));
                for (num = 0; num < (2 * base.lpcSize); num++)
                {
                    base.mem_sp[num] *= num14;
                }
            }
            if ((base.first != 0) || (this.count_lost != 0))
            {
                num = 0;
                while (num < base.lpcSize)
                {
                    base.old_qlsp[num] = base.qlsp[num];
                    num++;
                }
            }
            if (base.submodes[base.submodeID].LbrPitch != -1)
            {
                num4 = base.min_pitch + bits.Unpack(7);
            }
            if (base.submodes[base.submodeID].ForcedPitchGain != 0)
            {
                int num15 = bits.Unpack(4);
                num7 = 0.066667f * num15;
            }
            num6 = (float) Math.Exp(((double) bits.Unpack(5)) / 3.5);
            if (base.submodeID == 1)
            {
                if (bits.Unpack(4) == 15)
                {
                    base.dtx_enabled = 1;
                }
                else
                {
                    base.dtx_enabled = 0;
                }
            }
            if (base.submodeID > 1)
            {
                base.dtx_enabled = 0;
            }
            for (int i = 0; i < base.nbSubframes; i++)
            {
                int num26;
                int num27;
                int num30;
                float num32;
                int num18 = base.subframeSize * i;
                int nsi = base.frmIdx + num18;
                int es = base.excIdx + num18;
                float num21 = (1f + i) / ((float) base.nbSubframes);
                num = 0;
                while (num < base.lpcSize)
                {
                    base.interp_qlsp[num] = ((1f - num21) * base.old_qlsp[num]) + (num21 * base.qlsp[num]);
                    num++;
                }
                Lsp.Enforce_margin(base.interp_qlsp, base.lpcSize, 0.002f);
                num = 0;
                while (num < base.lpcSize)
                {
                    base.interp_qlsp[num] = (float) Math.Cos((double) base.interp_qlsp[num]);
                    num++;
                }
                base.m_lsp.Lsp2lpc(base.interp_qlsp, base.interp_qlpc, base.lpcSize);
                if (this.enhanced)
                {
                    float num22 = 0.9f;
                    float gamma = base.submodes[base.submodeID].LpcEnhK1;
                    float num24 = base.submodes[base.submodeID].LpcEnhK2;
                    float num25 = (1f - ((1f - (num22 * gamma)) / (1f - (num22 * num24)))) / num22;
                    Filters.Bw_lpc(gamma, base.interp_qlpc, base.awk1, base.lpcSize);
                    Filters.Bw_lpc(num24, base.interp_qlpc, base.awk2, base.lpcSize);
                    Filters.Bw_lpc(num25, base.interp_qlpc, base.awk3, base.lpcSize);
                }
                num21 = 1f;
                base.pi_gain[i] = 0f;
                num = 0;
                while (num <= base.lpcSize)
                {
                    base.pi_gain[i] += num21 * base.interp_qlpc[num];
                    num21 = -num21;
                    num++;
                }
                num = 0;
                while (num < base.subframeSize)
                {
                    base.excBuf[es + num] = 0f;
                    num++;
                }
                if (base.submodes[base.submodeID].LbrPitch != -1)
                {
                    int lbrPitch = base.submodes[base.submodeID].LbrPitch;
                    if (lbrPitch != 0)
                    {
                        num26 = (num4 - lbrPitch) + 1;
                        if (num26 < base.min_pitch)
                        {
                            num26 = base.min_pitch;
                        }
                        num27 = num4 + lbrPitch;
                        if (num27 > base.max_pitch)
                        {
                            num27 = base.max_pitch;
                        }
                    }
                    else
                    {
                        num26 = num27 = num4;
                    }
                }
                else
                {
                    num26 = base.min_pitch;
                    num27 = base.max_pitch;
                }
                int pitch = base.submodes[base.submodeID].Ltp.Unquant(base.excBuf, es, num26, num7, base.subframeSize, numArray, bits, this.count_lost, num18, this.last_pitch_gain);
                if ((this.count_lost != 0) && (num6 < this.last_ol_gain))
                {
                    float num29 = num6 / (this.last_ol_gain + 1f);
                    num = 0;
                    while (num < base.subframeSize)
                    {
                        base.excBuf[base.excIdx + num] *= num29;
                        num++;
                    }
                }
                num21 = Math.Abs((float) ((numArray[0] + numArray[1]) + numArray[2]));
                num21 = Math.Abs(numArray[1]);
                if (numArray[0] > 0f)
                {
                    num21 += numArray[0];
                }
                else
                {
                    num21 -= 0.5f * numArray[0];
                }
                if (numArray[2] > 0f)
                {
                    num21 += numArray[2];
                }
                else
                {
                    num21 -= 0.5f * numArray[0];
                }
                num10 += num21;
                if (num21 > num9)
                {
                    num8 = pitch;
                    num9 = num21;
                }
                int num31 = i * base.subframeSize;
                num = num31;
                while (num < (num31 + base.subframeSize))
                {
                    base.innov[num] = 0f;
                    num++;
                }
                if (base.submodes[base.submodeID].HaveSubframeGain == 3)
                {
                    num30 = bits.Unpack(3);
                    num32 = (float) (num6 * Math.Exp((double) NbCodec.exc_gain_quant_scal3[num30]));
                }
                else if (base.submodes[base.submodeID].HaveSubframeGain == 1)
                {
                    num30 = bits.Unpack(1);
                    num32 = (float) (num6 * Math.Exp((double) NbCodec.exc_gain_quant_scal1[num30]));
                }
                else
                {
                    num32 = num6;
                }
                if (base.submodes[base.submodeID].Innovation != null)
                {
                    base.submodes[base.submodeID].Innovation.Unquantify(base.innov, num31, base.subframeSize, bits);
                }
                num = num31;
                while (num < (num31 + base.subframeSize))
                {
                    base.innov[num] *= num32;
                    num++;
                }
                if (base.submodeID == 1)
                {
                    float num33 = num7;
                    num = 0;
                    while (num < base.subframeSize)
                    {
                        base.excBuf[es + num] = 0f;
                        num++;
                    }
                    while (base.voc_offset < base.subframeSize)
                    {
                        if (base.voc_offset >= 0)
                        {
                            base.excBuf[es + base.voc_offset] = (float) Math.Sqrt((double) (1f * num4));
                        }
                        base.voc_offset += num4;
                    }
                    base.voc_offset -= base.subframeSize;
                    num33 = 0.5f + (2f * (num33 - 0.6f));
                    if (num33 < 0f)
                    {
                        num33 = 0f;
                    }
                    if (num33 > 1f)
                    {
                        num33 = 1f;
                    }
                    num = 0;
                    while (num < base.subframeSize)
                    {
                        float num34 = base.excBuf[es + num];
                        base.excBuf[es + num] = ((((((0.8f * num33) * base.excBuf[es + num]) * num6) + (((0.6f * num33) * base.voc_m1) * num6)) + ((0.5f * num33) * base.innov[num31 + num])) - ((0.5f * num33) * base.voc_m2)) + ((1f - num33) * base.innov[num31 + num]);
                        base.voc_m1 = num34;
                        base.voc_m2 = base.innov[num31 + num];
                        base.voc_mean = (0.95f * base.voc_mean) + (0.05f * base.excBuf[es + num]);
                        base.excBuf[es + num] -= base.voc_mean;
                        num++;
                    }
                }
                else
                {
                    num = 0;
                    while (num < base.subframeSize)
                    {
                        base.excBuf[es + num] += base.innov[num31 + num];
                        num++;
                    }
                }
                if (base.submodes[base.submodeID].DoubleCodebook != 0)
                {
                    num = 0;
                    while (num < base.subframeSize)
                    {
                        this.innov2[num] = 0f;
                        num++;
                    }
                    base.submodes[base.submodeID].Innovation.Unquantify(this.innov2, 0, base.subframeSize, bits);
                    num = 0;
                    while (num < base.subframeSize)
                    {
                        this.innov2[num] *= num32 * 0.4545454f;
                        num++;
                    }
                    num = 0;
                    while (num < base.subframeSize)
                    {
                        base.excBuf[es + num] += this.innov2[num];
                        num++;
                    }
                }
                num = 0;
                while (num < base.subframeSize)
                {
                    base.frmBuf[nsi + num] = base.excBuf[es + num];
                    num++;
                }
                if (this.enhanced && (base.submodes[base.submodeID].CombGain > 0f))
                {
                    base.filters.Comb_filter(base.excBuf, es, base.frmBuf, nsi, base.subframeSize, pitch, numArray, base.submodes[base.submodeID].CombGain);
                }
                if (this.enhanced)
                {
                    Filters.Filter_mem2(base.frmBuf, nsi, base.awk2, base.awk1, base.subframeSize, base.lpcSize, base.mem_sp, base.lpcSize);
                    Filters.Filter_mem2(base.frmBuf, nsi, base.awk3, base.interp_qlpc, base.subframeSize, base.lpcSize, base.mem_sp, 0);
                }
                else
                {
                    num = 0;
                    while (num < base.lpcSize)
                    {
                        base.mem_sp[base.lpcSize + num] = 0f;
                        num++;
                    }
                    Filters.Iir_mem2(base.frmBuf, nsi, base.interp_qlpc, base.frmBuf, nsi, base.subframeSize, base.lpcSize, base.mem_sp);
                }
            }
            xout[0] = base.frmBuf[base.frmIdx] + (base.preemph * base.pre_mem);
            for (num = 1; num < base.frameSize; num++)
            {
                xout[num] = base.frmBuf[base.frmIdx + num] + (base.preemph * xout[num - 1]);
            }
            base.pre_mem = xout[base.frameSize - 1];
            for (num = 0; num < base.lpcSize; num++)
            {
                base.old_qlsp[num] = base.qlsp[num];
            }
            base.first = 0;
            this.count_lost = 0;
            this.last_pitch = num8;
            this.last_pitch_gain = 0.25f * num10;
            this.pitch_gain_buf[this.pitch_gain_buf_idx++] = this.last_pitch_gain;
            if (this.pitch_gain_buf_idx > 2)
            {
                this.pitch_gain_buf_idx = 0;
            }
            this.last_ol_gain = num6;
            return 0;
        }

        public int DecodeLost(float[] xout)
        {
            int num;
            float num3 = (float) Math.Exp((-0.04 * this.count_lost) * this.count_lost);
            float num4 = (this.pitch_gain_buf[0] < this.pitch_gain_buf[1]) ? ((this.pitch_gain_buf[1] < this.pitch_gain_buf[2]) ? this.pitch_gain_buf[1] : ((this.pitch_gain_buf[0] < this.pitch_gain_buf[2]) ? this.pitch_gain_buf[2] : this.pitch_gain_buf[0])) : ((this.pitch_gain_buf[2] < this.pitch_gain_buf[1]) ? this.pitch_gain_buf[1] : ((this.pitch_gain_buf[2] < this.pitch_gain_buf[0]) ? this.pitch_gain_buf[2] : this.pitch_gain_buf[0]));
            if (num4 < this.last_pitch_gain)
            {
                this.last_pitch_gain = num4;
            }
            float num2 = this.last_pitch_gain;
            if (num2 > 0.95f)
            {
                num2 = 0.95f;
            }
            num2 *= num3;
            Array.Copy(base.frmBuf, base.frameSize, base.frmBuf, 0, base.bufSize - base.frameSize);
            Array.Copy(base.excBuf, base.frameSize, base.excBuf, 0, base.bufSize - base.frameSize);
            for (int i = 0; i < base.nbSubframes; i++)
            {
                int num6 = base.subframeSize * i;
                int xs = base.frmIdx + num6;
                int num8 = base.excIdx + num6;
                if (this.enhanced)
                {
                    float num10;
                    float num11;
                    float num9 = 0.9f;
                    if (base.submodes[base.submodeID] != null)
                    {
                        num10 = base.submodes[base.submodeID].LpcEnhK1;
                        num11 = base.submodes[base.submodeID].LpcEnhK2;
                    }
                    else
                    {
                        num10 = num11 = 0.7f;
                    }
                    float gamma = (1f - ((1f - (num9 * num10)) / (1f - (num9 * num11)))) / num9;
                    Filters.Bw_lpc(num10, base.interp_qlpc, base.awk1, base.lpcSize);
                    Filters.Bw_lpc(num11, base.interp_qlpc, base.awk2, base.lpcSize);
                    Filters.Bw_lpc(gamma, base.interp_qlpc, base.awk3, base.lpcSize);
                }
                float num13 = 0f;
                num = 0;
                while (num < base.frameSize)
                {
                    num13 += base.innov[num] * base.innov[num];
                    num++;
                }
                num13 = (float) Math.Sqrt((double) (num13 / ((float) base.frameSize)));
                num = 0;
                while (num < base.subframeSize)
                {
                    base.excBuf[num8 + num] = (num2 * base.excBuf[(num8 + num) - this.last_pitch]) + ((((num3 * ((float) Math.Sqrt((double) (1f - num2)))) * 3f) * num13) * (((float) this.random.NextDouble()) - 0.5f));
                    num++;
                }
                num = 0;
                while (num < base.subframeSize)
                {
                    base.frmBuf[xs + num] = base.excBuf[num8 + num];
                    num++;
                }
                if (this.enhanced)
                {
                    Filters.Filter_mem2(base.frmBuf, xs, base.awk2, base.awk1, base.subframeSize, base.lpcSize, base.mem_sp, base.lpcSize);
                    Filters.Filter_mem2(base.frmBuf, xs, base.awk3, base.interp_qlpc, base.subframeSize, base.lpcSize, base.mem_sp, 0);
                }
                else
                {
                    num = 0;
                    while (num < base.lpcSize)
                    {
                        base.mem_sp[base.lpcSize + num] = 0f;
                        num++;
                    }
                    Filters.Iir_mem2(base.frmBuf, xs, base.interp_qlpc, base.frmBuf, xs, base.subframeSize, base.lpcSize, base.mem_sp);
                }
            }
            xout[0] = base.frmBuf[0] + (base.preemph * base.pre_mem);
            for (num = 1; num < base.frameSize; num++)
            {
                xout[num] = base.frmBuf[num] + (base.preemph * xout[num - 1]);
            }
            base.pre_mem = xout[base.frameSize - 1];
            base.first = 0;
            this.count_lost++;
            this.pitch_gain_buf[this.pitch_gain_buf_idx++] = num2;
            if (this.pitch_gain_buf_idx > 2)
            {
                this.pitch_gain_buf_idx = 0;
            }
            return 0;
        }

        public virtual void DecodeStereo(float[] data, int frameSize)
        {
            this.stereo.Decode(data, frameSize);
        }

        protected override void Init(int frameSize, int subframeSize, int lpcSize, int bufSize)
        {
            base.Init(frameSize, subframeSize, lpcSize, bufSize);
            this.innov2 = new float[40];
            this.count_lost = 0;
            this.last_pitch = 40;
            this.last_pitch_gain = 0f;
            this.pitch_gain_buf = new float[3];
            this.pitch_gain_buf_idx = 0;
            this.last_ol_gain = 0f;
        }

        public bool Dtx
        {
            get
            {
                return (base.dtx_enabled != 0);
            }
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

