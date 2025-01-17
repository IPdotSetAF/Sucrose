﻿using Sucrose.XamlAnimatedGif.Decoding;
using Sucrose.XamlAnimatedGif.Decompression;
using Sucrose.XamlAnimatedGif.Extensions;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;

namespace Sucrose.XamlAnimatedGif
{
    public abstract class Animator : DependencyObject, IDisposable
    {
        private readonly Stream _sourceStream;
        private readonly Uri _sourceUri;
        private readonly bool _isSourceStreamOwner;
        private readonly GifDataStream _metadata;
        private readonly Dictionary<int, GifPalette> _palettes;
        private readonly WriteableBitmap _bitmap;
        private readonly int _stride;
        private readonly byte[] _previousBackBuffer;
        private readonly byte[] _indexStreamBuffer;
        private readonly TimingManager _timingManager;
        private readonly bool _cacheFrameDataInMemory;
        private readonly byte[][] _cachedFrameBytes;
        private readonly Task _loadFramesDataTask;

        #region Constructor and factory methods

        internal Animator(Stream sourceStream, Uri sourceUri, GifDataStream metadata, RepeatBehavior repeatBehavior,
            bool cacheFrameDataInMemory)
        {
            _sourceStream = sourceStream;
            _sourceUri = sourceUri;
            _isSourceStreamOwner = sourceUri != null; // stream opened from URI, should close it
            _metadata = metadata;
            _palettes = CreatePalettes(metadata);
            _bitmap = CreateBitmap(metadata);
            GifLogicalScreenDescriptor desc = metadata.Header.LogicalScreenDescriptor;
            _stride = 4 * (((desc.Width * 32) + 31) / 32);
            _previousBackBuffer = new byte[desc.Height * _stride];
            _indexStreamBuffer = CreateIndexStreamBuffer(metadata, _sourceStream);
            _timingManager = CreateTimingManager(metadata, repeatBehavior);

            _cacheFrameDataInMemory = cacheFrameDataInMemory;

            if (cacheFrameDataInMemory)
            {
                _cachedFrameBytes = new byte[_metadata.Frames.Count][];
                _loadFramesDataTask = Task.Run(LoadFrames);
            }
        }

        private async Task LoadFrames()
        {
            long biggestFrameSize = 0L;
            for (int frameIndex = 0; frameIndex < _metadata.Frames.Count; frameIndex++)
            {
                long startPosition = _metadata.Frames[frameIndex].ImageData.CompressedDataStartOffset;
                long endPosition = _metadata.Frames.Count == frameIndex + 1
                    ? _sourceStream.Length
                    : _metadata.Frames[frameIndex + 1].ImageData.CompressedDataStartOffset - 1;
                long size = endPosition - startPosition;
                biggestFrameSize = Math.Max(size, biggestFrameSize);
            }

            byte[] indexCompressedBytes = new byte[biggestFrameSize];
            for (int frameIndex = 0; frameIndex < _metadata.Frames.Count; frameIndex++)
            {
                GifFrame frame = _metadata.Frames[frameIndex];
                GifImageDescriptor frameDesc = _metadata.Frames[frameIndex].Descriptor;
                await GetIndexBytesAsync(frameIndex, indexCompressedBytes);
                using LzwDecompressStream indexDecompressedStream =
                    new(indexCompressedBytes, frame.ImageData.LzwMinimumCodeSize);
                _cachedFrameBytes[frameIndex] = new byte[frameDesc.Width * frameDesc.Height];

                await indexDecompressedStream.ReadAllAsync(_cachedFrameBytes[frameIndex], 0, frameDesc.Width * frameDesc.Height);
            }
        }

        internal static async Task<TAnimator> CreateAsyncCore<TAnimator>(
            Uri sourceUri,
            IProgress<int> progress,
            Func<Stream, GifDataStream, TAnimator> create)
            where TAnimator : Animator
        {
            Stream stream = await UriLoader.GetStreamFromUriAsync(sourceUri, progress);
            try
            {
                // ReSharper disable once AccessToDisposedClosure
                return await CreateAsyncCore(stream, metadata => create(stream, metadata));
            }
            catch
            {
                stream?.Dispose();
                throw;
            }
        }

