﻿using IL.RulesBasedOutputCache.Helpers;

namespace IL.RulesBasedOutputCache.StreamExt;

internal sealed class SegmentWriteStream : Stream
{
    private readonly List<byte[]> _segments = new();
    private readonly MemoryStream _bufferStream = new();
    private readonly int _segmentSize;
    private long _length;
    private bool _closed;
    private bool _disposed;

    internal SegmentWriteStream(int segmentSize)
    {
        if (segmentSize <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(segmentSize), segmentSize, $"{nameof(segmentSize)} must be greater than 0.");
        }

        _segmentSize = segmentSize;
    }

    // Extracting the buffered segments closes the stream for writing
    internal List<byte[]> GetSegments()
    {
        if (!_closed)
        {
            _closed = true;
            FinalizeSegments();
        }
        return _segments;
    }

    public override bool CanRead => false;

    public override bool CanSeek => false;

    public override bool CanWrite => !_closed;

    public override long Length => _length;

    public override long Position
    {
        get
        {
            return _length;
        }
        set
        {
            throw new NotSupportedException("The stream does not support seeking.");
        }
    }

    private void DisposeMemoryStream()
    {
        // Clean up the memory stream
        _bufferStream.SetLength(0);
        _bufferStream.Capacity = 0;
        _bufferStream.Dispose();
    }

    private void FinalizeSegments()
    {
        // Append any remaining segments
        if (_bufferStream.Length > 0)
        {
            // Add the last segment
            _segments.Add(_bufferStream.ToArray());
        }

        DisposeMemoryStream();
    }

    protected override void Dispose(bool disposing)
    {
        try
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                _segments.Clear();
                DisposeMemoryStream();
            }

            _disposed = true;
            _closed = true;
        }
        finally
        {
            base.Dispose(disposing);
        }
    }

    public override void Flush()
    {
        if (!CanWrite)
        {
            throw new ObjectDisposedException("The stream has been closed for writing.");
        }
    }

    public override int Read(byte[] buffer, int offset, int count)
    {
        throw new NotSupportedException("The stream does not support reading.");
    }

    public override long Seek(long offset, SeekOrigin origin)
    {
        throw new NotSupportedException("The stream does not support seeking.");
    }

    public override void SetLength(long value)
    {
        throw new NotSupportedException("The stream does not support seeking.");
    }

    public override void Write(byte[] buffer, int offset, int count)
    {
        ArgumentNullException.ThrowIfNull(buffer);

        if (offset < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(offset), offset, "Non-negative number required.");
        }
        if (count < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(count), count, "Non-negative number required.");
        }
        if (count > buffer.Length - offset)
        {
            throw new ArgumentException("Offset and length were out of bounds for the array or count is greater than the number of elements from index to the end of the source collection.");
        }
        if (!CanWrite)
        {
            throw new ObjectDisposedException("The stream has been closed for writing.");
        }

        Write(buffer.AsSpan(offset, count));
    }

    public override void Write(ReadOnlySpan<byte> buffer)
    {
        while (!buffer.IsEmpty)
        {
            if ((int)_bufferStream.Length == _segmentSize)
            {
                _segments.Add(_bufferStream.ToArray());
                _bufferStream.SetLength(0);
            }

            var bytesWritten = Math.Min(buffer.Length, _segmentSize - (int)_bufferStream.Length);

            _bufferStream.Write(buffer[..bytesWritten]);
            buffer = buffer[bytesWritten..];
            _length += bytesWritten;
        }
    }

    public override Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
    {
        Write(buffer, offset, count);
        return Task.CompletedTask;
    }

    public override ValueTask WriteAsync(ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken)
    {
        Write(buffer.Span);
        return default;
    }

    public override void WriteByte(byte value)
    {
        if (!CanWrite)
        {
            throw new ObjectDisposedException("The stream has been closed for writing.");
        }

        if ((int)_bufferStream.Length == _segmentSize)
        {
            _segments.Add(_bufferStream.ToArray());
            _bufferStream.SetLength(0);
        }

        _bufferStream.WriteByte(value);
        _length++;
    }

    public override IAsyncResult BeginWrite(byte[] buffer, int offset, int count, AsyncCallback? callback, object? state)
        => TaskToApm.Begin(WriteAsync(buffer, offset, count, CancellationToken.None), callback, state);

    public override void EndWrite(IAsyncResult asyncResult)
        => TaskToApm.End(asyncResult);
}