namespace Ozeki.Media.Codec.Speex.Implementation
{
    using System;

    internal class SpeexJitterBuffer
    {
        private readonly JitterBuffer buffer = new JitterBuffer();
        private readonly SpeexDecoder decoder;
        private JitterBuffer.JitterBufferPacket inPacket;
        private JitterBuffer.JitterBufferPacket outPacket;

        public SpeexJitterBuffer(SpeexDecoder decoder)
        {
            this.decoder = decoder;
            this.inPacket.sequence = 0L;
            this.inPacket.span = 1L;
            this.inPacket.timestamp = 1L;
            this.buffer.DestroyBufferCallback = delegate (byte[] x) {
            };
            this.buffer.Init(1);
        }

        public void Get(short[] decodedFrame)
        {
            int num;
            if (this.outPacket.data == null)
            {
                this.outPacket.data = new byte[decodedFrame.Length * 2];
            }
            else
            {
                Array.Clear(this.outPacket.data, 0, this.outPacket.data.Length);
            }
            this.outPacket.len = this.outPacket.data.Length;
            if (this.buffer.Get(ref this.outPacket, 1, out num) != 0)
            {
                this.decoder.Decode(null, 0, 0, decodedFrame, 0, true);
            }
            else
            {
                this.decoder.Decode(this.outPacket.data, 0, this.outPacket.len, decodedFrame, 0, false);
            }
            this.buffer.Tick();
        }

        public void Put(byte[] frameData)
        {
            this.inPacket.data = frameData;
            this.inPacket.len = frameData.Length;
            this.inPacket.timestamp += 1L;
            this.buffer.Put(this.inPacket);
        }
    }
}

