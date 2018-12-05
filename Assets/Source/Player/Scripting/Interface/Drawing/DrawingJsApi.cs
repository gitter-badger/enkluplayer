using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace CreateAR.EnkluPlayer
{
    public class DrawingJsApi
    {
        private readonly Stack<Matrix4x4> _transformStack = new Stack<Matrix4x4>();
        private Matrix4x4 _current;

        public DrawingJsApi()
        {
            _current = Matrix4x4.identity;
        }

        public void translate(Vec3 to)
        {
            translate(to.x, to.y, to.z);
        }

        public void translate(float x, float y, float z)
        {
            _current = _current * Matrix4x4.Translate(new Vector3(x, y, z));
        }

        public void rotateX(float rad)
        {
            _current = _current * Matrix4x4.Rotate(Quaternion.AngleAxis(
               Mathf.Rad2Deg * rad,
               new Vector3(1, 0, 0)));
        }

        public void rotateY(float rad)
        {
            _current = _current * Matrix4x4.Rotate(Quaternion.AngleAxis(
               Mathf.Rad2Deg * rad,
               new Vector3(0, 1, 0)));
        }

        public void rotateZ(float rad)
        {
            _current = _current * Matrix4x4.Rotate(Quaternion.AngleAxis(
               Mathf.Rad2Deg * rad,
               new Vector3(0, 0, 1)));
        }

        public void scale(Vec3 to)
        {
            scale(to.x, to.y, to.z);
        }

        public void scale(float x, float y, float z)
        {
            _current = _current * Matrix4x4.Scale(new Vector3(x, y, z));
        }

        public void pushMatrix()
        {
            _transformStack.Push(_current);

            _current = Matrix4x4.identity;
        }

        public void popMatrix()
        {
            _current = _transformStack.Pop();
        }

        public void resetMatrix()
        {
            _current = Matrix4x4.identity;
        }
    }
}
