using System;
using System.Collections.Generic;
using Antlr4.Runtime;
using Antlr4.Runtime.Tree;
using CreateAR.SpirePlayer.IUX;

using ElementMap = CreateAR.Commons.Unity.DataStructures.Tuple<string, int>;

namespace CreateAR.SpirePlayer.Vine
{
    /// <summary>
    /// Listener that creates an <c>ElementDescription</c> from a vine.
    /// </summary>
    public class ElementDescriptionListener : IVineParserListener
    {
        /// <summary>
        /// Stores current attribute.
        /// </summary>
        private class AttributeData
        {
            public string Name;
            public string Value;
        }

        /// <summary>
        /// Stack of elements. The top is the one currently being edited.
        /// </summary>
        private readonly Stack<ElementData> _elements = new Stack<ElementData>();

        /// <summary>
        /// Maps strings to ints.
        /// </summary>
        private readonly List<ElementMap> _elementTypeMap = new List<ElementMap>
        {
            Commons.Unity.DataStructures.Tuple.Create("Container", ElementTypes.CONTAINER),
            Commons.Unity.DataStructures.Tuple.Create("Button", ElementTypes.BUTTON),
            Commons.Unity.DataStructures.Tuple.Create("Caption", ElementTypes.CAPTION),
            Commons.Unity.DataStructures.Tuple.Create("Menu", ElementTypes.MENU),
            Commons.Unity.DataStructures.Tuple.Create("Cursor", ElementTypes.CURSOR),
            Commons.Unity.DataStructures.Tuple.Create("TextCrawl", ElementTypes.TEXTCRAWL),
            Commons.Unity.DataStructures.Tuple.Create("Float", ElementTypes.FLOAT),
            Commons.Unity.DataStructures.Tuple.Create("Toggle", ElementTypes.TOGGLE),
            Commons.Unity.DataStructures.Tuple.Create("Slider", ElementTypes.SLIDER),
            Commons.Unity.DataStructures.Tuple.Create("Select", ElementTypes.SELECT),
            Commons.Unity.DataStructures.Tuple.Create("Grid", ElementTypes.GRID),
            Commons.Unity.DataStructures.Tuple.Create("Option", ElementTypes.OPTION),
            Commons.Unity.DataStructures.Tuple.Create("OptionGroup", ElementTypes.OPTION_GROUP)
        };

        /// <summary>
        /// The root data.
        /// </summary>
        private ElementData _root;
        
        /// <summary>
        /// Current attribute we're working on.
        /// </summary>
        private AttributeData _currentAttribute;

        /// <summary>
        /// Resulting description.
        /// </summary>
        public ElementDescription Description { get; private set; }

        /// <inheritdoc cref="IVineParserListener"/>
        public void VisitTerminal(ITerminalNode node)
        {
            //
        }

        /// <inheritdoc cref="IVineParserListener"/>
        public void VisitErrorNode(IErrorNode node)
        {
            //
        }

        /// <inheritdoc cref="IVineParserListener"/>
        public void EnterEveryRule(ParserRuleContext ctx)
        {
            //
        }

        /// <inheritdoc cref="IVineParserListener"/>
        public void ExitEveryRule(ParserRuleContext ctx)
        {
            //
        }

        /// <inheritdoc cref="IVineParserListener"/>
        public void EnterDocument(VineParser.DocumentContext context)
        {
            //
        }

        /// <inheritdoc cref="IVineParserListener"/>
        public void ExitDocument(VineParser.DocumentContext context)
        {
            if (null == _root)
            {
                throw new Exception("No root element found.");
            }

            Description = new ElementDescription
            {
                Elements = new[]
                {
                    _root
                }
            };

            if (string.IsNullOrEmpty(_root.Id))
            {
                _root.Id = Guid.NewGuid().ToString();
            }

            Description.Root = new ElementRef
            {
                Id = _root.Id
            };
        }

        /// <inheritdoc cref="IVineParserListener"/>
        public void EnterElement(VineParser.ElementContext context)
        {
            var name = context.GetChild(1);
            if (null == name)
            {
                throw new Exception(string.Format(
                    "Parser error, could not find element name. Please report! {0}",
                    GetExceptionLocation(context)));
            }

            var stringType = name.GetText();
            var type = ToIntElementType(stringType);
            if (-1 == type)
            {
                throw new Exception(string.Format(
                    "Invalid element type at {0}.",
                    GetExceptionLocation(context)));
            }

            EnterElementData(type);
        }

