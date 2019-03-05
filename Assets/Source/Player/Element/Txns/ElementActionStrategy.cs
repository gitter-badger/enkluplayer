using System.Diagnostics;
using CreateAR.Commons.Unity.Logging;
using CreateAR.EnkluPlayer.IUX;
using Enklu.Data;
using Newtonsoft.Json;
using ElementDescription = CreateAR.EnkluPlayer.IUX.ElementDescription;
using ElementRef = CreateAR.EnkluPlayer.IUX.ElementRef;

namespace CreateAR.EnkluPlayer
{
    /// <summary>
    /// Strategy to apply actions to elements.
    /// </summary>
    public class ElementActionStrategy : IElementActionStrategy
    {
        /// <summary>
        /// Creates elements.
        /// </summary>
        private readonly IElementFactory _elements;

        /// <inheritdoc />
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

        /// <inheritdoc />
        public bool ApplyCreateAction(
            ElementActionData action,
            out string error)
        {
            LogVerbose("Apply Create Action");

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
                    Id = action.Element.Id
                },
                Elements = new[]
                {
                    action.Element
                }
            });

            parent.AddChild(element);

            error = string.Empty;
            return true;
        }

        /// <inheritdoc />
        public bool ApplyDeleteAction(
            ElementActionData action,
            out string error)
        {
            LogVerbose("Apply Delete Action");

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

        /// <inheritdoc />
        public bool ApplyUpdateAction(
            ElementActionData action,
            out string error)
        {
            LogVerbose("Apply Update Action");

            // cannot delete root, so just find with ..
            var element = Element.FindOne<Element>(".." + action.ElementId);
            if (null == element)
            {
                error = string.Format(
                    "Could not find element {0} to update it!",
                    action.ElementId);
                return false;
            }

            switch (action.SchemaType)
            {
                case ElementActionSchemaTypes.STRING:
                {
                    LogVerbose(
                        "Setting [Element id={0}].strings[{1}] = {2}",
                        action.ElementId,
                        action.Key,
                        action.Value);

                    var prop = element.Schema.Get<string>(action.Key);
                    prop.Value = action.Value.ToString();

                    break;
                }
                case ElementActionSchemaTypes.INT:
                {
                    LogVerbose(
                        "Setting [Element id={0}].ints[{1}] = {2}",
                        action.ElementId,
                        action.Key,
                        action.Value);

                    var prop = element.Schema.Get<int>(action.Key);
                    prop.Value = (int) action.Value;

                    break;
                }
                case ElementActionSchemaTypes.FLOAT:
                {
                    LogVerbose(
                        "Setting [Element id={0}].float[{1}] = {2}",
                        action.ElementId,
                        action.Key,
                        action.Value);

                    var prop = element.Schema.Get<float>(action.Key);
                    prop.Value = (float) action.Value;

                    break;
                }
                case ElementActionSchemaTypes.BOOL:
                {
                    LogVerbose(
                        "Setting [Element id={0}].bools[{1}] = {2}",
                        action.ElementId,
                        action.Key,
                        action.Value);

                    var prop = element.Schema.Get<bool>(action.Key);
                    prop.Value = (bool) action.Value;

                    break;
                }
                case ElementActionSchemaTypes.VEC3:
                {
                    LogVerbose(
                        "Setting [Element id={0}].vectors[{1}] = {2}",
                        action.ElementId,
                        action.Key,
                        action.Value);

                    var prop = element.Schema.Get<Vec3>(action.Key);
                    prop.Value = (Vec3) action.Value;

                    break;
                }
                case ElementActionSchemaTypes.COL4:
                {
                    LogVerbose(
                        "Setting [Element id={0}].colors[{1}] = {2}",
                        action.ElementId,
                        action.Key,
                        action.Value);

                    var prop = element.Schema.Get<Col4>(action.Key);
                    prop.Value = (Col4) action.Value;

                    break;
                }
                default:
                {
                    error = string.Format(
                        "Invalid schema type '{0}'.",
                        action.SchemaType);

                    return false;
                }
            }

            error = string.Empty;
            return true;
        }

        /// <inheritdoc />
        public bool ApplyUpdateAction(
            ElementActionUpdateRecord record,
            out string error)
        {
            LogVerbose("Apply Update Action");

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

                    // un-escape if necessary
                    var value = record.NextValue.ToString();
                    value = value.Trim('"');

                    prop.Value = value;

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

        /// <inheritdoc />
        public bool ApplyMoveAction(ElementActionData action, out string error)
        {
            LogVerbose("Apply Move Action.");

            // find element (non-root only)
            var element = Element.FindOne<Element>(".." + action.ElementId);
            if (null == element)
            {
                error = string.Format(
                    "Could not find element with id '{0}'.",
                    action.ElementId);
                return false;
            }

            // find parent
            Element parent = null;
            if (Element.Id == action.ParentId)
            {
                parent = Element;
            }
            else
            {
                parent = Element.FindOne<Element>(".." + action.ParentId);
            }

            if (null == parent)
            {
                error = string.Format(
                    "Could not find parent with id '{0}'.",
                    action.ParentId);
                return false;
            }

            LogVerbose("\tAddChild.");

            parent.AddChild(element);

            LogVerbose("\tSet new position: {0}", action.Value);

            element.Schema.Set("position", (Vec3) action.Value);

            error = string.Empty;
            return true;
        }

        /// <summary>
        /// Verbose logging method.
        /// </summary>
        //[Conditional("LOGGING_VERBOSE")]
        private void LogVerbose(string message, params object[] replacements)
        {
            Log.Info(this, message, replacements);
        }
    }
}