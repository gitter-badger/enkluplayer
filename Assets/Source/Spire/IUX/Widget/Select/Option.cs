namespace CreateAR.SpirePlayer.IUX
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
                    _labelProp = Schema.Get<string>("label");
                }

                return _labelProp.Value;
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
                    _valueProp = Schema.Get<string>("value");
                }

                return _valueProp.Value;
            }
        }

    }
}