using System;
using System.Collections.Generic;
using Antlr4.Runtime;
using Antlr4.Runtime.Tree;
using CreateAR.Commons.Unity.DataStructures;
using CreateAR.SpirePlayer.IUX;
using Vine;

using ElementMap = CreateAR.Commons.Unity.DataStructures.Tuple<string, int>;

namespace CreateAR.SpirePlayer.Vine
{
    public class ElementDescriptionListener : IVineParserListener
    {
        private readonly ElementDescription _description;
        private readonly Stack<ElementData> _elements = new Stack<ElementData>();
        private readonly List<ElementMap> _elementTypeMap = new List<ElementMap>
        {
            CreateAR.Commons.Unity.DataStructures.Tuple.Create("Container", ElementTypes.CONTAINER),
            CreateAR.Commons.Unity.DataStructures.Tuple.Create("Button", ElementTypes.BUTTON),
            CreateAR.Commons.Unity.DataStructures.Tuple.Create("Caption", ElementTypes.CAPTION),
            CreateAR.Commons.Unity.DataStructures.Tuple.Create("Menu", ElementTypes.MENU),
            CreateAR.Commons.Unity.DataStructures.Tuple.Create("Cursor", ElementTypes.CURSOR)
        };

        private ElementData _root;
        private ElementData _current;

        public bool Success { get; private set; }

        public ElementDescriptionListener(ElementDescription description)
        {
            _description = description;
        }

        public void VisitTerminal(ITerminalNode node)
        {
                
        }

        public void VisitErrorNode(IErrorNode node)
        {
                
        }

        public void EnterEveryRule(ParserRuleContext ctx)
        {
                
        }

        public void ExitEveryRule(ParserRuleContext ctx)
        {
                
        }

        public void EnterDocument(VineParser.DocumentContext context)
        {
            //
        }

        public void ExitDocument(VineParser.DocumentContext context)
        {
            if (null == _root)
            {
                throw new Exception("No root element found.");
            }

            _description.Elements = new[]
            {
                _root
            };

            if (string.IsNullOrEmpty(_root.Id))
            {
                _root.Id = Guid.NewGuid().ToString();
            }

            _description.Root = new ElementRef
            {
                Id = _root.Id
            };
            
            Success = true;
        }

        public void EnterElement(VineParser.ElementContext context)
        {
            var stringType = context.children[1].GetText();
            var type = ToIntElementType(stringType);
            if (-1 == type)
            {
                throw new Exception(string.Format(
                    "Invalid element type at {0}.",
                    GetExceptionLocation(context)));
            }

            EnterElementData(type);
        }

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
                    "Invalid element exit. Parser/lexer error most likely. Please report this. {0}",
                    GetExceptionLocation(context)));
            }

            // check for matching types
            var type = ToIntElementType(stringType);
            if (_current.Type != type)
            {
                throw new Exception(string.Format(
                    "Invalid closing tag. Expected {0} but found {1} : {2}.",
                    ToStringElementType(_current.Type),
                    stringType,
                    GetExceptionLocation(context)));
            }

            ExitElementData();
        }

        private void EnterElementData(int type)
        {
            var element = new ElementData
            {
                Type = type
            };

            // add child
            if (null != _current)
            {
                _current.Children = _current.Children.Add(element);
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
            _current = element;
        }

        private void ExitElementData()
        {
            _elements.Pop();

            if (_elements.Count > 0)
            {
                _current = _elements.Peek();
            }
            else
            {
                _current = null;
            }
        }

        public void EnterContent(VineParser.ContentContext context)
        {
                
        }

        public void ExitContent(VineParser.ContentContext context)
        {
                
        }

        public void EnterMisc(VineParser.MiscContext context)
        {
                
        }

        public void ExitMisc(VineParser.MiscContext context)
        {
                
        }

        public void EnterComment(VineParser.CommentContext context)
        {
                
        }

        public void ExitComment(VineParser.CommentContext context)
        {
                
        }

        public void EnterAttribute(VineParser.AttributeContext context)
        {
                
        }

        public void ExitAttribute(VineParser.AttributeContext context)
        {
                
        }

        public void EnterAttributeName(VineParser.AttributeNameContext context)
        {
                
        }

        public void ExitAttributeName(VineParser.AttributeNameContext context)
        {
                
        }

        public void EnterAttributeValue(VineParser.AttributeValueContext context)
        {
                
        }

        public void ExitAttributeValue(VineParser.AttributeValueContext context)
        {
                
        }

        private static string GetExceptionLocation(ParserRuleContext context)
        {
            return string.Format(
                "[{0}:{1} - {2}]",
                context.SourceInterval.a,
                context.SourceInterval.b,
                context.GetText());
        }

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