        internal static async Task<TAnimator> CreateAsyncCore<TAnimator>(
            Stream sourceStream,
            Func<GifDataStream, TAnimator> create)
            where TAnimator : Animator
        {
            if (!sourceStream.CanSeek)
            {
                throw new ArgumentException("The stream is not seekable");
            }

            sourceStream.Seek(0, SeekOrigin.Begin);
            GifDataStream metadata = await GifDataStream.ReadAsync(sourceStream);
            return create(metadata);
        }

        #endregion

        #region Animation

        public int FrameCount => _metadata.Frames.Count;

        private bool _isStarted;
        private CancellationTokenSource _cancellationTokenSource;

        public async void Play()
        {
            try
            {
                if (_timingManager.IsComplete)
                {
                    _timingManager.Reset();
                    _isStarted = false;
                }

                if (!_isStarted)
                {
                    _cancellationTokenSource?.Dispose();
                    _cancellationTokenSource = new CancellationTokenSource();
                    _isStarted = true;
                    OnAnimationStarted();
                    if (_timingManager.IsPaused)
                    {
                        _timingManager.Resume();
                    }

                    await RunAsync(_cancellationTokenSource.Token);
                }
                else if (_timingManager.IsPaused)
                {
                    _timingManager.Resume();
                }
            }
            catch (OperationCanceledException)
            {
            }
            catch (Exception ex)
            {
                // ignore errors that might occur during Dispose
                if (!_disposing)
                {
                    OnError(ex, AnimationErrorKind.Rendering);
                }
            }
        }

        private int _frameIndex;
        private async Task RunAsync(CancellationToken cancellationToken)
        {
            if (_loadFramesDataTask != null)
            {
                await _loadFramesDataTask;
            }

            while (true)
            {
                cancellationToken.ThrowIfCancellationRequested();
                Task<bool> timing = _timingManager.NextAsync(cancellationToken);
                Task rendering = RenderFrameAsync(CurrentFrameIndex, cancellationToken);
                await Task.WhenAll(timing, rendering);
                if (!timing.Result)
                {
                    break;
                }

                CurrentFrameIndex = (CurrentFrameIndex + 1) % FrameCount;
            }
        }

        public void Pause()
        {
            _timingManager.Pause();
        }

        public void Resume()
        {
            _timingManager.Resume();
        }

        public bool IsPaused => _timingManager.IsPaused;

        public bool IsComplete
        {
            get
            {
                if (_isStarted)
                {
                    return _timingManager.IsComplete;
                }

                return false;
            }
        }

        public event EventHandler CurrentFrameChanged;

        protected virtual void OnCurrentFrameChanged()
        {
            CurrentFrameChanged?.Invoke(this, EventArgs.Empty);
        }

        public event EventHandler<AnimationStartedEventArgs> AnimationStarted;

        protected virtual void OnAnimationStarted()
        {
            AnimationStarted?.Invoke(this, new AnimationStartedEventArgs(AnimationSource));
        }

        public event EventHandler<AnimationCompletedEventArgs> AnimationCompleted;

        protected virtual void OnAnimationCompleted()
        {
            AnimationCompleted?.Invoke(this, new AnimationCompletedEventArgs(AnimationSource));
        }

        public event EventHandler<AnimationErrorEventArgs> Error;

        protected virtual void OnError(Exception ex, AnimationErrorKind kind)
        {
            Error?.Invoke(this, new AnimationErrorEventArgs(AnimationSource, ex, kind));
        }

        public int CurrentFrameIndex
        {
            get => _frameIndex;
            private set
            {
                _frameIndex = value;
                OnCurrentFrameChanged();
            }
        }

        private TimingManager CreateTimingManager(GifDataStream metadata, RepeatBehavior repeatBehavior)
        {
            RepeatBehavior actualRepeatBehavior = GetActualRepeatBehavior(metadata, repeatBehavior);

            TimingManager manager = new(actualRepeatBehavior);
            foreach (GifFrame frame in metadata.Frames)
            {
                manager.Add(GetFrameDelay(frame));
            }

            manager.Completed += TimingManagerCompleted;
            return manager;
        }

        private static RepeatBehavior GetActualRepeatBehavior(GifDataStream metadata, RepeatBehavior repeatBehavior)
        {
            return repeatBehavior == default
                    ? GetRepeatBehaviorFromGif(metadata)
                    : repeatBehavior;
        }

        protected abstract RepeatBehavior GetSpecifiedRepeatBehavior();

        private void TimingManagerCompleted(object sender, EventArgs e)
        {
            OnAnimationCompleted();
        }

