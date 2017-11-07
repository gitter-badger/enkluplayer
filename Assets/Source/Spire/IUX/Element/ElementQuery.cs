using System.Text.RegularExpressions;

namespace CreateAR.SpirePlayer.UI
{
    /// <summary>
    /// Query object.
    /// </summary>
    internal class ElementQuery
    {
        /// <summary>
        /// Regex for name matches.
        /// </summary>
        private static readonly Regex NAME_QUERY = new Regex(@"(\w+)");

        /// <summary>
        /// Regex for property matches.
        /// </summary>
        private static readonly Regex PROPERTY_QUERY = new Regex(@"\((@|(@@))([\w]+)\s*(([<>]=?)|==)\s*([\w]+)\)((\[(\d)?:(\d)?\])|$)");

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
        /// True iff valid query.
        /// </summary>
        private bool _isValid;

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
            switch (_operator)
            {
                case "==":
                {
                    if (_propName == "id")
                    {
                        return element.Id == _propValue;
                    }

                    var prop = element.Schema.Get<string>(_propName);
                    return prop.Value == _propValue;
                }
            }

            return false;
        }

        /// <summary>
        /// Parses the query.
        /// </summary>
        /// <param name="value">Value of the query.</param>
        private void Parse(string value)
        {
            var match = NAME_QUERY.Match(value);
            if (match.Success)
            {
                _propName = "id";
                _operator = "==";
                _propValue = match.Groups[1].Value;
                _startIndex = 0;
                _endIndex = 0;

                _isValid = true;
            }
            else
            {
                match = PROPERTY_QUERY.Match(value);
                if (match.Success)
                {
                    _propName = match.Groups[3].Value;
                    _operator = match.Groups[4].Value;
                    _propValue = match.Groups[6].Value;

                    if (!int.TryParse(match.Groups[9].Value, out _startIndex))
                    {
                        _isValid = false;

                        return;
                    }

                    if (!int.TryParse(match.Groups[10].Value, out _endIndex))
                    {
                        _isValid = false;

                        return;
                    }

                    _isValid = true;
                }
            }
        }
    }
}