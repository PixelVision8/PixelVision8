// MIT License - Copyright (C) The Mono.Xna Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Diagnostics;
using System.Text;
using System.Runtime.Serialization;

namespace Microsoft.Xna.Framework
{
    /// <summary>
    /// Describes a 3D-vector.
    /// </summary>
    [DataContract]
    [DebuggerDisplay("{DebugDisplayString,nq}")]
    public struct Vector3 : IEquatable<Vector3>
    {

        #region Public Fields

        /// <summary>
        /// The x coordinate of this <see cref="Vector3"/>.
        /// </summary>
        [DataMember]
        public float X;

        /// <summary>
        /// The y coordinate of this <see cref="Vector3"/>.
        /// </summary>
        [DataMember]
        public float Y;

        /// <summary>
        /// The z coordinate of this <see cref="Vector3"/>.
        /// </summary>
        [DataMember]
        public float Z;

        #endregion

        #region Constructors

        /// <summary>
        /// Constructs a 3d vector with X, Y and Z from three values.
        /// </summary>
        /// <param name="x">The x coordinate in 3d-space.</param>
        /// <param name="y">The y coordinate in 3d-space.</param>
        /// <param name="z">The z coordinate in 3d-space.</param>
        public Vector3(float x, float y, float z)
        {
            this.X = x;
            this.Y = y;
            this.Z = z;
        }

        #endregion
        
        #region Public Methods

        /// <summary>
        /// Compares whether current instance is equal to specified <see cref="Object"/>.
        /// </summary>
        /// <param name="obj">The <see cref="Object"/> to compare.</param>
        /// <returns><c>true</c> if the instances are equal; <c>false</c> otherwise.</returns>
        public override bool Equals(object obj)
        {
            if (!(obj is Vector3))
                return false;

            var other = (Vector3)obj;
            return  X == other.X &&
                    Y == other.Y &&
                    Z == other.Z;
        }

        /// <summary>
        /// Compares whether current instance is equal to specified <see cref="Vector3"/>.
        /// </summary>
        /// <param name="other">The <see cref="Vector3"/> to compare.</param>
        /// <returns><c>true</c> if the instances are equal; <c>false</c> otherwise.</returns>
        public bool Equals(Vector3 other)
        {
            return  X == other.X && 
                    Y == other.Y &&
                    Z == other.Z;
        }

        /// <summary>
        /// Gets the hash code of this <see cref="Vector3"/>.
        /// </summary>
        /// <returns>Hash code of this <see cref="Vector3"/>.</returns>
        public override int GetHashCode() {
            unchecked
            {
                var hashCode = X.GetHashCode();
                hashCode = (hashCode * 397) ^ Y.GetHashCode();
                hashCode = (hashCode * 397) ^ Z.GetHashCode();
                return hashCode;
            }
        }

        /// <summary>
        /// Returns a <see cref="String"/> representation of this <see cref="Vector3"/> in the format:
        /// {X:[<see cref="X"/>] Y:[<see cref="Y"/>] Z:[<see cref="Z"/>]}
        /// </summary>
        /// <returns>A <see cref="String"/> representation of this <see cref="Vector3"/>.</returns>
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder(32);
            sb.Append("{X:");
            sb.Append(this.X);
            sb.Append(" Y:");
            sb.Append(this.Y);
            sb.Append(" Z:");
            sb.Append(this.Z);
            sb.Append("}");
            return sb.ToString();
        }

        #endregion

        #region Operators

        /// <summary>
        /// Compares whether two <see cref="Vector3"/> instances are equal.
        /// </summary>
        /// <param name="value1"><see cref="Vector3"/> instance on the left of the equal sign.</param>
        /// <param name="value2"><see cref="Vector3"/> instance on the right of the equal sign.</param>
        /// <returns><c>true</c> if the instances are equal; <c>false</c> otherwise.</returns>
        public static bool operator ==(Vector3 value1, Vector3 value2)
        {
            return value1.X == value2.X
                && value1.Y == value2.Y
                && value1.Z == value2.Z;
        }

        /// <summary>
        /// Compares whether two <see cref="Vector3"/> instances are not equal.
        /// </summary>
        /// <param name="value1"><see cref="Vector3"/> instance on the left of the not equal sign.</param>
        /// <param name="value2"><see cref="Vector3"/> instance on the right of the not equal sign.</param>
        /// <returns><c>true</c> if the instances are not equal; <c>false</c> otherwise.</returns>	
        public static bool operator !=(Vector3 value1, Vector3 value2)
        {
            return !(value1 == value2);
        }

        /// <summary>
        /// Inverts values in the specified <see cref="Vector3"/>.
        /// </summary>
        /// <param name="value">Source <see cref="Vector3"/> on the right of the sub sign.</param>
        /// <returns>Result of the inversion.</returns>
        public static Vector3 operator -(Vector3 value)
        {
            value = new Vector3(-value.X, -value.Y, -value.Z);
            return value;
        }

        /// <summary>
        /// Subtracts a <see cref="Vector3"/> from a <see cref="Vector3"/>.
        /// </summary>
        /// <param name="value1">Source <see cref="Vector3"/> on the left of the sub sign.</param>
        /// <param name="value2">Source <see cref="Vector3"/> on the right of the sub sign.</param>
        /// <returns>Result of the vector subtraction.</returns>
        public static Vector3 operator -(Vector3 value1, Vector3 value2)
        {
            value1.X -= value2.X;
            value1.Y -= value2.Y;
            value1.Z -= value2.Z;
            return value1;
        }

        #endregion
    }
}
