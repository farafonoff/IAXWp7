namespace Ozeki.Media.Codec.Speex.Implementation
{
    using System;

    internal interface IEncoder
    {
        int Encode(Bits bits, float[] ins0);

        int Abr { get; set; }

        int BitRate { get; set; }

        int Complexity { get; set; }

        bool Dtx { get; set; }

        int EncodedFrameSize { get; }

        float[] Exc { get; }

        int FrameSize { get; }

        float[] Innov { get; }

        int LookAhead { get; }

        int Mode { get; set; }

        float[] PiGain { get; }

        int Quality { set; }

        float RelativeQuality { get; }

        int SamplingRate { get; set; }

        bool Vad { get; set; }

        bool Vbr { get; set; }

        float VbrQuality { get; set; }
    }
}