        #endregion

        #region Rendering

        private static WriteableBitmap CreateBitmap(GifDataStream metadata)
        {
            GifLogicalScreenDescriptor desc = metadata.Header.LogicalScreenDescriptor;
            WriteableBitmap bitmap = new(desc.Width, desc.Height, 96, 96, PixelFormats.Bgra32, null);
            return bitmap;
        }

        private static Dictionary<int, GifPalette> CreatePalettes(GifDataStream metadata)
        {
            Dictionary<int, GifPalette> palettes = new();
            Color[] globalColorTable = null;
            if (metadata.Header.LogicalScreenDescriptor.HasGlobalColorTable)
            {
                globalColorTable =
                    metadata.GlobalColorTable
                        .Select(gc => Color.FromArgb(0xFF, gc.R, gc.G, gc.B))
                        .ToArray();
            }

            for (int i = 0; i < metadata.Frames.Count; i++)
            {
                GifFrame frame = metadata.Frames[i];
                Color[] colorTable = globalColorTable;
                if (frame.Descriptor.HasLocalColorTable)
                {
                    colorTable =
                        frame.LocalColorTable
                            .Select(gc => Color.FromArgb(0xFF, gc.R, gc.G, gc.B))
                            .ToArray();
                }

                int? transparencyIndex = null;
                GifGraphicControlExtension gce = frame.GraphicControl;
                if (gce is { HasTransparency: true })
                {
                    transparencyIndex = gce.TransparencyIndex;
                }

                palettes[i] = new GifPalette(transparencyIndex, colorTable);
            }

            return palettes;
        }

        private static byte[] CreateIndexStreamBuffer(GifDataStream metadata, Stream stream)
        {
            // Find the size of the largest frame pixel data
            // (ignoring the fact that we include the next frame's header)

            long lastSize = stream.Length - metadata.Frames.Last().ImageData.CompressedDataStartOffset;
            long maxSize = lastSize;
            if (metadata.Frames.Count > 1)
            {
                IEnumerable<long> sizes = metadata.Frames.Zip(metadata.Frames.Skip(1),
                    (f1, f2) => f2.ImageData.CompressedDataStartOffset - f1.ImageData.CompressedDataStartOffset);
                maxSize = Math.Max(sizes.Max(), lastSize);
            }
            // Need 4 extra bytes so that BitReader doesn't need to check the size for every read
            return new byte[maxSize + 4];
        }

        private int _previousFrameIndex;
        private GifFrame _previousFrame;

        private async Task RenderFrameAsync(int frameIndex, CancellationToken cancellationToken)
        {
            if (frameIndex < 0)
            {
                return;
            }

            GifFrame frame = _metadata.Frames[frameIndex];
            GifImageDescriptor desc = frame.Descriptor;
            Int32Rect rect = GetFixedUpFrameRect(desc);

            Stream indexStream = null;
            if (!_cacheFrameDataInMemory)
            {
                indexStream = await GetIndexStreamAsync(frame, cancellationToken);
            }
            using (indexStream)
            using (_bitmap.LockInScope())
            {
                if (frameIndex < _previousFrameIndex)
                {
                    ClearArea(_metadata.Header.LogicalScreenDescriptor);
                }
                else
                {
                    DisposePreviousFrame(frame);
                }

                int bufferLength = 4 * rect.Width;
                byte[] indexBuffer;
                byte[] lineBuffer = new byte[bufferLength];

                GifPalette palette = _palettes[frameIndex];
                int transparencyIndex = palette.TransparencyIndex ?? -1;

                int[] rows = desc.Interlace
                    ? InterlacedRows(rect.Height).ToArray()
                    : NormalRows(rect.Height).ToArray();

                if (!_cacheFrameDataInMemory)
                {
                    indexBuffer = new byte[desc.Width * desc.Height];
                    await indexStream.ReadAllAsync(indexBuffer, 0, indexBuffer.Length, cancellationToken);
                }
                else
                {
                    indexBuffer = _cachedFrameBytes[frameIndex];
                }

                for (int y = 0; y < rect.Height; y++)
                {
                    int offset = ((desc.Top + rows[y]) * _stride) + (desc.Left * 4);

                    if (transparencyIndex >= 0)
                    {
                        CopyFromBitmap(lineBuffer, _bitmap, offset, bufferLength);
                    }

                    for (int x = 0; x < rect.Width; x++)
                    {
                        byte index = indexBuffer[x + (y * desc.Width)];
                        int i = 4 * x;
                        if (index != transparencyIndex)
                        {
                            WriteColor(lineBuffer, palette[index], i);
                        }
                    }
                    CopyToBitmap(lineBuffer, _bitmap, offset, bufferLength);
                }
                _bitmap.AddDirtyRect(rect);
            }

            _previousFrame = frame;
            _previousFrameIndex = frameIndex;
        }