        /// <inheritdoc cref="IVineParserListener"/>
        public void ExitElement(VineParser.ElementContext context)
        {
            string stringType;

            var numChildren = context.ChildCount;
            var closingToken = context.children[numChildren - 1].ToString();
            if (closingToken == ">")
            {
                stringType = context.children[context.children.Count - 2].ToString();
            }
            else if (closingToken == "/>")
            {
                stringType = context.children[1].ToString();
            }
            else
            {
                throw new Exception(string.Format(
                    "Invalid element exit '{0}'. Parser/lexer error most likely. Please report this. {1}",
                    closingToken,
                    GetExceptionLocation(context)));
            }

            // check for matching types
            var type = ToIntElementType(stringType);
            var current = _elements.Peek();
            if (current.Type != type)
            {
                throw new Exception(string.Format(
                    "Invalid closing tag. Expected {0} but found {1} : {2}.",
                    ToStringElementType(current.Type),
                    stringType,
                    GetExceptionLocation(context)));
            }

            ExitElementData();
        }

        /// <inheritdoc cref="IVineParserListener"/>
        public void EnterContent(VineParser.ContentContext context)
        {
            //
        }

        /// <inheritdoc cref="IVineParserListener"/>
        public void ExitContent(VineParser.ContentContext context)
        {
            //
        }

        /// <inheritdoc cref="IVineParserListener"/>
        public void EnterMisc(VineParser.MiscContext context)
        {
            //
        }

        /// <inheritdoc cref="IVineParserListener"/>
        public void ExitMisc(VineParser.MiscContext context)
        {
            //
        }

        /// <inheritdoc cref="IVineParserListener"/>
        public void EnterComment(VineParser.CommentContext context)
        {
            //
        }

        /// <inheritdoc cref="IVineParserListener"/>
        public void ExitComment(VineParser.CommentContext context)
        {
            //
        }

        /// <inheritdoc cref="IVineParserListener"/>
        public void EnterScript(VineParser.ScriptContext context)
        {
            //
        }

        /// <inheritdoc cref="IVineParserListener"/>
        public void ExitScript(VineParser.ScriptContext context)
        {
            //
        }

        /// <inheritdoc cref="IVineParserListener"/>
        public void EnterAttribute(VineParser.AttributeContext context)
        {
            _currentAttribute = new AttributeData();
        }

        /// <inheritdoc cref="IVineParserListener"/>
        public void ExitAttribute(VineParser.AttributeContext context)
        {
            // 
        }

        /// <inheritdoc cref="IVineParserListener"/>
        public void EnterAttributeName(VineParser.AttributeNameContext context)
        {
            //
        }

        /// <inheritdoc cref="IVineParserListener"/>
        public void ExitAttributeName(VineParser.AttributeNameContext context)
        {
            _currentAttribute.Name = context.children[0].ToString();
        }

        /// <inheritdoc cref="IVineParserListener"/>
        public void EnterAttributeValue(VineParser.AttributeValueContext context)
        {
            //
        }

