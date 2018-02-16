using CreateAR.Commons.Unity.Logging;
using CreateAR.SpirePlayer.IUX;
using LightJson;

namespace CreateAR.SpirePlayer
{
    public class ElementActionData
    {
        [JsonName("id")]
        public string Id;

        [JsonName("type")]
        public string Type;

        [JsonName("elementId")]
        public string ElementId;

        [JsonName("elementType")]
        public int ElementType;

        [JsonName("parentId")]
        public string ParentId;

        [JsonName("schemaType")]
        public string SchemaType;

        [JsonName("key")]
        public string Key;

        [JsonName("value")]
        public string Value;
    }

    public class ElementActionStrategy
    {
        private readonly IElementFactory _elements;

        public Element Element { get; private set; }

        public ElementActionStrategy(IElementFactory elements)
        {
            _elements = elements;
        }

        public void Apply(ElementActionData[] actions)
        {
            for (int i = 0, len = actions.Length; i < len; i++)
            {
                var action = actions[i];
                switch (action.Type)
                {
                    case "create":
                    {
                        var element = _elements.Element(new ElementDescription
                        {
                            Root = new ElementRef
                            {
                                Id = action.ElementId
                            },
                            Elements = new []
                            {
                                new ElementData
                                {
                                    Id = action.ElementId,
                                    Type = action.ElementType
                                }
                            }
                        });

                        Element.AddChild(element);

                        break;
                    }
                    case "delete":
                    {
                        var element = Element.FindOne<Element>(".." + action.Id);
                        if (null == element)
                        {
                            Log.Error(this,
                                "Could not find element {0} to delete it!",
                                action.Id);
                            return;
                        }

                        element.Destroy();

                        break;
                    }
                    case "update":
                    {
                        var element = Element.Id == action.Id
                            ? Element
                            : Element.FindOne<Element>(".." + action.Id);
                        if (null == element)
                        {
                            Log.Error(this,
                                "Could not find element {0} to delete it!",
                                action.Id);
                            return;
                        }

                        switch (action.SchemaType)
                        {
                            case "string":
                            {
                                element.Schema.Set(action.Key, action.Value);
                                break;
                            }
                            case "int":
                            {
                                int val;
                                if (int.TryParse(action.Value, out val))
                                {
                                    element.Schema.Set(action.Key, val);
                                }
                                else
                                {
                                    Log.Error(this,
                                        "Could not parse int value from {0}.",
                                        action.Value);
                                }
                                
                                break;
                            }
                            case "float":
                            {
                                float val;
                                if (float.TryParse(action.Value, out val))
                                {
                                    element.Schema.Set(action.Key, val);
                                }
                                else
                                {
                                    Log.Error(this,
                                        "Could not parse float value from {0}.",
                                        action.Value);
                                }

                                break;
                            }
                            case "bool":
                            {
                                if (action.Value == "true")
                                {
                                    element.Schema.Set(action.Key, true);
                                }
                                else if (action.Value == "false")
                                {
                                    element.Schema.Set(action.Key, false);
                                }
                                else
                                {
                                    Log.Error(this,
                                        "Could not parse bool from {0}.",
                                        action.Value);
                                }

                                break;
                            }
                            case "vec3":
                            {
                                break;
                            }
                            case "col4":
                            {
                                break;
                            }
                        }

                        break;
                    }
                    default:
                    {
                        break;
                    }
                }
            }
        }
    }

    public class ElementTxnManager
    {

    }
}