        private static IEnumerable<int> NormalRows(int height)
        {
            return Enumerable.Range(0, height);
        }

        private static IEnumerable<int> InterlacedRows(int height)
        {
            /*
             * 4 passes:
             * Pass 1: rows 0, 8, 16, 24...
             * Pass 2: rows 4, 12, 20, 28...
             * Pass 3: rows 2, 6, 10, 14...
             * Pass 4: rows 1, 3, 5, 7...
             * */
            var passes = new[]
            {
                new { Start = 0, Step = 8 },
                new { Start = 4, Step = 8 },
                new { Start = 2, Step = 4 },
                new { Start = 1, Step = 2 }
            };
            foreach (var pass in passes)
            {
                int y = pass.Start;
                while (y < height)
                {
                    yield return y;
                    y += pass.Step;
                }
            }
        }

        private static void CopyToBitmap(byte[] buffer, WriteableBitmap bitmap, int offset, int length)
        {
            Marshal.Copy(buffer, 0, bitmap.BackBuffer + offset, length);
        }

        private static void CopyFromBitmap(byte[] buffer, WriteableBitmap bitmap, int offset, int length)
        {
            Marshal.Copy(bitmap.BackBuffer + offset, buffer, 0, length);
        }

        private static void WriteColor(byte[] lineBuffer, Color color, int startIndex)
        {
            lineBuffer[startIndex] = color.B;
            lineBuffer[startIndex + 1] = color.G;
            lineBuffer[startIndex + 2] = color.R;
            lineBuffer[startIndex + 3] = color.A;
        }

        private void DisposePreviousFrame(GifFrame currentFrame)
        {
            GifGraphicControlExtension pgce = _previousFrame?.GraphicControl;
            if (pgce != null)
            {
                switch (pgce.DisposalMethod)
                {
                    case GifFrameDisposalMethod.None:
                    case GifFrameDisposalMethod.DoNotDispose:
                        {
                            // Leave previous frame in place
                            break;
                        }
                    case GifFrameDisposalMethod.RestoreBackground:
                        {
                            ClearArea(GetFixedUpFrameRect(_previousFrame.Descriptor));
                            break;
                        }
                    case GifFrameDisposalMethod.RestorePrevious:
                        {
                            CopyToBitmap(_previousBackBuffer, _bitmap, 0, _previousBackBuffer.Length);
                            GifLogicalScreenDescriptor desc = _metadata.Header.LogicalScreenDescriptor;
                            Int32Rect rect = new(0, 0, desc.Width, desc.Height);
                            _bitmap.AddDirtyRect(rect);
                            break;
                        }
                }
            }

            GifGraphicControlExtension gce = currentFrame.GraphicControl;
            if (gce is { DisposalMethod: GifFrameDisposalMethod.RestorePrevious })
            {
                CopyFromBitmap(_previousBackBuffer, _bitmap, 0, _previousBackBuffer.Length);
            }
        }

        private void ClearArea(IGifRect rect)
        {
            ClearArea(new Int32Rect(rect.Left, rect.Top, rect.Width, rect.Height));
        }

        private void ClearArea(Int32Rect rect)
        {
            int bufferLength = 4 * rect.Width;
            byte[] lineBuffer = new byte[bufferLength];
            for (int y = 0; y < rect.Height; y++)
            {
                int offset = ((rect.Y + y) * _stride) + (4 * rect.X);
                CopyToBitmap(lineBuffer, _bitmap, offset, bufferLength);
            }

            _bitmap.AddDirtyRect(new Int32Rect(rect.X, rect.Y, rect.Width, rect.Height));
        }

