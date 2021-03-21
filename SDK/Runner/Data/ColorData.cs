// MIT License - Copyright (C) The Mono.Xna Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Text;

// using System.Runtime.Serialization;
// using System.Diagnostics;

namespace PixelVision8.Runner
{
    /// <summary>
    /// Describes a 32-bit packed color.
    /// </summary>
    // [DataContract]
    // [DebuggerDisplay("{DebugDisplayString,nq}")]
    public struct ColorData
    {
        
        // Stored as RGBA with R in the least significant octet:
        // |-------|-------|-------|-------
        // A       B       G       R
        private readonly uint _packedValue;
	  
        /// <summary>
        /// Constructs an RGBA color from scalars representing red, green, blue and alpha values.
        /// </summary>
        /// <remarks>
        /// This overload sets the values directly without clamping, and may therefore be faster than the other overloads.
        /// </remarks>
        /// <param name="r"></param>
        /// <param name="g"></param>
        /// <param name="b"></param>
        /// <param name="alpha"></param>
        public ColorData(byte r, byte g, byte b, byte alpha = Byte.MaxValue)
        {
            _packedValue = ((uint)alpha << 24) | ((uint)b << 16) | ((uint)g << 8) | (r);
        }

        public ColorData(string hex)
        {
            DisplayTarget.HexToRgb(hex, out var r, out var g, out var b);

            _packedValue = ((uint)Byte.MaxValue << 24) | ((uint)b << 16) | ((uint)g << 8) | (r);
            
        }

        /// <summary>
        /// Gets or sets the blue component.
        /// </summary>
        // [DataMember]
        public byte B
        {
            get
            {
                unchecked
                {
                    return (byte) (_packedValue >> 16);
                }
            }
        }

        /// <summary>
        /// Gets or sets the green component.
        /// </summary>
        // [DataMember]
        public byte G
        {
            get
            {
                unchecked
                {
                    return (byte)(_packedValue >> 8);
                }
            }
        }

        /// <summary>
        /// Gets or sets the red component.
        /// </summary>
        // [DataMember]
        public byte R
        {
            get
            {
                unchecked
                {
                    return (byte) _packedValue;
                }
            }
        }

        /// <summary>
        /// Gets or sets the alpha component.
        /// </summary>
        // [DataMember]
        public byte A
        {
            get
            {
                unchecked
                {
                    return (byte)(_packedValue >> 24);
                }
            }
        }

        /// <summary>
        /// Returns a <see cref="String"/> representation of this <see cref="ColorData"/> in the format:
        /// {R:[red] G:[green] B:[blue] A:[alpha]}
        /// </summary>
        /// <returns><see cref="String"/> representation of this <see cref="ColorData"/>.</returns>
	    public override string ToString ()
	    {
            StringBuilder sb = new StringBuilder(25);
            sb.Append("{R:");
            sb.Append(R);
            sb.Append(" G:");
            sb.Append(G);
            sb.Append(" B:");
            sb.Append(B);
            sb.Append(" A:");
            sb.Append(A);
            sb.Append("}");
            return sb.ToString();
	    }
	
    }
}