        /// <inheritdoc cref="IVineParserListener"/>
        public void ExitAttributeValue(VineParser.AttributeValueContext context)
        {
            _currentAttribute.Value = context.children[context.ChildCount - 1]
                .ToString();
            
            // identify type and add to current schema
            var name = _currentAttribute.Name;
            var value = _currentAttribute.Value;
            var current = _elements.Peek();
            if (value.StartsWith("'"))
            {
                // string!
                if (current.Schema.Strings.ContainsKey(name))
                {
                    throw new Exception(string.Format(
                        "Multiple string attributes by the name {0} defined : {1}.",
                        name,
                        GetExceptionLocation(context)));
                }

                current.Schema.Strings[name] = value.Trim('\'');
            }
            else if (value.StartsWith("("))
            {
                // vec3
                if (current.Schema.Vectors.ContainsKey(name))
                {
                    throw new Exception(string.Format(
                        "Multiple Vec3 attributes by the name {0} defined : {1}.",
                        name,
                        GetExceptionLocation(context)));
                }

                // parse
                value = value.Replace(" ", "");
                value = value.TrimStart('(');
                value = value.TrimEnd(')');

                var substrings = value.Split(',');
                if (3 != substrings.Length)
                {
                    throw new Exception(string.Format(
                        "Vec3 could not be parsed for attribute {0} : {1}.",
                        name,
                        GetExceptionLocation(context)));
                }

                float x, y, z;
                if (!float.TryParse(substrings[0], out x)
                    || !float.TryParse(substrings[1], out y)
                    || !float.TryParse(substrings[2], out z))
                {
                    throw new Exception(string.Format(
                        "Vec3 could not be parsed for attribute {0} : {1}.",
                        name,
                        GetExceptionLocation(context)));
                }

                current.Schema.Vectors[name] = new Vec3(x, y, z);
            }
            else if (value.Contains("."))
            {
                // float
                if (current.Schema.Floats.ContainsKey(name))
                {
                    throw new Exception(string.Format(
                        "Multiple float attributes by the name {0} defined : {1}.",
                        name,
                        GetExceptionLocation(context)));
                }

                float floatValue;
                if (!float.TryParse(value, out floatValue))
                {
                    throw new Exception(string.Format(
                        "Float could not be parsed for attribute {0} : {1}.",
                        name,
                        GetExceptionLocation(context)));
                }

                current.Schema.Floats[name] = floatValue;
            }
            else if (value == "true" || value == "false")
            {
                // bool
                if (current.Schema.Bools.ContainsKey(name))
                {
                    throw new Exception(string.Format(
                        "Multiple bool attributes by the name {0} defined : {1}.",
                        name,
                        GetExceptionLocation(context)));
                }

                current.Schema.Bools[name] = "true" == value;
            }
            else
            {
                // int
                if (current.Schema.Ints.ContainsKey(name))
                {
                    throw new Exception(string.Format(
                        "Multiple int attributes by the name {0} defined : {1}.",
                        name,
                        GetExceptionLocation(context)));
                }

                int intValue;
                if (!int.TryParse(value, out intValue))
                {
                    throw new Exception(string.Format(
                        "Int could not be parsed for attribute {0} : {1}.",
                        name,
                        GetExceptionLocation(context)));
                }

                current.Schema.Ints[name] = intValue;
            }
        }

        /// <summary>
        /// Creates a new instance of <c>ElementData</c> when an element has been
        /// entered.
        /// </summary>
        /// <param name="type">The type to create.</param>
        private void EnterElementData(int type)
        {
            var element = new ElementData
            {
                Type = type
            };

            // add child
            if (0 != _elements.Count)
            {
                var current = _elements.Peek();
                current.Children = current.Children.Add(element);
            }
            else
            {
                // root
                if (null != _root)
                {
                    throw new Exception("Document contains multiple root elements.");
                }

                _root = element;
            }

            // push onto stack
            _elements.Push(element);
        }

        /// <summary>
        /// Exits an element.
        /// </summary>
        private void ExitElementData()
        {
            _elements.Pop();
        }

        /// <summary>
        /// Retrieves a location for an error.
        /// </summary>
        /// <param name="context">The context to use.</param>
        /// <returns></returns>
        private static string GetExceptionLocation(ParserRuleContext context)
        {
            return string.Format(
                "[{0}:{1} - {2}]",
                context.SourceInterval.a,
                context.SourceInterval.b,
                context.GetText());
        }

        /// <summary>
        /// Translates an int element type to a string element type.
        /// </summary>
        /// <param name="type">The type to translate.</param>
        /// <returns></returns>
        private string ToStringElementType(int type)
        {
            for (int i = 0, len = _elementTypeMap.Count; i < len; i++)
            {
                var map = _elementTypeMap[i];
                if (map.Item2 == type)
                {
                    return map.Item1;
                }
            }

            return string.Empty;
        }

        /// <summary>
        /// Translates an string element type to a int element type.
        /// </summary>
        /// <param name="type">The type to translate.</param>
        /// <returns></returns>
        private int ToIntElementType(string type)
        {
            for (int i = 0, len = _elementTypeMap.Count; i < len; i++)
            {
                var map = _elementTypeMap[i];
                if (map.Item1 == type)
                {
                    return map.Item2;
                }
            }

            return -1;
        }
    }
}