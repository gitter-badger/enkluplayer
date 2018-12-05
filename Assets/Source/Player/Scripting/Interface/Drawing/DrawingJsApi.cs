using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CreateAR.EnkluPlayer.Scripting;
using Jint;
using Jint.Native;
using UnityEngine;

using JsFunc = System.Func<Jint.Native.JsValue, Jint.Native.JsValue[], Jint.Native.JsValue>;

namespace CreateAR.EnkluPlayer
{
    public class DrawingJsApi
    {
        private readonly List<JsFunc> _cbs = new List<JsFunc>();

        private readonly ContextJsApi _context = new ContextJsApi();

        public void render(Engine engine, JsFunc fn)
        {

            fn(
                JsValue.FromObject(engine, this),
                new[] {JsValue.FromObject(engine, _context)});
        }
    }

    public class ContextJsApi
    {
        private Color _color;

        public ContextJsApi()
        {
            ResetState();
        }

        [DenyJsAccess]
        public void ResetState()
        {
            _color = Color.white;
        }
        
        public ContextJsApi color(Col4 col)
        {
            color(col.r, col.g, col.b, col.a);

            return this;
        }

        public ContextJsApi color(float r, float g, float b, float a)
        {
            _color = new Color(r, g, b, a);

            return this;
        }

        public ContextJsApi alpha(float a)
        {
            color(_color.r, _color.g, _color.b, a);

            return this;
        }

        public ContextJsApi red(float r)
        {
            color(r, _color.g, _color.b, _color.a);

            return this;
        }

        public ContextJsApi green(float g)
        {
            color(_color.r, g, _color.b, _color.a);

            return this;
        }

        public ContextJsApi blue(float b)
        {
            color(_color.r, _color.g, b, _color.a);

            return this;
        }

        public ContextJsApi line(Vec3 from, Vec3 to)
        {
            line(from.x, from.y, from.z, to.x, to.y, to.z);

            return this;
        }

        public ContextJsApi line(
            float xf, float yf, float zf,
            float xt, float yt, float zt)
        {
            GL.Begin(GL.LINES);
            GL.LoadProjectionMatrix(Camera.main.projectionMatrix);
            GL.Color(_color);
            {
                GL.Vertex(new Vector3(xf, yf, zf));
                GL.Vertex(new Vector3(xt, yt, zt));
            }
            GL.End();

            return this;
        }

        public ContextJsApi lines(Vec3[] points)
        {
            GL.Begin(GL.LINES);
            GL.LoadProjectionMatrix(Camera.main.projectionMatrix);
            GL.Color(_color);
            {
                for (int i = 0, len = points.Length; i < len; i += 2)
                {
                    GL.Vertex(points[i].ToVector());
                    GL.Vertex(points[i + 1].ToVector());
                }
            }
            GL.End();

            return this;
        }

        public ContextJsApi linestrip(Vec3[] points)
        {
            GL.Begin(GL.LINE_STRIP);
            GL.LoadProjectionMatrix(Camera.main.projectionMatrix);
            GL.Color(_color);
            {
                for (int i = 0, len = points.Length; i < len; i++)
                {
                    GL.Vertex(points[i].ToVector());
                }
            }
            GL.End();

            return this;
        }

        public ContextJsApi box(float size)
        {
            box(size, size, size);

            return this;
        }

        public ContextJsApi box(float width, float height, float depth)
        {
            return this;
        }
    }
}
