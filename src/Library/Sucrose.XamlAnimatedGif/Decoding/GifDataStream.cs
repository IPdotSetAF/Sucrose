using System.IO;

namespace Sucrose.XamlAnimatedGif.Decoding
{
    internal class GifDataStream
    {
        public GifHeader Header { get; private set; }
        public GifColor[] GlobalColorTable { get; set; }
        public IList<GifFrame> Frames { get; set; }
        public IList<GifExtension> Extensions { get; set; }
        public ushort RepeatCount { get; set; }

        private GifDataStream()
        {
        }

        internal static async Task<GifDataStream> ReadAsync(Stream stream)
        {
            GifDataStream file = new();
            await file.ReadInternalAsync(stream).ConfigureAwait(false);
            return file;
        }

        private async Task ReadInternalAsync(Stream stream)
        {
            Header = await GifHeader.ReadAsync(stream).ConfigureAwait(false);

            if (Header.LogicalScreenDescriptor.HasGlobalColorTable)
            {
                GlobalColorTable = await GifHelpers.ReadColorTableAsync(stream, Header.LogicalScreenDescriptor.GlobalColorTableSize).ConfigureAwait(false);
            }
            await ReadFramesAsync(stream).ConfigureAwait(false);

            GifApplicationExtension netscapeExtension =
                            Extensions
                                .OfType<GifApplicationExtension>()
                                .FirstOrDefault(GifHelpers.IsNetscapeExtension);

            RepeatCount = netscapeExtension != null
                ? GifHelpers.GetRepeatCount(netscapeExtension)
                : (ushort)1;
        }

        private async Task ReadFramesAsync(Stream stream)
        {
            List<GifFrame> frames = new();
            List<GifExtension> controlExtensions = new();
            List<GifExtension> specialExtensions = new();
            while (true)
            {
                try
                {
                    GifBlock block = await GifBlock.ReadAsync(stream, controlExtensions).ConfigureAwait(false);

                    if (block.Kind == GifBlockKind.GraphicRendering)
                    {
                        controlExtensions = new List<GifExtension>();
                    }

                    if (block is GifFrame frame)
                    {
                        frames.Add(frame);
                    }
                    else if (block is GifExtension extension)
                    {
                        switch (extension.Kind)
                        {
                            case GifBlockKind.Control:
                                controlExtensions.Add(extension);
                                break;
                            case GifBlockKind.SpecialPurpose:
                                specialExtensions.Add(extension);
                                break;

                                // Just discard plain text extensions for now, since we have no use for it
                        }
                    }
                    else if (block is GifTrailer)
                    {
                        break;
                    }
                }
                // Follow the same approach as Firefox:
                // If we find extraneous data between blocks, just assume the stream
                // was successfully terminated if we have some successfully decoded frames
                // https://dxr.mozilla.org/firefox/source/modules/libpr0n/decoders/gif/nsGIFDecoder2.cpp#894-909
                catch (UnknownBlockTypeException) when (frames.Count > 0)
                {
                    break;
                }
            }

            this.Frames = frames.AsReadOnly();
            this.Extensions = specialExtensions.AsReadOnly();
        }
    }
}