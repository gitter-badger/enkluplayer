using System.Text.RegularExpressions;

namespace CreateAR.EnkluPlayer.IUX
{
    /// <summary>
    /// Query object.
    /// </summary>
    internal class ElementQuery
    {
        private const string OPERATOR_EQUALS = "==";
        private const string OPERATOR_NOT_EQUALS = "!=";
        private const string OPERATOR_LT = "<";
        private const string OPERATOR_LT_EQUALS = "<=";
        private const string OPERATOR_GT = ">";
        private const string OPERATOR_GT_EQUALS = ">=";

        /// <summary>
        /// Regex for name matches.
        /// </summary>
        private static readonly Regex NAME_QUERY = new Regex(@"^([\w-_\.]+)$");

        /// <summary>
        /// Regex for property matches.
        /// </summary>
        private static readonly Regex PROPERTY_QUERY = new Regex(@"^\(@([\w-_\.]+)([<>=!]=?)(\w+)\)$");

        /// <summary>
        /// Property to look for.
        /// </summary>
        private string _propName;

        /// <summary>
        /// Value to look for.
        /// </summary>
        private string _propValue;

        /// <summary>
        /// Operator to look for.
        /// </summary>
        private string _operator;
        
        /// <summary>
        /// True iff this is a valid query.
        /// </summary>
        public bool IsValid { get; private set; }

        /// <summary>
        /// Creates a new query.
        /// </summary>
        /// <param name="value">Query value.</param>
        public ElementQuery(string value)
        {
            Parse(value);
        }

        /// <summary>
        /// Determines whether or not an element is a match.
        /// </summary>
        /// <param name="element">Element in question.</param>
        /// <returns></returns>
        public bool Execute(Element element)
        {
            if (_propName == "*")
            {
                return true;
            }

            if (_propName == "id")
            {
                return element.Id == _propValue;
            }

            if (_propName == "type")
            {
                return element.GetType().Name == _propValue;
            }

            // Early out if the element doesn't contain the prop itself.
            if (!element.Schema.HasOwnProp(_propName))
            {
                return false;
            }
            
            // bool
            if (_propValue == "true" || _propValue == "false")
            {
                var boolValue = _operator == OPERATOR_EQUALS
                    ? _propValue == "true"
                    : _propValue == "false";
                return boolValue == element.Schema.GetOwn(_propName, default(bool)).Value;
            }

            // int
            int intValue;
            if (int.TryParse(_propValue, out intValue))
            {
                var val = element.Schema.GetOwn(_propName, default(int)).Value;
                switch (_operator)
                {
                    case OPERATOR_EQUALS:
                    {
                        return intValue == val;
                    }
                    case OPERATOR_NOT_EQUALS:
                    {
                        return intValue != val;
                    }
                    case OPERATOR_GT:
                    {
                        return val > intValue;
                    }
                    case OPERATOR_GT_EQUALS:
                    {
                        return val >= intValue;
                    }
                    case OPERATOR_LT:
                    {
                        return val < intValue;
                    }
                    case OPERATOR_LT_EQUALS:
                    {
                        return val <= intValue;
                    }
                    default:
                    {
                        return false;
                    }
                }
            }

            // string
            return _propValue == element.Schema.GetOwn(_propName, string.Empty).Value;
        }

        /// <summary>
        /// Parses the query.
        /// </summary>
        /// <param name="value">Value of the query.</param>
        private void Parse(string value)
        {
            if (value == "*")
            {
                IsValid = true;

                _propName = "*";

                return;
            }

            var match = NAME_QUERY.Match(value);
            if (match.Success)
            {
                _propName = "id";
                _operator = OPERATOR_EQUALS;
                _propValue = match.Groups[1].Value;
                
                IsValid = true;
            }
            else
            {
                match = PROPERTY_QUERY.Match(value);

                if (match.Success) 
                {
                    _propName = match.Groups[1].Value;
                    _operator = match.Groups[2].Value;
                    if (_operator == "=")
                    {
                        _operator = OPERATOR_EQUALS;
                    }

                    _propValue = match.Groups[3].Value;
                    
                    IsValid = true;
                }
            }
        }
    }
}