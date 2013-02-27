using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Audio;

namespace Softphone
{
    public class SampleEventArgs
    {
        public byte[] buffer;
        public Microphone src;
        public int bytes;
    }
}
