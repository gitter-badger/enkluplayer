using System;
using System.IO;

public static class MemoryStreamExtensions
{
    /// <summary>
    /// Clears a <see cref="MemoryStream"/> and resets it's length to 0.
    /// </summary>
    public static void Clear(this MemoryStream source)
    {
        var buffer = source.GetBuffer();
        Array.Clear(buffer, 0, buffer.Length);
        source.Position = 0;
        source.SetLength(0);
    }

    /// <summary>
    /// Returns the total number of bytes to be read. 
    /// </summary>
    public static long GetBytesRemaining(this MemoryStream source)
    {
        return source.Length - source.Position;
    }

    /// <summary>
    /// Reads from the current position of the <see cref="MemoryStream"/> into a destination <see cref="MemoryStream"/>
    /// using the provided offset and length. 
    /// </summary>
    /// <param name="stream">The memory stream to read from.</param>
    /// <param name="destination">The destination memory stream to write each read byte to.</param>
    /// <param name="destinationOffset">The offset to start writing to.</param>
    /// <param name="length">The total number of bytes to read and write.</param>
    public static void ReadBytes(this MemoryStream stream, MemoryStream destination, long destinationOffset, long length = -1)
    {
        var currentPos = destination.Position;
        if (length <= 0)
        {
            length = stream.GetBytesRemaining();
        }
        else if (length > stream.GetBytesRemaining())
        {
            length = stream.GetBytesRemaining();
        }

        destination.Position = destinationOffset;
        for (var i = 0; i < length; ++i)
        {
            destination.WriteByte((byte) stream.ReadByte());
        }

        destination.Position = currentPos;
    }
}