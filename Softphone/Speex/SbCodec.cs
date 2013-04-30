namespace Ozeki.Media.Codec.Speex.Implementation
{
    using System;

    internal class SbCodec : NbCodec
    {
        protected internal float foldingGain;
        protected internal int fullFrameSize;
        protected internal float[] g0_mem;
        protected internal float[] g1_mem;
        protected internal float[] high;
        public const int QMF_ORDER = 0x40;
        public static readonly int[] SB_FRAME_SIZE = new int[] { 4, 0x24, 0x70, 0xc0, 0x160, -1, -1, -1 };
        public const int SB_SUBMODE_BITS = 3;
        public const int SB_SUBMODES = 8;
        protected internal float[] x0d;
        protected internal float[] y0;
        protected internal float[] y1;

        public SbCodec(bool ultraWide)
        {
            if (ultraWide)
            {
                base.submodes = BuildUwbSubModes();
                base.submodeID = 1;
            }
            else
            {
                base.submodes = BuildWbSubModes();
                base.submodeID = 3;
            }
        }

        protected internal static SubMode[] BuildUwbSubModes()
        {
            HighLspQuant lspQuant = new HighLspQuant();
            SubMode[] modeArray = new SubMode[8];
            modeArray[1] = new SubMode(0, 0, 1, 0, lspQuant, null, null, 0.75f, 0.75f, -1f, 2);
            return modeArray;
        }

        protected internal static SubMode[] BuildWbSubModes()
        {
            HighLspQuant lspQuant = new HighLspQuant();
            SplitShapeSearch innovation = new SplitShapeSearch(40, 10, 4, Codebook_Constants.hexc_10_32_table, 5, 0);
            SplitShapeSearch search2 = new SplitShapeSearch(40, 8, 5, Codebook_Constants.hexc_table, 7, 1);
            SubMode[] modeArray = new SubMode[8];
            modeArray[1] = new SubMode(0, 0, 1, 0, lspQuant, null, null, 0.75f, 0.75f, -1f, 0x24);
            modeArray[2] = new SubMode(0, 0, 1, 0, lspQuant, null, innovation, 0.85f, 0.6f, -1f, 0x70);
            modeArray[3] = new SubMode(0, 0, 1, 0, lspQuant, null, search2, 0.75f, 0.7f, -1f, 0xc0);
            modeArray[4] = new SubMode(0, 0, 1, 1, lspQuant, null, search2, 0.75f, 0.75f, -1f, 0x160);
            return modeArray;
        }

        protected virtual void Init(int frameSize, int subframeSize, int lpcSize, int bufSize, float foldingGain_0)
        {
            base.Init(frameSize, subframeSize, lpcSize, bufSize);
            this.fullFrameSize = 2 * frameSize;
            this.foldingGain = foldingGain_0;
            base.lag_factor = 0.002f;
            this.high = new float[this.fullFrameSize];
            this.y0 = new float[this.fullFrameSize];
            this.y1 = new float[this.fullFrameSize];
            this.x0d = new float[frameSize];
            this.g0_mem = new float[0x40];
            this.g1_mem = new float[0x40];
        }

        public bool Dtx
        {
            get
            {
                return (base.dtx_enabled != 0);
            }
        }

        public override float[] Exc
        {
            get
            {
                float[] numArray = new float[this.fullFrameSize];
                for (int i = 0; i < base.frameSize; i++)
                {
                    numArray[2 * i] = 2f * base.excBuf[base.excIdx + i];
                }
                return numArray;
            }
        }

        public override int FrameSize
        {
            get
            {
                return this.fullFrameSize;
            }
        }

        public override float[] Innov
        {
            get
            {
                return this.Exc;
            }
        }
    }
}

