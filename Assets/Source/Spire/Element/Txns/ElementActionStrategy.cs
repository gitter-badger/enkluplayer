﻿using CreateAR.Commons.Unity.Logging;
using CreateAR.SpirePlayer.IUX;

namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// Strategy to apply actions to elements.
    /// </summary>
    public class ElementActionStrategy
    {
        /// <summary>
        /// Creates elements.
        /// </summary>
        private readonly IElementFactory _elements;
        
        public Element Element { get; private set; }
        
        /// <summary>
        /// Constructor.
        /// </summary>
        public ElementActionStrategy(
            IElementFactory elements,
            Element root)
        {
            _elements = elements;

            Element = root;
        }
        
        /// <summary>
        /// Appllies a create action.
        /// </summary>
        /// <param name="action">The action to apply.</param>
        /// <param name="error">The error, if any.</param>
        public bool ApplyCreateAction(
            ElementActionData action,
            out string error)
        {
            var parent = Element;
            if (parent.Id != action.ParentId)
            {
                parent = Element.FindOne<Element>(".." + action.ParentId);
            }

            if (null == parent)
            {
                error = string.Format(
                    "Could not find parent '{0}'.",
                    action.ParentId);
                return false;
            }

            var element = _elements.Element(new ElementDescription
            {
                Root = new ElementRef
                {
                    Id = action.ElementId
                },
                Elements = new[]
                {
                    new ElementData
                    {
                        Id = action.ElementId,
                        Type = action.ElementType
                    }
                }
            });

            parent.AddChild(element);

            error = string.Empty;
            return true;
        }

        /// <summary>
        /// Applies a delete action.
        /// </summary>
        /// <param name="action">The action to apply.</param>
        /// <param name="error">The error, if any.</param>
        public bool ApplyDeleteAction(
            ElementActionData action,
            out string error)
        {
            // cannot delete root, so just find with ..
            var element = Element.FindOne<Element>(".." + action.ElementId);
            if (null == element)
            {
                error = string.Format(
                    "Could not find element {0} to delete it!",
                    action.ElementId);
                return false;
            }

            element.Destroy();

            error = string.Empty;
            return true;
        }
        
        /// <summary>
        /// Applies an update record.
        /// </summary>
        /// <param name="record">Record that contains new state and allows storing prev state.</param>
        /// <param name="error">The error, if any.</param>
        public bool ApplyUpdateAction(
            ElementActionUpdateRecord record,
            out string error)
        {
            var element = record.Element;
            
            switch (record.SchemaType)
            {
                case ElementActionSchemaTypes.STRING:
                {
                    LogVerbose(
                        "Setting [Element id={0}].strings[{1}] = {2}",
                        record.Element.Id,
                        record.Key,
                        record.NextValue);

                    var prop = element.Schema.Get<string>(record.Key);

                    record.PrevValue = prop.Value;
                    
                    prop.Value = record.NextValue.ToString();

                    break;
                }
                case ElementActionSchemaTypes.INT:
                {
                    LogVerbose(
                        "Setting [Element id={0}].ints[{1}] = {2}",
                        record.Element.Id,
                        record.Key,
                        record.NextValue);

                    var prop = element.Schema.Get<int>(record.Key);

                    record.PrevValue = prop.Value;

                    prop.Value = (int) record.NextValue;

                    break;
                }
                case ElementActionSchemaTypes.FLOAT:
                {
                    LogVerbose(
                        "Setting [Element id={0}].float[{1}] = {2}",
                        record.Element.Id,
                        record.Key,
                        record.NextValue);

                    var prop = element.Schema.Get<float>(record.Key);

                    record.PrevValue = prop.Value;

                    prop.Value = (float) record.NextValue;

                    break;
                }
                case ElementActionSchemaTypes.BOOL:
                {
                    LogVerbose(
                        "Setting [Element id={0}].bools[{1}] = {2}",
                        record.Element.Id,
                        record.Key,
                        record.NextValue);

                    var prop = element.Schema.Get<bool>(record.Key);

                    record.PrevValue = prop.Value;

                    prop.Value = (bool) record.NextValue;

                    break;
                }
                case ElementActionSchemaTypes.VEC3:
                {
                    LogVerbose(
                        "Setting [Element id={0}].vectors[{1}] = {2}",
                        record.Element.Id,
                        record.Key,
                        record.NextValue);

                    var prop = element.Schema.Get<Vec3>(record.Key);

                    record.PrevValue = prop.Value;

                    prop.Value = (Vec3) record.NextValue;

                    break;
                }
                case ElementActionSchemaTypes.COL4:
                {
                    LogVerbose(
                        "Setting [Element id={0}].colors[{1}] = {2}",
                        record.Element.Id,
                        record.Key,
                        record.NextValue);

                    var prop = element.Schema.Get<Col4>(record.Key);

                    record.PrevValue = prop.Value;

                    prop.Value = (Col4) record.NextValue;

                    break;
                }
                default:
                {
                    error = string.Format(
                        "Invalid schema type '{0}'.",
                        record.SchemaType);

                    return false;
                }
            }

            error = string.Empty;
            return true;
        }

        //[Conditional("LOGGING_VERBOSE")]
        private void LogVerbose(string message, params object[] replacements)
        {
            Log.Info(this, message, replacements);
        }
    }
}