        private async Task<Stream> GetIndexStreamAsync(GifFrame frame, CancellationToken cancellationToken)
        {
            GifImageData data = frame.ImageData;
            cancellationToken.ThrowIfCancellationRequested();
            _sourceStream.Seek(data.CompressedDataStartOffset, SeekOrigin.Begin);
            using (MemoryStream ms = new(_indexStreamBuffer))
            {
                await GifHelpers.CopyDataBlocksToStreamAsync(_sourceStream, ms, cancellationToken).ConfigureAwait(false);
            }
            LzwDecompressStream lzwStream = new(_indexStreamBuffer, data.LzwMinimumCodeSize);
            return lzwStream;
        }

        private async Task GetIndexBytesAsync(int frameIndex, byte[] buffer)
        {
            long startPosition = _metadata.Frames[frameIndex].ImageData.CompressedDataStartOffset;

            _sourceStream.Seek(startPosition, SeekOrigin.Begin);
            using MemoryStream memoryStream = new(buffer);
            await GifHelpers.CopyDataBlocksToStreamAsync(_sourceStream, memoryStream).ConfigureAwait(false);
        }

        internal BitmapSource Bitmap => _bitmap;

        #endregion

        #region Helper methods

        private static TimeSpan GetFrameDelay(GifFrame frame)
        {
            GifGraphicControlExtension gce = frame.GraphicControl;
            if (gce != null)
            {
                if (gce.Delay != 0)
                {
                    return TimeSpan.FromMilliseconds(gce.Delay);
                }
            }
            return TimeSpan.FromMilliseconds(100);
        }

        private static RepeatBehavior GetRepeatBehaviorFromGif(GifDataStream metadata)
        {
            if (metadata.RepeatCount == 0)
            {
                return RepeatBehavior.Forever;
            }

            return new RepeatBehavior(metadata.RepeatCount);
        }

        private Int32Rect GetFixedUpFrameRect(GifImageDescriptor desc)
        {
            int width = Math.Min(desc.Width, _bitmap.PixelWidth - desc.Left);
            int height = Math.Min(desc.Height, _bitmap.PixelHeight - desc.Top);
            return new Int32Rect(desc.Left, desc.Top, width, height);
        }

        #endregion

        #region Finalizer and Dispose

        ~Animator()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private volatile bool _disposing;
        private bool _disposed;
        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                _disposing = true;
                if (_timingManager != null)
                {
                    _timingManager.Completed -= TimingManagerCompleted;
                }

                _cancellationTokenSource?.Cancel();
                if (_isSourceStreamOwner)
                {
                    try
                    {
                        _sourceStream?.Dispose();
                    }
                    catch
                    {
                        /* ignored */
                    }
                }
                _disposed = true;
            }
        }

        #endregion

        public override string ToString()
        {
            string s = _sourceUri?.ToString() ?? _sourceStream.ToString();
            return "GIF: " + s;
        }

        class GifPalette
        {
            private readonly Color[] _colors;

            public GifPalette(int? transparencyIndex, Color[] colors)
            {
                TransparencyIndex = transparencyIndex;
                _colors = colors;
            }

            public int? TransparencyIndex { get; }

            public Color this[int i] => _colors[i];
        }

        internal async Task ShowFirstFrameAsync()
        {
            try
            {
                if (_loadFramesDataTask != null)
                {
                    await _loadFramesDataTask;
                }

                await RenderFrameAsync(0, CancellationToken.None);
                CurrentFrameIndex = 0;
                _timingManager.Pause();
            }
            catch (Exception ex)
            {
                OnError(ex, AnimationErrorKind.Rendering);
            }
        }

        public async void Rewind()
        {
            CurrentFrameIndex = 0;
            bool isStopped = _timingManager.IsPaused || _timingManager.IsComplete;
            _timingManager.Reset();
            if (isStopped)
            {
                _timingManager.Pause();
                _isStarted = false;
                try
                {
                    await RenderFrameAsync(0, CancellationToken.None);
                }
                catch (Exception ex)
                {
                    OnError(ex, AnimationErrorKind.Rendering);
                }
            }
        }

        protected abstract object AnimationSource { get; }

        internal void OnRepeatBehaviorChanged()
        {
            if (_timingManager == null)
            {
                return;
            }

            RepeatBehavior newValue = GetSpecifiedRepeatBehavior();
            RepeatBehavior newActualValue = GetActualRepeatBehavior(_metadata, newValue);
            if (_timingManager.RepeatBehavior == newActualValue)
            {
                return;
            }

            _timingManager.RepeatBehavior = newActualValue;
            Rewind();
        }
    }
}