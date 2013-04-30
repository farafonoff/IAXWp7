namespace Ozeki.Media.Codec
{
    using Ozeki.Media.Codec.Speex.Implementation;
    using System;
    using System.IO;

    internal abstract class CodecSpeexBase
    {
        protected readonly Ozeki.Media.Codec.Speex.Implementation.SpeexDecoder SpeexDecoder;
        protected readonly Ozeki.Media.Codec.Speex.Implementation.SpeexEncoder SpeexEncoder;

        protected CodecSpeexBase(BandMode bandMode, int quality)
        {
            this.SpeexEncoder = new Ozeki.Media.Codec.Speex.Implementation.SpeexEncoder(bandMode);
            this.SpeexEncoder.Quality = quality;
            this.SpeexDecoder = new Ozeki.Media.Codec.Speex.Implementation.SpeexDecoder(bandMode, true);
        }

        public byte[] Decode(byte[] data)
        {
            short[] outData = new short[this.SpeexDecoder.FrameSize * 2];
            int num = this.SpeexDecoder.Decode(data, 0, data.Length, outData, 0, false);
            using (MemoryStream stream = new MemoryStream())
            {
                for (int i = 0; i < num; i++)
                {
                    stream.Write(BitConverter.GetBytes(outData[i]), 0, 2);
                }
                return stream.ToArray();
            }
        }

        public byte[] Encode(byte[] data)
        {
            short[] inData = data.ToShortArray();
            byte[] outData = new byte[data.Length];
            int length = this.SpeexEncoder.Encode(inData, 0, inData.Length, outData, 0, data.Length);
            if (length == 0)
            {
                throw new InvalidOperationException();
            }
            byte[] destinationArray = new byte[length];
            Array.Copy(outData, destinationArray, length);
            return destinationArray;
        }

        public int BitRate
        {
            get
            {
                return 8;
            }
        }

        public string Name
        {
            get
            {
                return "SPEEX";
            }
        }

        public int PacketizationTime
        {
            get
            {
                return 20;
            }
        }

        public abstract int SampleRate { get; }
    }
}

