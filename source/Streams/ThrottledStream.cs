using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Open.P2P.Progress;

namespace Open.P2P.Streams
{
    internal class ThrottledStream : Stream
    {
        private readonly Stream _stream;
        private readonly IBandwidthController _receiveBandwithController;
        private readonly IBandwidthController _sendBandwithController;

        public ThrottledStream(Stream stream, IBandwidthController receiveBandwithController, IBandwidthController sendBandwithController)
        {
            _stream = stream;
            _receiveBandwithController = receiveBandwithController;
            _sendBandwithController = sendBandwithController;
        }

        public override void Flush()
        {
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotSupportedException();
        }

        public override void SetLength(long value)
        {
            throw new NotSupportedException();
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            var ct = new CancellationToken();
            return ReadAsync(buffer, offset, count, ct).Result;
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            var ct = new CancellationToken();
            WriteAsync(buffer, offset, count, ct).RunSynchronously();
        }

        public override async Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            await _receiveBandwithController.WaitToTransmit(count);

            return await _stream.ReadAsync(buffer, offset, count);
        }

        public override async Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            await _sendBandwithController.WaitToTransmit(count);
            await _stream.WriteAsync(buffer, offset, count);
        }

        public override bool CanRead
        {
            get { return true; }
        }

        public override bool CanSeek
        {
            get { return false; }
        }

        public override bool CanWrite
        {
            get { return true; }
        }

        public override long Length
        {
            get { throw new NotSupportedException(); }
        }

        public override long Position
        {
            get
            {
                throw new NotSupportedException();
            }
            set
            {
                throw new NotSupportedException();
            }
        }
    }
}