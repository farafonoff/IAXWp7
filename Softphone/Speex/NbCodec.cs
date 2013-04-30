namespace Ozeki.Media.Codec.Speex.Implementation
{
    using System;

    internal class NbCodec
    {
        protected internal float[] awk1;
        protected internal float[] awk2;
        protected internal float[] awk3;
        protected internal int bufSize;
        protected internal int dtx_enabled;
        protected static readonly float[] exc_gain_quant_scal1 = new float[] { -0.35f, 0.05f };
        protected static readonly float[] exc_gain_quant_scal3 = new float[] { -2.79475f, -1.81066f, -1.16985f, -0.848119f, -0.58719f, -0.329818f, -0.063266f, 0.282826f };
        protected internal float[] excBuf;
        protected internal int excIdx;
        protected internal Filters filters = new Filters();
        protected internal int first;
        protected internal int frameSize;
        protected internal float[] frmBuf;
        protected internal int frmIdx;
        protected internal float gamma1;
        protected internal float gamma2;
        protected internal float[] innov;
        protected internal float[] interp_qlpc;
        protected internal float[] interp_qlsp;
        protected internal float lag_factor;
        protected internal float[] lpc;
        protected internal float lpc_floor;
        protected internal int lpcSize;
        protected internal Lsp m_lsp = new Lsp();
        protected internal int max_pitch;
        protected internal float[] mem_sp;
        protected internal int min_pitch;
        protected static readonly int[] NB_FRAME_SIZE = new int[] { 5, 0x2b, 0x77, 160, 220, 300, 0x16c, 0x1ec, 0x4f, 1, 1, 1, 1, 1, 1, 1 };
        protected const int NB_SUBMODE_BITS = 4;
        protected const int NB_SUBMODES = 0x10;
        protected internal int nbSubframes;
        protected internal float[] old_qlsp;
        protected internal float[] pi_gain;
        protected internal float pre_mem;
        protected internal float preemph;
        protected internal float[] qlsp;
        protected internal int subframeSize;
        protected internal int submodeID;
        protected internal SubMode[] submodes;
        protected const float VERY_SMALL = 0f;
        protected internal float voc_m1;
        protected internal float voc_m2;
        protected internal float voc_mean;
        protected internal int voc_offset;
        protected internal int windowSize;

        public NbCodec()
        {
            this.Nbinit();
        }

        private static SubMode[] BuildNbSubModes()
        {
            Ltp3Tap ltp = new Ltp3Tap(Codebook_Constants.gain_cdbk_nb, 7, 7);
            Ltp3Tap tap2 = new Ltp3Tap(Codebook_Constants.gain_cdbk_lbr, 5, 0);
            Ltp3Tap tap3 = new Ltp3Tap(Codebook_Constants.gain_cdbk_lbr, 5, 7);
            Ltp3Tap tap4 = new Ltp3Tap(Codebook_Constants.gain_cdbk_lbr, 5, 7);
            LtpForcedPitch pitch = new LtpForcedPitch();
            NoiseSearch innovation = new NoiseSearch();
            SplitShapeSearch search2 = new SplitShapeSearch(40, 10, 4, Codebook_Constants.exc_10_16_table, 4, 0);
            SplitShapeSearch search3 = new SplitShapeSearch(40, 10, 4, Codebook_Constants.exc_10_32_table, 5, 0);
            SplitShapeSearch search4 = new SplitShapeSearch(40, 5, 8, Codebook_Constants.exc_5_64_table, 6, 0);
            SplitShapeSearch search5 = new SplitShapeSearch(40, 8, 5, Codebook_Constants.exc_8_128_table, 7, 0);
            SplitShapeSearch search6 = new SplitShapeSearch(40, 5, 8, Codebook_Constants.exc_5_256_table, 8, 0);
            SplitShapeSearch search7 = new SplitShapeSearch(40, 20, 2, Codebook_Constants.exc_20_32_table, 5, 0);
            NbLspQuant lspQuant = new NbLspQuant();
            LbrLspQuant quant2 = new LbrLspQuant();
            SubMode[] modeArray = new SubMode[0x10];
            modeArray[1] = new SubMode(0, 1, 0, 0, quant2, pitch, innovation, 0.7f, 0.7f, -1f, 0x2b);
            modeArray[2] = new SubMode(0, 0, 0, 0, quant2, tap2, search2, 0.7f, 0.5f, 0.55f, 0x77);
            modeArray[3] = new SubMode(-1, 0, 1, 0, quant2, tap3, search3, 0.7f, 0.55f, 0.45f, 160);
            modeArray[4] = new SubMode(-1, 0, 1, 0, quant2, tap4, search5, 0.7f, 0.63f, 0.35f, 220);
            modeArray[5] = new SubMode(-1, 0, 3, 0, lspQuant, ltp, search4, 0.7f, 0.65f, 0.25f, 300);
            modeArray[6] = new SubMode(-1, 0, 3, 0, lspQuant, ltp, search6, 0.68f, 0.65f, 0.1f, 0x16c);
            modeArray[7] = new SubMode(-1, 0, 3, 1, lspQuant, ltp, search4, 0.65f, 0.65f, -1f, 0x1ec);
            modeArray[8] = new SubMode(0, 1, 0, 0, quant2, pitch, search7, 0.7f, 0.5f, 0.65f, 0x4f);
            return modeArray;
        }

        protected virtual void Init(int frameSize, int subframeSize, int lpcSize, int bufSize)
        {
            this.first = 1;
            this.frameSize = frameSize;
            this.windowSize = (frameSize * 3) / 2;
            this.subframeSize = subframeSize;
            this.nbSubframes = frameSize / subframeSize;
            this.lpcSize = lpcSize;
            this.bufSize = bufSize;
            this.min_pitch = 0x11;
            this.max_pitch = 0x90;
            this.preemph = 0f;
            this.pre_mem = 0f;
            this.gamma1 = 0.9f;
            this.gamma2 = 0.6f;
            this.lag_factor = 0.01f;
            this.lpc_floor = 1.0001f;
            this.frmBuf = new float[bufSize];
            this.frmIdx = bufSize - this.windowSize;
            this.excBuf = new float[bufSize];
            this.excIdx = bufSize - this.windowSize;
            this.innov = new float[frameSize];
            this.lpc = new float[lpcSize + 1];
            this.qlsp = new float[lpcSize];
            this.old_qlsp = new float[lpcSize];
            this.interp_qlsp = new float[lpcSize];
            this.interp_qlpc = new float[lpcSize + 1];
            this.mem_sp = new float[5 * lpcSize];
            this.pi_gain = new float[this.nbSubframes];
            this.awk1 = new float[lpcSize + 1];
            this.awk2 = new float[lpcSize + 1];
            this.awk3 = new float[lpcSize + 1];
            this.voc_m1 = this.voc_m2 = this.voc_mean = 0f;
            this.voc_offset = 0;
            this.dtx_enabled = 0;
        }

        private void Nbinit()
        {
            this.submodes = BuildNbSubModes();
            this.submodeID = 5;
            this.Init(160, 40, 10, 640);
        }

        public virtual float[] Exc
        {
            get
            {
                float[] destinationArray = new float[this.frameSize];
                Array.Copy(this.excBuf, this.excIdx, destinationArray, 0, this.frameSize);
                return destinationArray;
            }
        }

        public virtual int FrameSize
        {
            get
            {
                return this.frameSize;
            }
        }

        public virtual float[] Innov
        {
            get
            {
                return this.innov;
            }
        }

        public float[] PiGain
        {
            get
            {
                return this.pi_gain;
            }
        }
    }
}

