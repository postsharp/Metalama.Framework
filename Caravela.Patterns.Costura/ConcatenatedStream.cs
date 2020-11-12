using System;
using System.Collections.Generic;
using System.IO;

namespace Caravela.Patterns.Costura
{
    /// <summary>
    /// Multiple streams rolled into one. Read-only. Comes from https://stackoverflow.com/a/3879231/1580088.
    /// </summary>
    class ConcatenatedStream : Stream
    {
        readonly Queue<Stream> streams;
        private readonly Stream[] allStreams;

        public ConcatenatedStream(Stream[] streams)
        {
            this.allStreams = streams;
            this.streams = new Queue<Stream>(streams);
        }

        public override bool CanRead
        {
            get { return true; }
        }

        public void ResetAllToZero()
        {
            foreach (Stream stream in allStreams)
            {
                stream.Position = 0;
            }
        }
        
        public override int Read(byte[] buffer, int offset, int count)
        {
            int totalBytesRead = 0;

            while (count > 0 && streams.Count > 0)
            {
                int bytesRead = streams.Peek().Read(buffer, offset, count);
                if (bytesRead == 0)
                {
                    streams.Dequeue();
                    continue;
                }

                totalBytesRead += bytesRead;
                offset += bytesRead;
                count -= bytesRead;
            }

            return totalBytesRead;
        }

        public override bool CanSeek
        {
            get { return false; }
        }

        public override bool CanWrite
        {
            get { return false; }
        }

        public override void Flush()
        {
            throw new NotImplementedException();
        }

        public override long Length
        {
            get { throw new NotImplementedException(); }
        }

        public override long Position
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotImplementedException();
        }

        public override void SetLength(long value)
        {
            throw new NotImplementedException();
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new NotImplementedException();
        }
    }
}