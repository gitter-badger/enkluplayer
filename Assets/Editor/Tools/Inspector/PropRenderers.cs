using System;
using CreateAR.Commons.Unity.Editor;
using CreateAR.EnkluPlayer.IUX;
using UnityEditor;
using UnityEngine;

namespace CreateAR.EnkluPlayer.Editor
{
    /// <summary>
    /// Attribute for linking a prop renderer to a type.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class PropRendererAttribute : Attribute
    {
        /// <summary>
        /// The type to link to.
        /// </summary>
        public readonly Type Type;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="type">The associated type.</param>
        public PropRendererAttribute(Type type)
        {
            Type = type;
        }
    }

    /// <summary>
    /// Renders controls for a bool prop.
    /// </summary>
    [PropRenderer(typeof(bool))]
    public class BoolPropRenderer : PropRenderer
    {
        /// <summary>
        /// Underyling renderer.
        /// </summary>
        private readonly BoolControlRenderer _renderer = new BoolControlRenderer();

        /// <summary>
        /// Parameters for rendering.
        /// </summary>
        private ControlRendererParameter[] _parameters = new ControlRendererParameter[0];

        /// <inheritdoc cref="PropRenderer"/>
        public override bool Draw(ElementSchemaProp prop)
        {
            var cast = (ElementSchemaProp<bool>) prop;
            var value = (object) cast.Value;
            var repaint = _renderer.Draw(prop.Name, ref value, ref _parameters);

            if ((bool) value != cast.Value)
            {
                cast.Value = (bool) value;
            }

            return repaint;
        }
    }

    /// <summary>
    /// Renders controls for an int prop.
    /// </summary>
    [PropRenderer(typeof(int))]
    public class IntPropRenderer : PropRenderer
    {
        /// <summary>
        /// Underyling renderer.
        /// </summary>
        private readonly IntControlRenderer _renderer = new IntControlRenderer();

        /// <summary>
        /// Parameters for rendering.
        /// </summary>
        private ControlRendererParameter[] _parameters = new ControlRendererParameter[0];

        /// <inheritdoc cref="PropRenderer"/>
        public override bool Draw(ElementSchemaProp prop)
        {
            var cast = (ElementSchemaProp<int>)prop;
            var value = (object) cast.Value;
            var repaint = _renderer.Draw(prop.Name, ref value, ref _parameters);

            if ((int) value != cast.Value)
            {
                cast.Value = (int) value;
            }

            return repaint;
        }
    }

    /// <summary>
    /// Renderer for strings.
    /// </summary>
    [PropRenderer(typeof(string))]
    public class StringPropRenderer : PropRenderer
    {
        /// <summary>
        /// Renderer.
        /// </summary>
        private readonly StringControlRenderer _renderer = new StringControlRenderer();

        /// <summary>
        /// Parameters to render with.
        /// </summary>
        private ControlRendererParameter[] _parameters = new ControlRendererParameter[0];

        /// <inheritdoc cref="PropRenderer"/>
        public override bool Draw(ElementSchemaProp prop)
        {
            var cast = (ElementSchemaProp<string>)prop;
            var value = (object) cast.Value;
            var repaint = _renderer.Draw(prop.Name, ref value, ref _parameters);

            if ((string) value != cast.Value)
            {
                cast.Value = (string)value;
            }

            return repaint;
        }
    }

    /// <summary>
    /// Renderer for float props.
    /// </summary>
    [PropRenderer(typeof(float))]
    public class FloatPropRenderer : PropRenderer
    {
        /// <summary>
        /// Underlying renderer.
        /// </summary>
        private readonly FloatControlRenderer _renderer = new FloatControlRenderer();

        /// <summary>
        /// Parameters for rendering.
        /// </summary>
        private ControlRendererParameter[] _parameters = new ControlRendererParameter[0];

        /// <inheritdoc cref="PropRenderer"/>
        public override bool Draw(ElementSchemaProp prop)
        {
            var cast = (ElementSchemaProp<float>) prop;
            var value = (object) cast.Value;
            var repaint = _renderer.Draw(prop.Name, ref value, ref _parameters);

            if (Math.Abs((float) value - cast.Value) > Mathf.Epsilon)
            {
                cast.Value = (float) value;
            }

            return repaint;
        }
    }

    /// <summary>
    /// Renderer for vec3 props.
    /// </summary>
    [PropRenderer(typeof(Vec3))]
    public class Vec3PropRenderer : PropRenderer
    {
        /// <inheritdoc cref="PropRenderer"/>
        public override bool Draw(ElementSchemaProp prop)
        {
            var cast = (ElementSchemaProp<Vec3>) prop;
            var before = cast.Value;

            var after = EditorGUILayout.Vector3Field(prop.Name, before.ToVector()).ToVec();
            if (after.Approximately(before))
            {
                return false;
            }

            cast.Value = after;
            return true;
        }
    }

    /// <summary>
    /// Renderer for Vec2 props.
    /// </summary>
    [PropRenderer(typeof(Vec2))]
    public class Vec2PropRenderer : PropRenderer
    {
        /// <inheritdoc cref="PropRenderer"/>
        public override bool Draw(ElementSchemaProp prop)
        {
            var cast = (ElementSchemaProp<Vec2>)prop;
            var before = cast.Value;

            var after = EditorGUILayout.Vector2Field(prop.Name, before.ToVector()).ToVec();
            if (after.Approximately(before))
            {
                return false;
            }

            cast.Value = after;
            return true;
        }
    }

    /// <summary>
    /// Renderer for Color props.
    /// </summary>
    [PropRenderer(typeof(Col4))]
    public class ColorPropRenderer : PropRenderer
    {
        /// <inheritdoc cref="PropRenderer"/>
        public override bool Draw(ElementSchemaProp prop)
        {
            var cast = (ElementSchemaProp<Col4>) prop;
            var before = cast.Value;

            var after = EditorGUILayout.ColorField(prop.Name, before.ToColor()).ToCol();
            if (after.Approximately(before))
            {
                return false;
            }

            cast.Value = after;
            return true;
        }
    }
}