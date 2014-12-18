using System;
using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Open.P2P.IO;

namespace Open.P2P.Streams
{
    public class PeerStream : Stream
    {
        private readonly CommunicationManager _communicationManager;
        private readonly IPEndPoint _endPoint;

        public PeerStream(CommunicationManager communicationManager, IPEndPoint endPoint)
        {
            _communicationManager = communicationManager;
            _endPoint = endPoint;
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

            return await _communicationManager.ReceiveAsync(buffer, offset, count, _endPoint);
        }

        public override async Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            await _communicationManager.SendAsync(buffer, offset, count, _endPoint);
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
