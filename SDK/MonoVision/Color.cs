// MIT License - Copyright (C) The Mono.Xna Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Text;
using System.Runtime.Serialization;
using System.Diagnostics;

namespace Microsoft.Xna.Framework
{
    /// <summary>
    /// Describes a 32-bit packed color.
    /// </summary>
    [DataContract]
    [DebuggerDisplay("{DebugDisplayString,nq}")]
    public struct Color : IEquatable<Color>
    {
        static Color()
        {
            Black = new Color(0xff000000);
            Magenta = new Color(0xffff00ff);
            White= new Color(uint.MaxValue);
        }

        // Stored as RGBA with R in the least significant octet:
        // |-------|-------|-------|-------
        // A       B       G       R
        private uint _packedValue;
	  
        /// <summary>
        /// Constructs an RGBA color from a packed value.
        /// The value is a 32-bit unsigned integer, with R in the least significant octet.
        /// </summary>
        /// <param name="packedValue">The packed value.</param>
        [CLSCompliant(false)]
        public Color(uint packedValue)
        {
            _packedValue = packedValue;
        }

        /// <summary>
        /// Constructs an RGBA color from scalars representing red, green and blue values. Alpha value will be opaque.
        /// </summary>
        /// <param name="r">Red component value from 0.0f to 1.0f.</param>
        /// <param name="g">Green component value from 0.0f to 1.0f.</param>
        /// <param name="b">Blue component value from 0.0f to 1.0f.</param>
        public Color(float r, float g, float b)
            : this((int)(r * 255), (int)(g * 255), (int)(b * 255))
        {
        }

        /// <summary>
        /// Constructs an RGBA color from scalars representing red, green and blue values. Alpha value will be opaque.
        /// </summary>
        /// <param name="r">Red component value from 0 to 255.</param>
        /// <param name="g">Green component value from 0 to 255.</param>
        /// <param name="b">Blue component value from 0 to 255.</param>
        public Color(int r, int g, int b)
        {
            _packedValue = 0xFF000000; // A = 255

            if (((r | g | b) & 0xFFFFFF00) != 0)
            {
                var clampedR = (uint)MathHelper.Clamp(r, Byte.MinValue, Byte.MaxValue);
                var clampedG = (uint)MathHelper.Clamp(g, Byte.MinValue, Byte.MaxValue);
                var clampedB = (uint)MathHelper.Clamp(b, Byte.MinValue, Byte.MaxValue);

                _packedValue |= (clampedB << 16) | (clampedG << 8) | (clampedR);
            }
            else
            {
                _packedValue |= ((uint)b << 16) | ((uint)g << 8) | ((uint)r);
            }
        }

        /// <summary>
        /// Constructs an RGBA color from scalars representing red, green, blue and alpha values.
        /// </summary>
        /// <param name="r">Red component value from 0 to 255.</param>
        /// <param name="g">Green component value from 0 to 255.</param>
        /// <param name="b">Blue component value from 0 to 255.</param>
        /// <param name="alpha">Alpha component value from 0 to 255.</param>
        public Color(int r, int g, int b, int alpha)
        {
            if (((r | g | b | alpha) & 0xFFFFFF00) != 0)
            {
                var clampedR = (uint)MathHelper.Clamp(r, Byte.MinValue, Byte.MaxValue);
                var clampedG = (uint)MathHelper.Clamp(g, Byte.MinValue, Byte.MaxValue);
                var clampedB = (uint)MathHelper.Clamp(b, Byte.MinValue, Byte.MaxValue);
                var clampedA = (uint)MathHelper.Clamp(alpha, Byte.MinValue, Byte.MaxValue);

                _packedValue = (clampedA << 24) | (clampedB << 16) | (clampedG << 8) | (clampedR);
            }
            else
            {
                _packedValue = ((uint)alpha << 24) | ((uint)b << 16) | ((uint)g << 8) | ((uint)r);
            }
        }

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
        public Color(byte r, byte g, byte b, byte alpha)
        {
            _packedValue = ((uint)alpha << 24) | ((uint)b << 16) | ((uint)g << 8) | (r);
        }

        /// <summary>
        /// Gets or sets the blue component.
        /// </summary>
        [DataMember]
        public byte B
        {
            get
            {
                unchecked
                {
                    return (byte) (this._packedValue >> 16);
                }
            }
            set
            {
                this._packedValue = (this._packedValue & 0xff00ffff) | ((uint)value << 16);
            }
        }

