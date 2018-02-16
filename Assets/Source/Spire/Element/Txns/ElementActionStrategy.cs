using System.Text;
using CreateAR.Commons.Unity.Logging;
using CreateAR.SpirePlayer.IUX;

namespace CreateAR.SpirePlayer
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

        /// <summary>
        /// Json-izer.
        /// </summary>
        private readonly JsonSerializer _serializer;

        /// <inheritdoc />
        public Element Element { get; private set; }
        
        /// <summary>
        /// Constructor.
        /// </summary>
        public ElementActionStrategy(
            IElementFactory elements,
            JsonSerializer serializer)
        {
            _elements = elements;
            _serializer = serializer;
        }

        /// <inheritdoc />
        public void Initialize(Element root)
        {
            Element = root;
        }

        /// <inheritdoc />
        public void Uninitialize()
        {
            Element = null;
        }

        /// <inheritdoc />
        public bool Apply(ElementActionData action, out string error)
        {
            if (null == Element)
            {
                error = "Root element is null. Did you forget to call Initialize first?";
                return false;
            }
            
            switch (action.Type)
            {
                case ElementActionTypes.CREATE:
                {
                    return ApplyCreateAction(action, out error);
                }
                case ElementActionTypes.DELETE:
                {
                    return ApplyDeleteAction(action, out error);
                }
                case ElementActionTypes.UPDATE:
                {
                    return ApplyUpdateAction(action, out error);
                }
                default:
                {
                    error = string.Format(
                        "Unknown action type '{0}'.",
                        action.Type);

                    return false;
                }
            }
        }

        /// <summary>
        /// Appllies a create action.
        /// </summary>
        /// <param name="action">The action to apply.</param>
        /// <param name="error">The error, if any.</param>
        private bool ApplyCreateAction(
            ElementActionData action,
            out string error)
        {
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

            Element.AddChild(element);

            error = string.Empty;
            return true;
        }

        /// <summary>
        /// Applies a delete action.
        /// </summary>
        /// <param name="action">The action to apply.</param>
        /// <param name="error">The error, if any.</param>
        private bool ApplyDeleteAction(
            ElementActionData action,
            out string error)
        {
            // cannot delete root, so just find with ..
            var element = Element.FindOne<Element>(".." + action.Id);
            if (null == element)
            {
                error = string.Format(
                    "Could not find element {0} to delete it!",
                    action.Id);
                return false;
            }

            element.Destroy();

            error = string.Empty;
            return true;
        }

        /// <summary>
        /// Applies an update action.
        /// </summary>
        /// <param name="action">The action.</param>
        /// <param name="error">The error, if any.</param>
        private bool ApplyUpdateAction(
            ElementActionData action,
            out string error)
        {
            var element = Element.Id == action.Id
                ? Element
                : Element.FindOne<Element>(".." + action.Id);
            if (null == element)
            {
                error = string.Format(
                    "Could not find element {0} to delete it!",
                    action.Id);
                return false;
            }

            switch (action.SchemaType)
            {
                case ElementActionSchemaTypes.STRING:
                {
                    LogVerbose(
                        "Setting [Element id={0}].strings[{1}] = {2}",
                        action.Id,
                        action.Key,
                        action.Value);

                    element.Schema.Set(action.Key, action.Value);
                    break;
                }
                case ElementActionSchemaTypes.INT:
                {
                    int val;
                    if (int.TryParse(action.Value, out val))
                    {
                        LogVerbose(
                            "Setting [Element id={0}].ints[{1}] = {2}",
                            action.Id,
                            action.Key,
                            val);

                        element.Schema.Set(action.Key, val);
                    }
                    else
                    {
                        error = string.Format(
                            "Could not parse int value from {0}.",
                            action.Value);
                        return false;
                    }

                    break;
                }
                case ElementActionSchemaTypes.FLOAT:
                {
                    float val;
                    if (float.TryParse(action.Value, out val))
                    {
                        LogVerbose(
                            "Setting [Element id={0}].floats[{1}] = {2}",
                            action.Id,
                            action.Key,
                            val);

                        element.Schema.Set(action.Key, val);
                    }
                    else
                    {
                        error = string.Format(
                            "Could not parse float value from {0}.",
                            action.Value);
                        return false;
                    }

                    break;
                }
                case ElementActionSchemaTypes.BOOL:
                {
                    if (action.Value == "true")
                    {
                        LogVerbose(
                            "Setting [Element id={0}].bools[{1}] = {2}",
                            action.Id,
                            action.Key,
                            true);

                        element.Schema.Set(action.Key, true);
                    }
                    else if (action.Value == "false")
                    {
                        LogVerbose(
                            "Setting [Element id={0}].bools[{1}] = {2}",
                            action.Id,
                            action.Key,
                            false);

                        element.Schema.Set(action.Key, false);
                    }
                    else
                    {
                        error = string.Format(
                            "Could not parse bool from {0}.",
                            action.Value);
                        return false;
                    }

                    break;
                }
                case ElementActionSchemaTypes.VEC3:
                {
                    object val;
                    var bytes = Encoding.UTF8.GetBytes(action.Value);
                    _serializer.Deserialize(typeof(Vec3), ref bytes, out val);

                    LogVerbose(
                        "Setting [Element id={0}].vectors[{1}] = {2}",
                        action.Id,
                        action.Key,
                        val);

                    element.Schema.Set(action.Key, (Vec3) val);
                    
                    break;
                }
                case ElementActionSchemaTypes.COL4:
                {
                    object val;
                    var bytes = Encoding.UTF8.GetBytes(action.Value);
                    _serializer.Deserialize(typeof(Col4), ref bytes, out val);

                    LogVerbose(
                        "Setting [Element id={0}].colors[{1}] = {2}",
                        action.Id,
                        action.Key,
                        val);

                    element.Schema.Set(action.Key, (Col4) val);

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

        //[Conditional("LOGGING_VERBOSE")]
        private void LogVerbose(string message, params object[] replacements)
        {
            Log.Info(this, message, replacements);
        }
    }
}