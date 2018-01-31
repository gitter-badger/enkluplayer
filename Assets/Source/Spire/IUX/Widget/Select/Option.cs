using System.Collections.Generic;

namespace CreateAR.SpirePlayer.IUX
{
    /// <summary>
    /// Basic element for Options.
    /// </summary>
    public class Option : Element
    {
        private ElementSchemaProp<string> _labelProp;
        private ElementSchemaProp<string> _valueProp;

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

    /// <summary>
    /// Basic element for OptionGroups.
    /// </summary>
    public class OptionGroup : Element
    {
        private ElementSchemaProp<string> _labelProp;
        private ElementSchemaProp<string> _valueProp;

        private readonly List<Option> _optionsList = new List<Option>();
        private Option[] _options;

        private bool _isDirty = true;
        
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

        public Option[] Options
        {
            get
            {
                if (_isDirty)
                {
                    UpdateOptions();
                }

                return _options;
            }
        }

        protected override void AddChildInternal(Element element)
        {
            base.AddChildInternal(element);

            _isDirty = true;
        }

        protected override void RemoveChildInternal(Element element)
        {
            base.RemoveChildInternal(element);

            _isDirty = true;
        }
        
        private void UpdateOptions()
        {
            _optionsList.Clear();

            var children = Children;
            for (int i = 0, len = children.Length; i < len; i++)
            {
                var option = children[i] as Option;
                if (null != option)
                {
                    _optionsList.Add(option);
                }
            }

            _options = _optionsList.ToArray();
        }
    }
}