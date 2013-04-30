namespace Ozeki.Media.Codec.Speex.Implementation
{
    using System;

    internal class SpeexEncoder
    {
        private readonly Bits bits = new Bits();
        private readonly IEncoder encoder;
        private readonly int frameSize;
        private readonly float[] rawData;

        public SpeexEncoder(BandMode mode)
        {
            switch (mode)
            {
                case BandMode.Narrow:
                    this.encoder = new NbEncoder();
                    break;

                case BandMode.Wide:
                    this.encoder = new SbEncoder(false);
                    break;

                case BandMode.UltraWide:
                    this.encoder = new SbEncoder(true);
                    break;

                default:
                    throw new ArgumentException("Invalid mode", "mode");
            }
            this.frameSize = this.encoder.FrameSize;
            this.rawData = new float[this.frameSize];
        }

        public int Encode(short[] inData, int inOffset, int inCount, byte[] outData, int outOffset, int outCount)
        {
            this.bits.Reset();
            int num = 0;
            int num2 = 0;
            while (num < inCount)
            {
                for (int i = 0; i < this.frameSize; i++)
                {
                    this.rawData[i] = inData[(inOffset + i) + num];
                }
                num2 += this.encoder.Encode(this.bits, this.rawData);
                num += this.frameSize;
            }
            if (num2 == 0)
            {
                return 0;
            }
            return this.bits.Write(outData, outOffset, outCount);
        }

        public int FrameSize
        {
            get
            {
                return this.frameSize;
            }
        }

        public int Quality
        {
            set
            {
                this.encoder.Quality = value;
            }
        }

        public int SampleRate
        {
            get
            {
                return this.encoder.SamplingRate;
            }
        }

        public bool VBR
        {
            get
            {
                return this.encoder.Vbr;
            }
            set
            {
                this.encoder.Vbr = true;
            }
        }
    }
}

