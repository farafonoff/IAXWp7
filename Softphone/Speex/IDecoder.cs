namespace Ozeki.Media.Codec.Speex.Implementation
{
    using System;

    internal interface IDecoder
    {
        int Decode(Bits bits, float[] xout);
        void DecodeStereo(float[] data, int frameSize);

        bool Dtx { get; }

        float[] Exc { get; }

        int FrameSize { get; }

        float[] Innov { get; }

        bool PerceptualEnhancement { get; set; }

        float[] PiGain { get; }
    }
}

