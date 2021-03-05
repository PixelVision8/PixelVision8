// MIT License - Copyright (C) The Mono.Xna Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Runtime.Serialization;
using System.Diagnostics;

namespace Microsoft.Xna.Framework
{
    /// <summary>
    /// Describes a 4D-vector.
    /// </summary>
// #if XNADESIGNPROVIDED
//     [System.ComponentModel.TypeConverter(typeof(Microsoft.Xna.Framework.Design.Vector4TypeConverter))]
// #endif
    [DataContract]
    [DebuggerDisplay("{DebugDisplayString,nq}")]
    public struct Vector4 : IEquatable<Vector4>
    {
        #region Private Fields

        private static readonly Vector4 zero = new Vector4();
        private static readonly Vector4 one = new Vector4(1f, 1f, 1f, 1f);
        private static readonly Vector4 unitX = new Vector4(1f, 0f, 0f, 0f);
        private static readonly Vector4 unitY = new Vector4(0f, 1f, 0f, 0f);
        private static readonly Vector4 unitZ = new Vector4(0f, 0f, 1f, 0f);
        private static readonly Vector4 unitW = new Vector4(0f, 0f, 0f, 1f);

        #endregion

        #region Public Fields

        /// <summary>
        /// The x coordinate of this <see cref="Vector4"/>.
        /// </summary>
        [DataMember]
        public float X;

        /// <summary>
        /// The y coordinate of this <see cref="Vector4"/>.
        /// </summary>
        [DataMember]
        public float Y;

        /// <summary>
        /// The z coordinate of this <see cref="Vector4"/>.
        /// </summary>
        [DataMember]
        public float Z;

        /// <summary>
        /// The w coordinate of this <see cref="Vector4"/>.
        /// </summary>
        [DataMember]
        public float W;

        #endregion

        #region Public Properties

        /// <summary>
        /// Returns a <see cref="Vector4"/> with components 0, 0, 0, 0.
        /// </summary>
        public static Vector4 Zero
        {
            get { return zero; }
        }

        /// <summary>
        /// Returns a <see cref="Vector4"/> with components 1, 1, 1, 1.
        /// </summary>
        public static Vector4 One
        {
            get { return one; }
        }

        /// <summary>
        /// Returns a <see cref="Vector4"/> with components 1, 0, 0, 0.
        /// </summary>
        public static Vector4 UnitX
        {
            get { return unitX; }
        }

        /// <summary>
        /// Returns a <see cref="Vector4"/> with components 0, 1, 0, 0.
        /// </summary>
        public static Vector4 UnitY
        {
            get { return unitY; }
        }

        /// <summary>
        /// Returns a <see cref="Vector4"/> with components 0, 0, 1, 0.
        /// </summary>
        public static Vector4 UnitZ
        {
            get { return unitZ; }
        }

        /// <summary>
        /// Returns a <see cref="Vector4"/> with components 0, 0, 0, 1.
        /// </summary>
        public static Vector4 UnitW
        {
            get { return unitW; }
        }

        #endregion

        #region Internal Properties

        internal string DebugDisplayString
        {
            get
            {
                return string.Concat(
                    this.X.ToString(), "  ",
                    this.Y.ToString(), "  ",
                    this.Z.ToString(), "  ",
                    this.W.ToString()
                );
            }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Constructs a 3d vector with X, Y, Z and W from four values.
        /// </summary>
        /// <param name="x">The x coordinate in 4d-space.</param>
        /// <param name="y">The y coordinate in 4d-space.</param>
        /// <param name="z">The z coordinate in 4d-space.</param>
        /// <param name="w">The w coordinate in 4d-space.</param>
        public Vector4(float x, float y, float z, float w)
        {
            this.X = x;
            this.Y = y;
            this.Z = z;
            this.W = w;
        }

        #endregion

        /// <summary>
        /// Compares whether current instance is equal to specified <see cref="Vector4"/>.
        /// </summary>
        /// <param name="other">The <see cref="Vector4"/> to compare.</param>
        /// <returns><c>true</c> if the instances are equal; <c>false</c> otherwise.</returns>
        public bool Equals(Vector4 other)
        {
            return this.W == other.W
                && this.X == other.X
                && this.Y == other.Y
                && this.Z == other.Z;
        }

        /// <summary>
        /// Gets the hash code of this <see cref="Vector4"/>.
        /// </summary>
        /// <returns>Hash code of this <see cref="Vector4"/>.</returns>
        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = W.GetHashCode();
                hashCode = (hashCode * 397) ^ X.GetHashCode();
                hashCode = (hashCode * 397) ^ Y.GetHashCode();
                hashCode = (hashCode * 397) ^ Z.GetHashCode();
                return hashCode;
            }
        }

        /// <summary>
        /// Returns a <see cref="String"/> representation of this <see cref="Vector4"/> in the format:
        /// {X:[<see cref="X"/>] Y:[<see cref="Y"/>] Z:[<see cref="Z"/>] W:[<see cref="W"/>]}
        /// </summary>
        /// <returns>A <see cref="String"/> representation of this <see cref="Vector4"/>.</returns>
        public override string ToString()
        {
            return "{X:" + X + " Y:" + Y + " Z:" + Z + " W:" + W + "}";
        }

        #region Operators

        /// <summary>
        /// Inverts values in the specified <see cref="Vector4"/>.
        /// </summary>
        /// <param name="value">Source <see cref="Vector4"/> on the right of the sub sign.</param>
        /// <returns>Result of the inversion.</returns>
        public static Vector4 operator -(Vector4 value)
        {
            return new Vector4(-value.X, -value.Y, -value.Z, -value.W);
        }

