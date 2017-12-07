using System.Text.RegularExpressions;

namespace CreateAR.SpirePlayer.IUX
{
    /// <summary>
    /// Query object.
    /// </summary>
    internal class ElementQuery
    {
        /// <summary>
        /// Regex for name matches.
        /// </summary>
        private static readonly Regex NAME_QUERY = new Regex(@"^(\w+)$");

        /// <summary>
        /// Regex for property matches.
        /// </summary>
        private static readonly Regex PROPERTY_QUERY = new Regex(@"^\(@(\w+)=(\w+)\)$");

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
        /// Start index.
        /// </summary>
        private int _startIndex;

        /// <summary>
        /// End index.
        /// </summary>
        private int _endIndex;

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
            
            // bool
            if (_propValue == "true" || _propValue == "false")
            {
                var boolValue = _propValue == "true";
                return boolValue == element.Schema.Get<bool>(_propName).Value;
            }

            // int
            int intValue;
            if (int.TryParse(_propValue, out intValue))
            {
                return intValue == element.Schema.Get<int>(_propName).Value;
            }

            // string
            return _propValue == element.Schema.Get<string>(_propName).Value;
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
                _operator = "==";
                _propValue = match.Groups[1].Value;
                _startIndex = 0;
                _endIndex = 0;

                IsValid = true;
            }
            else
            {
                match = PROPERTY_QUERY.Match(value);

                if (match.Success)
                {
                    _propName = match.Groups[1].Value;
                    _operator = "==";
                    _propValue = match.Groups[2].Value;
                    
                    IsValid = true;
                }
            }
        }
    }
}