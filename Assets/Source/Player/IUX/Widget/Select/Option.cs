namespace CreateAR.EnkluPlayer.IUX
{
    /// <summary>
    /// Basic element for Options.
    /// </summary>
    public class Option : Element
    {
        /// <summary>
        /// Props.
        /// </summary>
        private ElementSchemaProp<string> _labelProp;
        private ElementSchemaProp<string> _valueProp;

        /// <summary>
        /// Retrieves the label for this option. Shortcut for Schema lookup.
        /// </summary>
        public string Label
        {
            get
            {
                if (null == _labelProp)
                {
                    _labelProp = Schema.GetOwn("label", "New Option");
                }

                return _labelProp.Value;
            }
            set
            {
                if (null == _labelProp)
                {
                    _labelProp = Schema.GetOwn("label", "New Option");
                }

                _labelProp.Value = value;
            }
        }

        /// <summary>
        /// Retrieves the value for this option. Shortcut for Schema lookup.
        /// </summary>
        public string Value
        {
            get
            {
                if (null == _valueProp)
                {
                    _valueProp = Schema.GetOwn("value", "value");
                }

                return _valueProp.Value;
            }

            set
            {
                if (null == _valueProp)
                {
                    _valueProp = Schema.GetOwn("value", "value");
                }

                _valueProp.Value = value;
            }
        }

    }
}