        /// <summary>
        /// Gets or sets the green component.
        /// </summary>
        [DataMember]
        public byte G
        {
            get
            {
                unchecked
                {
                    return (byte)(this._packedValue >> 8);
                }
            }
            set
            {
                this._packedValue = (this._packedValue & 0xffff00ff) | ((uint)value << 8);
            }
        }

        /// <summary>
        /// Gets or sets the red component.
        /// </summary>
        [DataMember]
        public byte R
        {
            get
            {
                unchecked
                {
                    return (byte) this._packedValue;
                }
            }
            set
            {
                this._packedValue = (this._packedValue & 0xffffff00) | value;
            }
        }

        /// <summary>
        /// Gets or sets the alpha component.
        /// </summary>
        [DataMember]
        public byte A
        {
            get
            {
                unchecked
                {
                    return (byte)(this._packedValue >> 24);
                }
            }
            set
            {
                this._packedValue = (this._packedValue & 0x00ffffff) | ((uint)value << 24);
            }
        }
		
	/// <summary>
        /// Compares whether two <see cref="Color"/> instances are equal.
        /// </summary>
        /// <param name="a"><see cref="Color"/> instance on the left of the equal sign.</param>
        /// <param name="b"><see cref="Color"/> instance on the right of the equal sign.</param>
        /// <returns><c>true</c> if the instances are equal; <c>false</c> otherwise.</returns>
        public static bool operator ==(Color a, Color b)
        {
            return (a._packedValue == b._packedValue);
        }
	
	    /// <summary>
        /// Compares whether two <see cref="Color"/> instances are not equal.
        /// </summary>
        /// <param name="a"><see cref="Color"/> instance on the left of the not equal sign.</param>
        /// <param name="b"><see cref="Color"/> instance on the right of the not equal sign.</param>
        /// <returns><c>true</c> if the instances are not equal; <c>false</c> otherwise.</returns>	
        public static bool operator !=(Color a, Color b)
        {
            return (a._packedValue != b._packedValue);
        }

        /// <summary>
        /// Gets the hash code of this <see cref="Color"/>.
        /// </summary>
        /// <returns>Hash code of this <see cref="Color"/>.</returns>
        public override int GetHashCode()
        {
            return this._packedValue.GetHashCode();
        }
	
        /// <summary>
        /// Compares whether current instance is equal to specified object.
        /// </summary>
        /// <param name="obj">The <see cref="Color"/> to compare.</param>
        /// <returns><c>true</c> if the instances are equal; <c>false</c> otherwise.</returns>
        public override bool Equals(object obj)
        {
            return ((obj is Color) && this.Equals((Color)obj));
        }

        #region Color Bank
        
        /// <summary>
        /// Black color (R:0,G:0,B:0,A:255).
        /// </summary>
        public static Color Black
        {
            get;
            private set;
        }
        
        /// <summary>
        /// Magenta color (R:255,G:0,B:255,A:255).
        /// </summary>
        public static Color Magenta
        {
            get;
            private set;
        }

        /// <summary>
        /// White color (R:255,G:255,B:255,A:255).
        /// </summary>
        public static Color White
        {
            get;
            private set;
        }
       
        #endregion

        /// <summary>
        /// Gets or sets packed value of this <see cref="Color"/>.
        /// </summary>
        [CLSCompliant(false)]
        public UInt32 PackedValue
        {
            get { return _packedValue; }
            set { _packedValue = value; }
        }


        internal string DebugDisplayString
        {
            get
            {
                return string.Concat(
                    this.R.ToString(), "  ",
                    this.G.ToString(), "  ",
                    this.B.ToString(), "  ",
                    this.A.ToString()
                );
            }
        }


        /// <summary>
        /// Returns a <see cref="String"/> representation of this <see cref="Color"/> in the format:
        /// {R:[red] G:[green] B:[blue] A:[alpha]}
        /// </summary>
        /// <returns><see cref="String"/> representation of this <see cref="Color"/>.</returns>
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
	
        #region IEquatable<Color> Members
	
	/// <summary>
        /// Compares whether current instance is equal to specified <see cref="Color"/>.
        /// </summary>
        /// <param name="other">The <see cref="Color"/> to compare.</param>
        /// <returns><c>true</c> if the instances are equal; <c>false</c> otherwise.</returns>
        public bool Equals(Color other)
        {
	    return this.PackedValue == other.PackedValue;
        }

        #endregion

    }
}
