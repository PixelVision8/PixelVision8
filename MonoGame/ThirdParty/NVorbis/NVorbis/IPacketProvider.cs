/****************************************************************************
 * NVorbis                                                                  *
 * Copyright (C) 2014, Andrew Ward <afward@gmail.com>                       *
 *                                                                          *
 * See COPYING for license terms (Ms-PL).                                   *
 *                                                                          *
 ***************************************************************************/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NVorbis
{
    /// <summary>
    /// Provides packets on-demand for the Vorbis stream decoder.
    /// </summary>
    public interface IPacketProvider : IDisposable
    {
        /// <summary>
        /// Gets the serial number associated with this stream.
        /// </summary>
        int StreamSerial { get; }

        /// <summary>
        /// Gets whether seeking is supported on this stream.
        /// </summary>
        bool CanSeek { get; }

        /// <summary>
        /// Gets the number of bits of overhead in this stream's container.
        /// </summary>
        long ContainerBits { get; }

        /// <summary>
        /// Retrieves the total number of pages (or frames) this stream uses.
        /// </summary>
        /// <returns>The page count.</returns>
        /// <exception cref="InvalidOperationException"><see cref="CanSeek"/> is <c>False</c>.</exception>
        int GetTotalPageCount();

        /// <summary>
        /// Retrieves the next packet in the stream.
        /// </summary>
        /// <returns>The next packet in the stream or <c>null</c> if no more packets.</returns>
        DataPacket GetNextPacket();

        /// <summary>
        /// Retrieves the next packet in the stream but does not advance to the following packet.
        /// </summary>
        /// <returns>The next packet in the stream or <c>null</c> if no more packets.</returns>
        DataPacket PeekNextPacket();

        /// <summary>
        /// Retrieves the packet specified from the stream.
        /// </summary>
        /// <param name="packetIndex">The index of the packet to retrieve.</param>
        /// <returns>The specified packet.</returns>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="packetIndex"/> is less than 0 or past the end of the stream.</exception>
        /// <exception cref="InvalidOperationException"><see cref="CanSeek"/> is <c>False</c>.</exception>
        DataPacket GetPacket(int packetIndex);

        /// <summary>
        /// Retrieves the total number of granules in this Vorbis stream.
        /// </summary>
        /// <returns>The number of samples</returns>
        /// <exception cref="InvalidOperationException"><see cref="CanSeek"/> is <c>False</c>.</exception>
        long GetGranuleCount();

        /// <summary>
        /// Finds the packet index to the granule position specified in the current stream.
        /// </summary>
        /// <param name="granulePos">The granule position to seek to.</param>
        /// <param name="packetGranuleCountCallback">A callback method that takes the current and previous packets and returns the number of granules in the current packet.</param>
        /// <returns>The index of the packet that includes the specified granule position or -1 if none found.</returns>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="granulePos"/> is less than 0 or is after the last granule.</exception>
        DataPacket FindPacket(long granulePos, Func<DataPacket, DataPacket, int> packetGranuleCountCallback);

        /// <summary>
        /// Sets the next packet to be returned, applying a pre-roll as necessary.
        /// </summary>
        /// <param name="packet">The packet to key from.</param>
        /// <param name="preRoll">The number of packets to return before the indicated packet.</param>
        void SeekToPacket(DataPacket packet, int preRoll);

        /// <summary>
        /// Occurs when the stream is about to change parameters.
        /// </summary>
        event EventHandler<ParameterChangeEventArgs> ParameterChange;
    }
}
