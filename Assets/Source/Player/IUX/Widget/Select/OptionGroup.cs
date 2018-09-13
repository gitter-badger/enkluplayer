using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace CreateAR.EnkluPlayer.IUX
{
    /// <summary>
    /// Basic element for OptionGroups.
    /// </summary>
    public class OptionGroup : Element
    {
        /// <summary>
        /// Props.
        /// </summary>
        private ElementSchemaProp<string> _labelProp;
        private ElementSchemaProp<string> _valueProp;

        /// <summary>
        /// List of options under this group.
        /// </summary>
        private readonly List<Option> _optionsList = new List<Option>();

        /// <summary>
        /// Public data structure.
        /// </summary>
        private readonly ReadOnlyCollection<Option> _publicOptions;
        
        /// <summary>
        /// True iff list of options is dirty.
        /// </summary>
        private bool _isDirty = true;
        
        /// <summary>
        /// Retrieves the label. This is a shortcut for Schema lookup.
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
        /// Retrieves the value. This is a shortcut for Schema lookup.
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

        /// <summary>
        /// Child options.
        /// </summary>
        public ReadOnlyCollection<Option> Options
        {
            get
            {
                if (_isDirty)
                {
                    UpdateOptions();
                }

                return _publicOptions;
            }
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        public OptionGroup()
        {
            _publicOptions = new ReadOnlyCollection<Option>(_optionsList);
        }

        /// <inheritdoc />
        protected override void AddChildInternal(Element element)
        {
            base.AddChildInternal(element);

            _isDirty = true;
        }

        /// <inheritdoc />
        protected override void RemoveChildInternal(Element element)
        {
            base.RemoveChildInternal(element);

            _isDirty = true;
        }

        /// <summary>
        /// Finds child Option instances.
        /// </summary>
        private void UpdateOptions()
        {
            _optionsList.Clear();
            
            for (int i = 0, len = Children.Count; i < len; i++)
            {
                var option = Children[i] as Option;
                if (null != option)
                {
                    _optionsList.Add(option);
                }
            }
        }
    }
}