        /// <summary>
        /// Compares whether two <see cref="Vector4"/> instances are equal.
        /// </summary>
        /// <param name="value1"><see cref="Vector4"/> instance on the left of the equal sign.</param>
        /// <param name="value2"><see cref="Vector4"/> instance on the right of the equal sign.</param>
        /// <returns><c>true</c> if the instances are equal; <c>false</c> otherwise.</returns>
        public static bool operator ==(Vector4 value1, Vector4 value2)
        {
            return value1.W == value2.W
                && value1.X == value2.X
                && value1.Y == value2.Y
                && value1.Z == value2.Z;
        }

        /// <summary>
        /// Compares whether two <see cref="Vector4"/> instances are not equal.
        /// </summary>
        /// <param name="value1"><see cref="Vector4"/> instance on the left of the not equal sign.</param>
        /// <param name="value2"><see cref="Vector4"/> instance on the right of the not equal sign.</param>
        /// <returns><c>true</c> if the instances are not equal; <c>false</c> otherwise.</returns>	
        public static bool operator !=(Vector4 value1, Vector4 value2)
        {
            return !(value1 == value2);
        }

        /// <summary>
        /// Adds two vectors.
        /// </summary>
        /// <param name="value1">Source <see cref="Vector4"/> on the left of the add sign.</param>
        /// <param name="value2">Source <see cref="Vector4"/> on the right of the add sign.</param>
        /// <returns>Sum of the vectors.</returns>
        public static Vector4 operator +(Vector4 value1, Vector4 value2)
        {
            value1.W += value2.W;
            value1.X += value2.X;
            value1.Y += value2.Y;
            value1.Z += value2.Z;
            return value1;
        }

        /// <summary>
        /// Subtracts a <see cref="Vector4"/> from a <see cref="Vector4"/>.
        /// </summary>
        /// <param name="value1">Source <see cref="Vector4"/> on the left of the sub sign.</param>
        /// <param name="value2">Source <see cref="Vector4"/> on the right of the sub sign.</param>
        /// <returns>Result of the vector subtraction.</returns>
        public static Vector4 operator -(Vector4 value1, Vector4 value2)
        {
            value1.W -= value2.W;
            value1.X -= value2.X;
            value1.Y -= value2.Y;
            value1.Z -= value2.Z;
            return value1;
        }

        /// <summary>
        /// Multiplies the components of two vectors by each other.
        /// </summary>
        /// <param name="value1">Source <see cref="Vector4"/> on the left of the mul sign.</param>
        /// <param name="value2">Source <see cref="Vector4"/> on the right of the mul sign.</param>
        /// <returns>Result of the vector multiplication.</returns>
        public static Vector4 operator *(Vector4 value1, Vector4 value2)
        {
            value1.W *= value2.W;
            value1.X *= value2.X;
            value1.Y *= value2.Y;
            value1.Z *= value2.Z;
            return value1;
        }

        /// <summary>
        /// Multiplies the components of vector by a scalar.
        /// </summary>
        /// <param name="value">Source <see cref="Vector4"/> on the left of the mul sign.</param>
        /// <param name="scaleFactor">Scalar value on the right of the mul sign.</param>
        /// <returns>Result of the vector multiplication with a scalar.</returns>
        public static Vector4 operator *(Vector4 value, float scaleFactor)
        {
            value.W *= scaleFactor;
            value.X *= scaleFactor;
            value.Y *= scaleFactor;
            value.Z *= scaleFactor;
            return value;
        }

        /// <summary>
        /// Multiplies the components of vector by a scalar.
        /// </summary>
        /// <param name="scaleFactor">Scalar value on the left of the mul sign.</param>
        /// <param name="value">Source <see cref="Vector4"/> on the right of the mul sign.</param>
        /// <returns>Result of the vector multiplication with a scalar.</returns>
        public static Vector4 operator *(float scaleFactor, Vector4 value)
        {
            value.W *= scaleFactor;
            value.X *= scaleFactor;
            value.Y *= scaleFactor;
            value.Z *= scaleFactor;
            return value;
        }

        /// <summary>
        /// Divides the components of a <see cref="Vector4"/> by the components of another <see cref="Vector4"/>.
        /// </summary>
        /// <param name="value1">Source <see cref="Vector4"/> on the left of the div sign.</param>
        /// <param name="value2">Divisor <see cref="Vector4"/> on the right of the div sign.</param>
        /// <returns>The result of dividing the vectors.</returns>
        public static Vector4 operator /(Vector4 value1, Vector4 value2)
        {
            value1.W /= value2.W;
            value1.X /= value2.X;
            value1.Y /= value2.Y;
            value1.Z /= value2.Z;
            return value1;
        }

        /// <summary>
        /// Divides the components of a <see cref="Vector4"/> by a scalar.
        /// </summary>
        /// <param name="value1">Source <see cref="Vector4"/> on the left of the div sign.</param>
        /// <param name="divider">Divisor scalar on the right of the div sign.</param>
        /// <returns>The result of dividing a vector by a scalar.</returns>
        public static Vector4 operator /(Vector4 value1, float divider)
        {
            float factor = 1f / divider;
            value1.W *= factor;
            value1.X *= factor;
            value1.Y *= factor;
            value1.Z *= factor;
            return value1;
        }

        #endregion
    }
}
