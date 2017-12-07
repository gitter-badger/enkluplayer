using System;
using CreateAR.Commons.Unity.Editor;
using CreateAR.SpirePlayer.IUX;
using UnityEditor;

namespace CreateAR.SpirePlayer.Editor
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
            var value = (object)cast.Value;
            var repaint = _renderer.Draw(prop.Name, ref value, ref _parameters);
            cast.Value = (int)value;

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
            var value = (object)cast.Value;
            var repaint = _renderer.Draw(prop.Name, ref value, ref _parameters);
            cast.Value = (string)value;

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
            cast.Value = (float) value;

            return repaint;
        }
    }

    /// <summary>
    /// Renderer for float props.
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
}