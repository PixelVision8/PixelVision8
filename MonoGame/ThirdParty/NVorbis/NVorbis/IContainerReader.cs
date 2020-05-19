/****************************************************************************
 * NVorbis                                                                  *
 * Copyright (C) 2014, Andrew Ward <afward@gmail.com>                       *
 *                                                                          *
 * See COPYING for license terms (Ms-PL).                                   *
 *                                                                          *
 ***************************************************************************/
using System;

namespace NVorbis
{
    /// <summary>
    /// Provides a interface for a Vorbis logical stream container.
    /// </summary>
    public interface IContainerReader : IDisposable
    {
        /// <summary>
        /// Gets the list of stream serials found in the container so far.
        /// </summary>
        int[] StreamSerials { get; }

        /// <summary>
        /// Gets whether the container supports seeking.
        /// </summary>
        bool CanSeek { get; }

        /// <summary>
        /// Gets the number of bits in the container that are not associated with a logical stream.
        /// </summary>
        long WasteBits { get; }

        /// <summary>
        /// Gets the number of pages that have been read in the container.
        /// </summary>
        int PagesRead { get; }

        /// <summary>
        /// Event raised when a new logical stream is found in the container.
        /// </summary>
        event EventHandler<NewStreamEventArgs> NewStream;

        /// <summary>
        /// Initializes the container and finds the first stream.
        /// </summary>
        /// <returns><c>True</c> if a valid logical stream is found, otherwise <c>False</c>.</returns>
        bool Init();

        /// <summary>
        /// Finds the next new stream in the container.
        /// </summary>
        /// <returns><c>True</c> if a new stream was found, otherwise <c>False</c>.</returns>
        /// <exception cref="InvalidOperationException"><see cref="CanSeek"/> is <c>False</c>.</exception>
        bool FindNextStream();

        /// <summary>
        /// Retrieves the total number of pages in the container.
        /// </summary>
        /// <returns>The total number of pages.</returns>
        /// <exception cref="InvalidOperationException"><see cref="CanSeek"/> is <c>False</c>.</exception>
        int GetTotalPageCount();
    }
}
