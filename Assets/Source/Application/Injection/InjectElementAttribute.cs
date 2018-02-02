using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using CreateAR.SpirePlayer.IUX;

namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// Marks a property to receive an element.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class InjectElementAttribute : Attribute
    {
        /// <summary>
        /// Scratch list for elements.
        /// </summary>
        private static readonly List<Element> _elementScratch = new List<Element>();

        /// <summary>
        /// Element query.
        /// </summary>
        public string Query { get; private set; }

        /// <summary>
        /// Constructor.
        /// </summary>
        public InjectElementAttribute(string query)
        {
            Query = query;
        }

        /// <summary>
        /// Injects elements into properties.
        /// </summary>
        public static void InjectElements(
            object @object,
            Element start)
        {
            var props = @object.GetType().GetProperties(
                BindingFlags.Instance
                | BindingFlags.Public
                | BindingFlags.NonPublic);

            for (int i = 0, len = props.Length; i < len; i++)
            {
                var prop = props[i];
                var attributes = prop.GetCustomAttributes(typeof(InjectElementAttribute), true);
                if (0 == attributes.Length)
                {
                    continue;
                }

                var query = ((InjectElementAttribute)attributes[0]).Query;

                // single injection or multiple
                var type = prop.PropertyType;
                if (type.IsArray)
                {
                    var elementType = type.GetElementType();

                    _elementScratch.Clear();
                    start.Find(query, _elementScratch);
                    
                    // filter list
                    for (var j = _elementScratch.Count - 1; j >= 0; j--)
                    {
                        var element = _elementScratch[j];
                        if (!elementType.IsInstanceOfType(element))
                        {
                            _elementScratch.RemoveAt(j);
                        }
                    }

                    var arr = Array.CreateInstance(
                        elementType,
                        _elementScratch.Count);
                    prop.SetValue(@object, arr, null);

                    // copy to array
                    for (var j = _elementScratch.Count - 1; j >= 0; j--)
                    {
                        arr.SetValue(_elementScratch[j], j);
                    }
                }
                else if (typeof(Element).IsAssignableFrom(type))
                {
                    prop.SetValue(@object, start.FindOne<Element>(query), null);
                }
            }
        }
    }
}