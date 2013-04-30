namespace Ozeki.Media.Codec.Speex.Implementation
{
    using System;

    internal class InvalidFormatException : Exception
    {
        public InvalidFormatException(string message) : base(message)
        {
        }
    }
}

