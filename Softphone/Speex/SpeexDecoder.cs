namespace Ozeki.Media.Codec.Speex.Implementation
{
    using System;
    using System.Runtime.InteropServices;

    internal class SpeexDecoder
    {
        private readonly Bits bits = new Bits();
        private readonly float[] decodedData;
        private readonly IDecoder decoder;
        private readonly int frameSize;
        private readonly int sampleRate;

        public SpeexDecoder(BandMode mode, bool enhanced = true)
        {
            switch (mode)
            {
                case BandMode.Narrow:
                    this.decoder = new NbDecoder();
                    this.sampleRate = 0x1f40;
                    break;

                case BandMode.Wide:
                    this.decoder = new SbDecoder(false);
                    this.sampleRate = 0x3e80;
                    break;

                case BandMode.UltraWide:
                    this.decoder = new SbDecoder(true);
                    this.sampleRate = 0x7d00;
                    break;

                default:
                    this.decoder = new NbDecoder();
                    this.sampleRate = 0x1f40;
                    break;
            }
            this.decoder.PerceptualEnhancement = enhanced;
            this.frameSize = this.decoder.FrameSize;
            this.decodedData = new float[this.sampleRate * 2];
        }

        public int Decode(byte[] inData, int inOffset, int inCount, short[] outData, int outOffset, bool lostFrame)
        {
            if (lostFrame || (inData == null))
            {
                this.decoder.Decode(null, this.decodedData);
                return this.frameSize;
            }
            this.bits.ReadFrom(inData, inOffset, inCount);
            int num = 0;
            while (this.decoder.Decode(this.bits, this.decodedData) == 0)
            {
                int index = 0;
                while (index < this.frameSize)
                {
                    if (this.decodedData[index] > 32767f)
                    {
                        this.decodedData[index] = 32767f;
                    }
                    else if (this.decodedData[index] < -32768f)
                    {
                        this.decodedData[index] = -32768f;
                    }
                    outData[outOffset] = (this.decodedData[index] > 0f) ? ((short) (this.decodedData[index] + 0.5)) : ((short) (this.decodedData[index] - 0.5));
                    index++;
                    outOffset++;
                }
                num += this.frameSize;
            }
            return num;
        }

        public int FrameSize
        {
            get
            {
                return this.decoder.FrameSize;
            }
        }

        public int SampleRate
        {
            get
            {
                return this.sampleRate;
            }
        }
    }
}

