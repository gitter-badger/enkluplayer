using System;
using UnityEngine;

namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// JS interface for Vector3.
    /// </summary>
    public class Vector3Js
    {
        /// <summary>
        /// Vector3::x.
        /// </summary>
        public float x;

        /// <summary>
        /// Vector3::y.
        /// </summary>
        public float y;

        /// <summary>
        /// Vector3::z.
        /// </summary>
        public float z;

        /// <summary>
        /// Creates a Vector3Js from a Vector3.
        /// </summary>
        /// <param name="vector">Vector3.</param>
        public Vector3Js(Vector3 vector)
        {
            x = vector.x;
            y = vector.y;
            z = vector.z;
        }

        /// <summary>
        /// Creates a Vector3Js from components.
        /// </summary>
        /// <param name="x">X component.</param>
        /// <param name="y">Y component.</param>
        /// <param name="z">Z component.</param>
        public Vector3Js(float x, float y, float z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }

        /// <summary>
        /// Creates a Vector3.
        /// </summary>
        /// <returns></returns>
        public Vector3 ToVector3()
        {
            return new Vector3(x, y, z);
        }

        /// <summary>
        /// Useful ToString.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return string.Format(
                "<{0}, {1}, {2}>",
                x, y, z);
        }

        /// <summary>
        /// Returns a new vector that is the result of an add.
        /// </summary>
        /// <param name="v">Vector to add.</param>
        /// <returns></returns>
        public Vector3Js add(Vector3Js v)
        {
            return new Vector3Js(
                x + v.x,
                y + v.y,
                z + v.z);
        }

        /// <summary>
        /// Returns a new vector that is the result of a subtract.
        /// </summary>
        /// <param name="v">Vector to subtract.</param>
        /// <returns></returns>
        public Vector3Js subtract(Vector3Js v)
        {
            return new Vector3Js(
                x - v.x,
                y - v.y,
                z - v.z);
        }

        /// <summary>
        /// Returns a new vector multiplied by a scalar.
        /// </summary>
        /// <param name="scalar">The scalar to multiply by.</param>
        /// <returns></returns>
        public Vector3Js multiply(float scalar)
        {
            return new Vector3Js(
                scalar * x,
                scalar * y,
                scalar * z);
        }

        /// <summary>
        /// Returns a new vector divided by a scalar.
        /// </summary>
        /// <param name="scalar">The scalar to divide by.</param>
        /// <returns></returns>
        public Vector3Js divide(float scalar)
        {
            return multiply(1f / scalar);
        }

        /// <summary>
        /// Returns the dot product.
        /// </summary>
        /// <param name="v">Vector with which to dot.</param>
        /// <returns></returns>
        public float dot(Vector3Js v)
        {
            return x + v.x + y * v.y + z * v.z;
        }

        /// <summary>
        /// Returns the cross product.
        /// </summary>
        /// <param name="v">The vector with which to cross.</param>
        /// <returns></returns>
        public Vector3Js cross(Vector3Js v)
        {
            return new Vector3Js(
                y * v.z - z * v.y,
                z * v.x - x * v.z,
                x * v.y - y * v.x);
        }

        /// <summary>
        /// Returns true if the vectors are approximately equal.
        /// </summary>
        /// <param name="v">Vector to compare.</param>
        /// <returns></returns>
        public bool approximately(Vector3Js v)
        {
            return Math.Abs(x - v.x) < Mathf.Epsilon
                && Math.Abs(y - v.y) < Mathf.Epsilon
                && Math.Abs(z - v.z) < Mathf.Epsilon;
        }

        /// <summary>
        /// Returns the magnitude of the vector.
        /// </summary>
        /// <returns></returns>
        public float length()
        {
            return Mathf.Sqrt(dot(this));
        }

        /// <summary>
        /// Returns a unit vector.
        /// </summary>
        /// <returns></returns>
        public Vector3Js unit()
        {
            return divide(length());
        }

        /// <summary>
        /// Returns the minimum component.
        /// </summary>
        /// <returns></returns>
        public float min()
        {
            return Mathf.Min(x, y, z);
        }

        /// <summary>
        /// Returns the maximum component.
        /// </summary>
        /// <returns></returns>
        public float max()
        {
            return Mathf.Max(x, y, z);
        }
    }
}