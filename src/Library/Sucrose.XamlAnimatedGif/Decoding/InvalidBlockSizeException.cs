using System.Runtime.Serialization;

namespace Sucrose.XamlAnimatedGif.Decoding
{
    [Serializable]
    public class InvalidBlockSizeException : GifDecoderException
    {
        internal InvalidBlockSizeException(string message) : base(message) { }
        internal InvalidBlockSizeException(string message, Exception inner) : base(message, inner) { }

        protected InvalidBlockSizeException(
            SerializationInfo info,
            StreamingContext context)
            : base(info, context) { }
    }
}