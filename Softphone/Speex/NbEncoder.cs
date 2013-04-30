namespace Ozeki.Media.Codec.Speex.Implementation
{
    using System;

    internal class NbEncoder : NbCodec, IEncoder
    {
        protected internal float abr_count;
        protected internal float abr_drift;
        protected internal float abr_drift2;
        protected internal int abr_enabled;
        private float[] autocorr;
        private int bounded_pitch;
        private float[] buf2;
        private float[] bw_lpc1;
        private float[] bw_lpc2;
        protected internal int complexity;
        private int dtx_count;
        private float[] exc2Buf;
        private int exc2Idx;
        private float[] innov2;
        private float[] interp_lpc;
        private float[] interp_lsp;
        private float[] lagWindow;
        private float[] lsp;
        private float[] mem_exc;
        private float[] mem_sw;
        private float[] mem_sw_whole;
        public static readonly int[] NB_QUALITY_MAP = new int[] { 1, 8, 2, 3, 3, 4, 4, 5, 5, 6, 7 };
        private float[] old_lsp;
        private int[] pitch;
        private float pre_mem2;
        private float[] rc;
        protected internal float relative_quality;
        protected internal int sampling_rate;
        protected internal int submodeSelect;
        private float[] swBuf;
        private int swIdx;
        protected internal int vad_enabled;
        private Ozeki.Media.Codec.Speex.Implementation.Vbr vbr;
        protected internal int vbr_enabled;
        protected internal float vbr_quality;
        private float[] window;

        public virtual int Encode(Bits bits, float[] ins0)
        {
            int num;
            int num5;
            float num6;
            Array.Copy(base.frmBuf, base.frameSize, base.frmBuf, 0, base.bufSize - base.frameSize);
            base.frmBuf[base.bufSize - base.frameSize] = ins0[0] - (base.preemph * base.pre_mem);
            for (num = 1; num < base.frameSize; num++)
            {
                base.frmBuf[(base.bufSize - base.frameSize) + num] = ins0[num] - (base.preemph * ins0[num - 1]);
            }
            base.pre_mem = ins0[base.frameSize - 1];
            Array.Copy(this.exc2Buf, base.frameSize, this.exc2Buf, 0, base.bufSize - base.frameSize);
            Array.Copy(base.excBuf, base.frameSize, base.excBuf, 0, base.bufSize - base.frameSize);
            Array.Copy(this.swBuf, base.frameSize, this.swBuf, 0, base.bufSize - base.frameSize);
            for (num = 0; num < base.windowSize; num++)
            {
                this.buf2[num] = base.frmBuf[num + base.frmIdx] * this.window[num];
            }
            Lpc.Autocorr(this.buf2, this.autocorr, base.lpcSize + 1, base.windowSize);
            this.autocorr[0] += 10f;
            this.autocorr[0] *= base.lpc_floor;
            for (num = 0; num < (base.lpcSize + 1); num++)
            {
                this.autocorr[num] *= this.lagWindow[num];
            }
            Lpc.Wld(base.lpc, this.autocorr, this.rc, base.lpcSize);
            Array.Copy(base.lpc, 0, base.lpc, 1, base.lpcSize);
            base.lpc[0] = 1f;
            int num2 = Lsp.Lpc2lsp(base.lpc, base.lpcSize, this.lsp, 15, 0.2f);
            if (num2 == base.lpcSize)
            {
                for (num = 0; num < base.lpcSize; num++)
                {
                    this.lsp[num] = (float) Math.Acos((double) this.lsp[num]);
                }
            }
            else
            {
                if (this.complexity > 1)
                {
                    num2 = Lsp.Lpc2lsp(base.lpc, base.lpcSize, this.lsp, 11, 0.05f);
                }
                if (num2 == base.lpcSize)
                {
                    for (num = 0; num < base.lpcSize; num++)
                    {
                        this.lsp[num] = (float) Math.Acos((double) this.lsp[num]);
                    }
                }
                else
                {
                    num = 0;
                    while (num < base.lpcSize)
                    {
                        this.lsp[num] = this.old_lsp[num];
                        num++;
                    }
                }
            }
            float num3 = 0f;
            for (num = 0; num < base.lpcSize; num++)
            {
                num3 += (this.old_lsp[num] - this.lsp[num]) * (this.old_lsp[num] - this.lsp[num]);
            }
            if (base.first != 0)
            {
                for (num = 0; num < base.lpcSize; num++)
                {
                    this.interp_lsp[num] = this.lsp[num];
                }
            }
            else
            {
                num = 0;
                while (num < base.lpcSize)
                {
                    this.interp_lsp[num] = (0.375f * this.old_lsp[num]) + (0.625f * this.lsp[num]);
                    num++;
                }
            }
            Lsp.Enforce_margin(this.interp_lsp, base.lpcSize, 0.002f);
            for (num = 0; num < base.lpcSize; num++)
            {
                this.interp_lsp[num] = (float) Math.Cos((double) this.interp_lsp[num]);
            }
            base.m_lsp.Lsp2lpc(this.interp_lsp, this.interp_lpc, base.lpcSize);
            if (((base.submodes[base.submodeID] == null) || (this.vbr_enabled != 0)) || (((this.vad_enabled != 0) || (base.submodes[base.submodeID].ForcedPitchGain != 0)) || (base.submodes[base.submodeID].LbrPitch != -1)))
            {
                int[] pitch = new int[6];
                float[] gain = new float[6];
                Filters.Bw_lpc(base.gamma1, this.interp_lpc, this.bw_lpc1, base.lpcSize);
                Filters.Bw_lpc(base.gamma2, this.interp_lpc, this.bw_lpc2, base.lpcSize);
                Filters.Filter_mem2(base.frmBuf, base.frmIdx, this.bw_lpc1, this.bw_lpc2, this.swBuf, this.swIdx, base.frameSize, base.lpcSize, this.mem_sw_whole, 0);
                Ltp.Open_loop_nbest_pitch(this.swBuf, this.swIdx, base.min_pitch, base.max_pitch, base.frameSize, pitch, gain, 6);
                num5 = pitch[0];
                num6 = gain[0];
                for (num = 1; num < 6; num++)
                {
                    if ((gain[num] > (0.85 * num6)) && (((Math.Abs((double) (pitch[num] - (((double) num5) / 2.0))) <= 1.0) || (Math.Abs((double) (pitch[num] - (((double) num5) / 3.0))) <= 1.0)) || ((Math.Abs((double) (pitch[num] - (((double) num5) / 4.0))) <= 1.0) || (Math.Abs((double) (pitch[num] - (((double) num5) / 5.0))) <= 1.0))))
                    {
                        num5 = pitch[num];
                    }
                }
            }
            else
            {
                num5 = 0;
                num6 = 0f;
            }
            Filters.Fir_mem2(base.frmBuf, base.frmIdx, this.interp_lpc, base.excBuf, base.excIdx, base.frameSize, base.lpcSize, this.mem_exc);
            float num4 = 0f;
            for (num = 0; num < base.frameSize; num++)
            {
                num4 += base.excBuf[base.excIdx + num] * base.excBuf[base.excIdx + num];
            }
            num4 = (float) Math.Sqrt((double) (1f + (num4 / ((float) base.frameSize))));
            if ((this.vbr != null) && ((this.vbr_enabled != 0) || (this.vad_enabled != 0)))
            {
                if (this.abr_enabled != 0)
                {
                    float num7 = 0f;
                    if ((this.abr_drift2 * this.abr_drift) > 0f)
                    {
                        num7 = (-1E-05f * this.abr_drift) / (1f + this.abr_count);
                        if (num7 > 0.05f)
                        {
                            num7 = 0.05f;
                        }
                        if (num7 < -0.05f)
                        {
                            num7 = -0.05f;
                        }
                    }
                    this.vbr_quality += num7;
                    if (this.vbr_quality > 10f)
                    {
                        this.vbr_quality = 10f;
                    }
                    if (this.vbr_quality < 0f)
                    {
                        this.vbr_quality = 0f;
                    }
                }
                this.relative_quality = this.vbr.Analysis(ins0, base.frameSize, num5, num6);
                if (this.vbr_enabled != 0)
                {
                    int num8;
                    int num9 = 0;
                    float num10 = 100f;
                    for (num8 = 8; num8 > 0; num8--)
                    {
                        float num12;
                        int index = (int) Math.Floor((double) this.vbr_quality);
                        if (index == 10)
                        {
                            num12 = Ozeki.Media.Codec.Speex.Implementation.Vbr.nb_thresh[num8][index];
                        }
                        else
                        {
                            num12 = ((this.vbr_quality - index) * Ozeki.Media.Codec.Speex.Implementation.Vbr.nb_thresh[num8][index + 1]) + (((1 + index) - this.vbr_quality) * Ozeki.Media.Codec.Speex.Implementation.Vbr.nb_thresh[num8][index]);
                        }
                        if ((this.relative_quality > num12) && ((this.relative_quality - num12) < num10))
                        {
                            num9 = num8;
                            num10 = this.relative_quality - num12;
                        }
                    }
                    num8 = num9;
                    if (num8 == 0)
                    {
                        if (((this.dtx_count == 0) || (num3 > 0.05)) || ((base.dtx_enabled == 0) || (this.dtx_count > 20)))
                        {
                            num8 = 1;
                            this.dtx_count = 1;
                        }
                        else
                        {
                            num8 = 0;
                            this.dtx_count++;
                        }
                    }
                    else
                    {
                        this.dtx_count = 0;
                    }
                    this.Mode = num8;
                    if (this.abr_enabled != 0)
                    {
                        int bitRate = this.BitRate;
                        this.abr_drift += bitRate - this.abr_enabled;
                        this.abr_drift2 = (0.95f * this.abr_drift2) + (0.05f * (bitRate - this.abr_enabled));
                        float? nullable = 1f;
                        this.abr_count += nullable.Value;
                    }
                }
                else
                {
                    int submodeSelect;
                    if (this.relative_quality < 2f)
                    {
                        if (((this.dtx_count == 0) || (num3 > 0.05)) || ((base.dtx_enabled == 0) || (this.dtx_count > 20)))
                        {
                            this.dtx_count = 1;
                            submodeSelect = 1;
                        }
                        else
                        {
                            submodeSelect = 0;
                            this.dtx_count++;
                        }
                    }
                    else
                    {
                        this.dtx_count = 0;
                        submodeSelect = this.submodeSelect;
                    }
                    base.submodeID = submodeSelect;
                }
            }
            else
            {
                this.relative_quality = -1f;
            }
            bits.Pack(0, 1);
            bits.Pack(base.submodeID, 4);
            if (base.submodes[base.submodeID] == null)
            {
                for (num = 0; num < base.frameSize; num++)
                {
                    float num34;
                    this.swBuf[this.swIdx + num] = num34 = 0f;
                    base.excBuf[base.excIdx + num] = this.exc2Buf[this.exc2Idx + num] = num34;
                }
                for (num = 0; num < base.lpcSize; num++)
                {
                    this.mem_sw[num] = 0f;
                }
                base.first = 1;
                this.bounded_pitch = 1;
                Filters.Iir_mem2(base.excBuf, base.excIdx, base.interp_qlpc, base.frmBuf, base.frmIdx, base.frameSize, base.lpcSize, base.mem_sp);
                ins0[0] = base.frmBuf[base.frmIdx] + (base.preemph * this.pre_mem2);
                for (num = 1; num < base.frameSize; num++)
                {
                    ins0[num] = base.frmBuf[base.frmIdx = num] + (base.preemph * ins0[num - 1]);
                }
                this.pre_mem2 = ins0[base.frameSize - 1];
                return 0;
            }
            if (base.first != 0)
            {
                for (num = 0; num < base.lpcSize; num++)
                {
                    this.old_lsp[num] = this.lsp[num];
                }
            }
            base.submodes[base.submodeID].LsqQuant.Quant(this.lsp, base.qlsp, base.lpcSize, bits);
            if (base.submodes[base.submodeID].LbrPitch != -1)
            {
                bits.Pack(num5 - base.min_pitch, 7);
            }
            if (base.submodes[base.submodeID].ForcedPitchGain != 0)
            {
                int num15 = (int) Math.Floor((double) (0.5 + (15f * num6)));
                if (num15 > 15)
                {
                    num15 = 15;
                }
                if (num15 < 0)
                {
                    num15 = 0;
                }
                bits.Pack(num15, 4);
                num6 = 0.066667f * num15;
            }
            int data = (int) Math.Floor((double) (0.5 + (3.5 * Math.Log((double) num4))));
            if (data < 0)
            {
                data = 0;
            }
            if (data > 0x1f)
            {
                data = 0x1f;
            }
            num4 = (float) Math.Exp(((double) data) / 3.5);
            bits.Pack(data, 5);
            if (base.first != 0)
            {
                for (num = 0; num < base.lpcSize; num++)
                {
                    base.old_qlsp[num] = base.qlsp[num];
                }
            }
            float[] y = new float[base.subframeSize];
            float[] target = new float[base.subframeSize];
            float[] numArray4 = new float[base.subframeSize];
            float[] mem = new float[base.lpcSize];
            float[] numArray5 = new float[base.frameSize];
            num = 0;
            while (num < base.frameSize)
            {
                numArray5[num] = base.frmBuf[base.frmIdx + num];
                num++;
            }
            for (int i = 0; i < base.nbSubframes; i++)
            {
                int num25;
                int num26;
                int num19 = base.subframeSize * i;
                int xs = base.frmIdx + num19;
                int num22 = base.excIdx + num19;
                int ys = this.swIdx + num19;
                int num23 = this.exc2Idx + num19;
                float num18 = ((float) (1.0 + i)) / ((float) base.nbSubframes);
                num = 0;
                while (num < base.lpcSize)
                {
                    this.interp_lsp[num] = ((1f - num18) * this.old_lsp[num]) + (num18 * this.lsp[num]);
                    num++;
                }
                num = 0;
                while (num < base.lpcSize)
                {
                    base.interp_qlsp[num] = ((1f - num18) * base.old_qlsp[num]) + (num18 * base.qlsp[num]);
                    num++;
                }
                Lsp.Enforce_margin(this.interp_lsp, base.lpcSize, 0.002f);
                Lsp.Enforce_margin(base.interp_qlsp, base.lpcSize, 0.002f);
                num = 0;
                while (num < base.lpcSize)
                {
                    this.interp_lsp[num] = (float) Math.Cos((double) this.interp_lsp[num]);
                    num++;
                }
                base.m_lsp.Lsp2lpc(this.interp_lsp, this.interp_lpc, base.lpcSize);
                num = 0;
                while (num < base.lpcSize)
                {
                    base.interp_qlsp[num] = (float) Math.Cos((double) base.interp_qlsp[num]);
                    num++;
                }
                base.m_lsp.Lsp2lpc(base.interp_qlsp, base.interp_qlpc, base.lpcSize);
                num18 = 1f;
                base.pi_gain[i] = 0f;
                num = 0;
                while (num <= base.lpcSize)
                {
                    base.pi_gain[i] += num18 * base.interp_qlpc[num];
                    num18 = -num18;
                    num++;
                }
                Filters.Bw_lpc(base.gamma1, this.interp_lpc, this.bw_lpc1, base.lpcSize);
                if (base.gamma2 >= 0f)
                {
                    Filters.Bw_lpc(base.gamma2, this.interp_lpc, this.bw_lpc2, base.lpcSize);
                }
                else
                {
                    this.bw_lpc2[0] = 1f;
                    this.bw_lpc2[1] = -base.preemph;
                    num = 2;
                    while (num <= base.lpcSize)
                    {
                        this.bw_lpc2[num] = 0f;
                        num++;
                    }
                }
                num = 0;
                while (num < base.subframeSize)
                {
                    base.excBuf[num22 + num] = 0f;
                    num++;
                }
                base.excBuf[num22] = 1f;
                Filters.Syn_percep_zero(base.excBuf, num22, base.interp_qlpc, this.bw_lpc1, this.bw_lpc2, numArray4, base.subframeSize, base.lpcSize);
                num = 0;
                while (num < base.subframeSize)
                {
                    base.excBuf[num22 + num] = 0f;
                    num++;
                }
                num = 0;
                while (num < base.subframeSize)
                {
                    this.exc2Buf[num23 + num] = 0f;
                    num++;
                }
                num = 0;
                while (num < base.lpcSize)
                {
                    mem[num] = base.mem_sp[num];
                    num++;
                }
                Filters.Iir_mem2(base.excBuf, num22, base.interp_qlpc, base.excBuf, num22, base.subframeSize, base.lpcSize, mem);
                num = 0;
                while (num < base.lpcSize)
                {
                    mem[num] = this.mem_sw[num];
                    num++;
                }
                Filters.Filter_mem2(base.excBuf, num22, this.bw_lpc1, this.bw_lpc2, y, 0, base.subframeSize, base.lpcSize, mem, 0);
                num = 0;
                while (num < base.lpcSize)
                {
                    mem[num] = this.mem_sw[num];
                    num++;
                }
                Filters.Filter_mem2(base.frmBuf, xs, this.bw_lpc1, this.bw_lpc2, this.swBuf, ys, base.subframeSize, base.lpcSize, mem, 0);
                num = 0;
                while (num < base.subframeSize)
                {
                    target[num] = this.swBuf[ys + num] - y[num];
                    num++;
                }
                num = 0;
                while (num < base.subframeSize)
                {
                    base.excBuf[num22 + num] = this.exc2Buf[num23 + num] = 0f;
                    num++;
                }
                if (base.submodes[base.submodeID].LbrPitch != -1)
                {
                    int lbrPitch = base.submodes[base.submodeID].LbrPitch;
                    if (lbrPitch != 0)
                    {
                        if (num5 < ((base.min_pitch + lbrPitch) - 1))
                        {
                            num5 = (base.min_pitch + lbrPitch) - 1;
                        }
                        if (num5 > (base.max_pitch - lbrPitch))
                        {
                            num5 = base.max_pitch - lbrPitch;
                        }
                        num25 = (num5 - lbrPitch) + 1;
                        num26 = num5 + lbrPitch;
                    }
                    else
                    {
                        num25 = num26 = num5;
                    }
                }
                else
                {
                    num25 = base.min_pitch;
                    num26 = base.max_pitch;
                }
                if ((this.bounded_pitch != 0) && (num26 > num19))
                {
                    num26 = num19;
                }
                this.pitch[i] = base.submodes[base.submodeID].Ltp.Quant(target, this.swBuf, ys, base.interp_qlpc, this.bw_lpc1, this.bw_lpc2, base.excBuf, num22, num25, num26, num6, base.lpcSize, base.subframeSize, bits, this.exc2Buf, num23, numArray4, this.complexity);
                Filters.Syn_percep_zero(base.excBuf, num22, base.interp_qlpc, this.bw_lpc1, this.bw_lpc2, y, base.subframeSize, base.lpcSize);
                num = 0;
                while (num < base.subframeSize)
                {
                    target[num] -= y[num];
                    num++;
                }
                float num29 = 0f;
                int es = i * base.subframeSize;
                num = 0;
                while (num < base.subframeSize)
                {
                    base.innov[es + num] = 0f;
                    num++;
                }
                Filters.Residue_percep_zero(target, 0, base.interp_qlpc, this.bw_lpc1, this.bw_lpc2, this.buf2, base.subframeSize, base.lpcSize);
                num = 0;
                while (num < base.subframeSize)
                {
                    num29 += this.buf2[num] * this.buf2[num];
                    num++;
                }
                num29 = (float) Math.Sqrt((double) (0.1f + (num29 / ((float) base.subframeSize))));
                num29 /= num4;
                if (base.submodes[base.submodeID].HaveSubframeGain != 0)
                {
                    int num31;
                    num29 = (float) Math.Log((double) num29);
                    if (base.submodes[base.submodeID].HaveSubframeGain == 3)
                    {
                        num31 = VQ.Index(num29, NbCodec.exc_gain_quant_scal3, 8);
                        bits.Pack(num31, 3);
                        num29 = NbCodec.exc_gain_quant_scal3[num31];
                    }
                    else
                    {
                        num31 = VQ.Index(num29, NbCodec.exc_gain_quant_scal1, 2);
                        bits.Pack(num31, 1);
                        num29 = NbCodec.exc_gain_quant_scal1[num31];
                    }
                    num29 = (float) Math.Exp((double) num29);
                }
                else
                {
                    num29 = 1f;
                }
                num29 *= num4;
                float num30 = 1f / num29;
                num = 0;
                while (num < base.subframeSize)
                {
                    target[num] *= num30;
                    num++;
                }
                base.submodes[base.submodeID].Innovation.Quantify(target, base.interp_qlpc, this.bw_lpc1, this.bw_lpc2, base.lpcSize, base.subframeSize, base.innov, es, numArray4, bits, this.complexity);
                num = 0;
                while (num < base.subframeSize)
                {
                    base.innov[es + num] *= num29;
                    num++;
                }
                num = 0;
                while (num < base.subframeSize)
                {
                    base.excBuf[num22 + num] += base.innov[es + num];
                    num++;
                }
                if (base.submodes[base.submodeID].DoubleCodebook != 0)
                {
                    float[] exc = new float[base.subframeSize];
                    num = 0;
                    while (num < base.subframeSize)
                    {
                        target[num] *= 2.2f;
                        num++;
                    }
                    base.submodes[base.submodeID].Innovation.Quantify(target, base.interp_qlpc, this.bw_lpc1, this.bw_lpc2, base.lpcSize, base.subframeSize, exc, 0, numArray4, bits, this.complexity);
                    num = 0;
                    while (num < base.subframeSize)
                    {
                        exc[num] *= (float) (num29 * 0.45454545454545453);
                        num++;
                    }
                    num = 0;
                    while (num < base.subframeSize)
                    {
                        base.excBuf[num22 + num] += exc[num];
                        num++;
                    }
                }
                num = 0;
                while (num < base.subframeSize)
                {
                    target[num] *= num29;
                    num++;
                }
                num = 0;
                while (num < base.lpcSize)
                {
                    mem[num] = base.mem_sp[num];
                    num++;
                }
                Filters.Iir_mem2(base.excBuf, num22, base.interp_qlpc, base.frmBuf, xs, base.subframeSize, base.lpcSize, base.mem_sp);
                Filters.Filter_mem2(base.frmBuf, xs, this.bw_lpc1, this.bw_lpc2, this.swBuf, ys, base.subframeSize, base.lpcSize, this.mem_sw, 0);
                num = 0;
                while (num < base.subframeSize)
                {
                    this.exc2Buf[num23 + num] = base.excBuf[num22 + num];
                    num++;
                }
            }
            if (base.submodeID >= 1)
            {
                for (num = 0; num < base.lpcSize; num++)
                {
                    this.old_lsp[num] = this.lsp[num];
                }
                for (num = 0; num < base.lpcSize; num++)
                {
                    base.old_qlsp[num] = base.qlsp[num];
                }
            }
            if (base.submodeID == 1)
            {
                if (this.dtx_count != 0)
                {
                    bits.Pack(15, 4);
                }
                else
                {
                    bits.Pack(0, 4);
                }
            }
            base.first = 0;
            float num32 = 0f;
            float num33 = 0f;
            for (num = 0; num < base.frameSize; num++)
            {
                num32 += base.frmBuf[base.frmIdx + num] * base.frmBuf[base.frmIdx + num];
                num33 += (base.frmBuf[base.frmIdx + num] - numArray5[num]) * (base.frmBuf[base.frmIdx + num] - numArray5[num]);
            }
            Math.Log((double) ((num32 + 1f) / (num33 + 1f)));
            ins0[0] = base.frmBuf[base.frmIdx] + (base.preemph * this.pre_mem2);
            for (num = 1; num < base.frameSize; num++)
            {
                ins0[num] = base.frmBuf[base.frmIdx + num] + (base.preemph * ins0[num - 1]);
            }
            this.pre_mem2 = ins0[base.frameSize - 1];
            if ((base.submodes[base.submodeID].Innovation is NoiseSearch) || (base.submodeID == 0))
            {
                this.bounded_pitch = 1;
            }
            else
            {
                this.bounded_pitch = 0;
            }
            return 1;
        }

        protected override void Init(int frameSize, int subframeSize, int lpcSize, int bufSize)
        {
            base.Init(frameSize, subframeSize, lpcSize, bufSize);
            this.complexity = 3;
            this.vbr_enabled = 0;
            this.vad_enabled = 0;
            this.abr_enabled = 0;
            this.vbr_quality = 8f;
            this.submodeSelect = 5;
            this.pre_mem2 = 0f;
            this.bounded_pitch = 1;
            this.exc2Buf = new float[bufSize];
            this.exc2Idx = bufSize - base.windowSize;
            this.swBuf = new float[bufSize];
            this.swIdx = bufSize - base.windowSize;
            this.window = Misc.Window(base.windowSize, subframeSize);
            this.lagWindow = Misc.LagWindow(lpcSize, base.lag_factor);
            this.autocorr = new float[lpcSize + 1];
            this.buf2 = new float[base.windowSize];
            this.interp_lpc = new float[lpcSize + 1];
            base.interp_qlpc = new float[lpcSize + 1];
            this.bw_lpc1 = new float[lpcSize + 1];
            this.bw_lpc2 = new float[lpcSize + 1];
            this.lsp = new float[lpcSize];
            base.qlsp = new float[lpcSize];
            this.old_lsp = new float[lpcSize];
            base.old_qlsp = new float[lpcSize];
            this.interp_lsp = new float[lpcSize];
            base.interp_qlsp = new float[lpcSize];
            this.rc = new float[lpcSize];
            base.mem_sp = new float[lpcSize];
            this.mem_sw = new float[lpcSize];
            this.mem_sw_whole = new float[lpcSize];
            this.mem_exc = new float[lpcSize];
            this.vbr = new Ozeki.Media.Codec.Speex.Implementation.Vbr();
            this.dtx_count = 0;
            this.abr_count = 0f;
            this.sampling_rate = 0x1f40;
            base.awk1 = new float[lpcSize + 1];
            base.awk2 = new float[lpcSize + 1];
            base.awk3 = new float[lpcSize + 1];
            this.innov2 = new float[40];
            this.pitch = new int[base.nbSubframes];
        }

        public virtual int Abr
        {
            get
            {
                return this.abr_enabled;
            }
            set
            {
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
                    return ((this.sampling_rate * base.submodes[base.submodeID].BitsPerFrame) / base.frameSize);
                }
                return ((this.sampling_rate * 5) / base.frameSize);
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

        public virtual bool Dtx
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
                return NbCodec.NB_FRAME_SIZE[base.submodeID];
            }
        }

        public virtual int LookAhead
        {
            get
            {
                return (base.windowSize - base.frameSize);
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
                base.submodeID = this.submodeSelect = NB_QUALITY_MAP[value];
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
                if (value < 0f)
                {
                    value = 0f;
                }
                if (value > 10f)
                {
                    value = 10f;
                }
                this.vbr_quality = value;
            }
        }
    }
}

