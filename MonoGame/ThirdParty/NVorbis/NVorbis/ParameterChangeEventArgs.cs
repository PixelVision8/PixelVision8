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
    /// Event data for when a logical stream has a parameter change.
    /// </summary>
    [Serializable]
    public class ParameterChangeEventArgs : EventArgs
    {
        /// <summary>
        /// Creates a new instance of <see cref="ParameterChangeEventArgs"/>.
        /// </summary>
        /// <param name="firstPacket">The first packet after the parameter change.</param>
        public ParameterChangeEventArgs(DataPacket firstPacket)
        {
            FirstPacket = firstPacket;
        }

        /// <summary>
        /// Gets the first packet after the parameter change.  This would typically be the parameters packet.
        /// </summary>
        public DataPacket FirstPacket { get; private set; }
    }
}
