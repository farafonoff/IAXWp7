namespace Ozeki.Media.Codec.Speex.Implementation
{
    using System;

    internal class Bits
    {
        private int bitPtr = 0;
        private int bytePtr = 0;
        private byte[] bytes = new byte[0x400];
        public const int DefaultBufferSize = 0x400;
        private int nbBits;

        public void Advance(int n)
        {
            this.bytePtr += n >> 3;
            this.bitPtr += n & 7;
            if (this.bitPtr > 7)
            {
                this.bitPtr -= 8;
                this.bytePtr++;
            }
        }

        public int BitsRemaining()
        {
            return (this.nbBits - ((this.bytePtr * 8) + this.bitPtr));
        }

        public void InsertTerminator()
        {
            if (this.bitPtr > 0)
            {
                this.Pack(0, 1);
            }
            while (this.bitPtr != 0)
            {
                this.Pack(1, 1);
            }
        }

        public void Pack(int data, int nbBits)
        {
            int num = data;
            while ((this.bytePtr + ((nbBits + this.bitPtr) >> 3)) >= this.bytes.Length)
            {
                int num2 = this.bytes.Length * 2;
                byte[] destinationArray = new byte[num2];
                Array.Copy(this.bytes, 0, destinationArray, 0, this.bytes.Length);
                this.bytes = destinationArray;
            }
            while (nbBits > 0)
            {
                int num3 = (num >> (nbBits - 1)) & 1;
                this.bytes[this.bytePtr] = (byte) (this.bytes[this.bytePtr] | ((byte) (num3 << (7 - this.bitPtr))));
                this.bitPtr++;
                if (this.bitPtr == 8)
                {
                    this.bitPtr = 0;
                    this.bytePtr++;
                }
                nbBits--;
            }
        }

        public int Peek()
        {
            return (((this.bytes[this.bytePtr] & 0xff) >> (7 - this.bitPtr)) & 1);
        }

        public void ReadFrom(byte[] newbytes, int offset, int len)
        {
            for (int i = 0; i < len; i++)
            {
                this.bytes[i] = newbytes[offset + i];
            }
            this.bytePtr = 0;
            this.bitPtr = 0;
            this.nbBits = len * 8;
        }

        public void Reset()
        {
            Array.Clear(this.bytes, 0, this.bytes.Length);
            this.bytePtr = 0;
            this.bitPtr = 0;
        }

        public int Unpack(int nbBits)
        {
            int num = 0;
            while (nbBits != 0)
            {
                num = num << 1;
                num |= ((this.bytes[this.bytePtr] & 0xff) >> (7 - this.bitPtr)) & 1;
                this.bitPtr++;
                if (this.bitPtr == 8)
                {
                    this.bitPtr = 0;
                    this.bytePtr++;
                }
                nbBits--;
            }
            return num;
        }

        public int Write(byte[] buffer, int offset, int maxBytes)
        {
            int bitPtr = this.bitPtr;
            int bytePtr = this.bytePtr;
            byte[] bytes = this.bytes;
            this.InsertTerminator();
            this.bitPtr = bitPtr;
            this.bytePtr = bytePtr;
            this.bytes = bytes;
            if (maxBytes > this.BufferSize)
            {
                maxBytes = this.BufferSize;
            }
            Array.Copy(this.bytes, 0, buffer, offset, maxBytes);
            return maxBytes;
        }

        public int BufferSize
        {
            get
            {
                return (this.bytePtr + ((this.bitPtr > 0) ? 1 : 0));
            }
        }
    }
}

