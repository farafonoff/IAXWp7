namespace Ozeki.Media.Codec
{
    using System;
    using Ozeki.Media.Codec.Speex.Implementation;

    internal class CodecSpeexNarrowband : CodecSpeexBase
    {
        public CodecSpeexNarrowband() : base(BandMode.Narrow, 6)
        {
        }

        public override int SampleRate
        {
            get
            {
                return 0x1f40;
            }
        }
    }
}

