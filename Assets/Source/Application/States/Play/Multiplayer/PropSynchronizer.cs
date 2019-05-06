using System;
using System.Collections.Generic;
using CreateAR.Commons.Unity.Logging;
using CreateAR.EnkluPlayer.IUX;
using Enklu.Data;
using Enklu.Mycelium.Messages;

namespace CreateAR.EnkluPlayer
{
    /// <summary>
    /// Object that sends prop updates to the network.
    /// </summary>
    public class PropSynchronizer
    {
        /// <summary>
        /// Information about the prop.
        /// </summary>
        private class PropInfo
        {
            /// <summary>
            /// Hash of the element.
            /// </summary>
            public ushort ElementHash;

            /// <summary>
            /// Hash of the prop.
            /// </summary>
            public ushort PropHash;
        }

        /// <summary>
        /// Function that sends messages over the network.
        /// </summary>
        private readonly Action<object> _sender;

        /// <summary>
        /// Lookup of prop info we are aware of.
        /// </summary>
        private readonly Dictionary<ElementSchemaProp, PropInfo> _props = new Dictionary<ElementSchemaProp, PropInfo>();

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="sender">Function that can send messages.</param>
        public PropSynchronizer(Action<object> sender)
        {
            _sender = sender;
        }

        /// <summary>
        /// Tracks changes to an element.
        /// </summary>
        /// <param name="elementHash">The hash of the element.</param>
        /// <param name="propHash">The hash of the prop.</param>
        /// <param name="prop">The prop.</param>
        public void Track(ushort elementHash, ushort propHash, ElementSchemaProp prop)
        {
            // save prop info
            _props[prop] = new PropInfo
            {
                ElementHash = elementHash,
                PropHash = propHash
            };

            var stringProp = prop as ElementSchemaProp<string>;
            if (null != stringProp)
            {
                stringProp.OnChanged -= Prop_OnChanged;
                stringProp.OnChanged += Prop_OnChanged;
                return;
            }

            var boolProp = prop as ElementSchemaProp<bool>;
            if (null != boolProp)
            {
                boolProp.OnChanged -= Prop_OnChanged;
                boolProp.OnChanged += Prop_OnChanged;
                return;
            }

            var intProp = prop as ElementSchemaProp<int>;
            if (null != intProp)
            {
                intProp.OnChanged -= Prop_OnChanged;
                intProp.OnChanged += Prop_OnChanged;
                return;
            }

            var floatProp = prop as ElementSchemaProp<float>;
            if (null != floatProp)
            {
                floatProp.OnChanged -= Prop_OnChanged;
                floatProp.OnChanged += Prop_OnChanged;
                return;
            }

            var vecProp = prop as ElementSchemaProp<Vec3>;
            if (null != vecProp)
            {
                vecProp.OnChanged -= Prop_OnChanged;
                vecProp.OnChanged += Prop_OnChanged;
                return;
            }

            var colProp = prop as ElementSchemaProp<Col4>;
            if (null != colProp)
            {
                colProp.OnChanged -= Prop_OnChanged;
                colProp.OnChanged += Prop_OnChanged;
            }
        }
        
        /// <summary>
        /// Stops sending changes to a prop over the network.
        /// </summary>
        /// <param name="prop">The prop.</param>
        public void Untrack(ElementSchemaProp prop)
        {
            // delete prop info
            _props.Remove(prop);

            var stringProp = prop as ElementSchemaProp<string>;
            if (null != stringProp)
            {
                stringProp.OnChanged -= Prop_OnChanged;
                return;
            }

            var boolProp = prop as ElementSchemaProp<bool>;
            if (null != boolProp)
            {
                boolProp.OnChanged -= Prop_OnChanged;
                return;
            }

            var intProp = prop as ElementSchemaProp<int>;
            if (null != intProp)
            {
                intProp.OnChanged -= Prop_OnChanged;
                return;
            }

            var floatProp = prop as ElementSchemaProp<float>;
            if (null != floatProp)
            {
                floatProp.OnChanged -= Prop_OnChanged;
                return;
            }

            var vecProp = prop as ElementSchemaProp<Vec3>;
            if (null != vecProp)
            {
                vecProp.OnChanged -= Prop_OnChanged;
                return;
            }

            var colProp = prop as ElementSchemaProp<Col4>;
            if (null != colProp)
            {
                colProp.OnChanged -= Prop_OnChanged;
            }
        }

        /// <summary>
        /// Called when prop is updated.
        /// </summary>
        /// <param name="prop">The prop.</param>
        /// <param name="prev">Previous value of the prop.</param>
        /// <param name="next">Next value of the prop.</param>
        private void Prop_OnChanged(ElementSchemaProp<string> prop, string prev, string next)
        {
            PropInfo info;
            if (!_props.TryGetValue(prop, out info))
            {
                Log.Warning(this, "Received a prop changed event for an untracked prop: {0}.", prop.Name);
                return;
            }

            _sender(new UpdateElementStringEvent
            {
                ElementHash = info.ElementHash,
                PropHash = info.PropHash,
                Value = next
            });
        }

        /// <summary>
        /// Called when prop is updated.
        /// </summary>
        /// <param name="prop">The prop.</param>
        /// <param name="prev">Previous value of the prop.</param>
        /// <param name="next">Next value of the prop.</param>
        private void Prop_OnChanged(ElementSchemaProp<bool> prop, bool prev, bool next)
        {
            PropInfo info;
            if (!_props.TryGetValue(prop, out info))
            {
                Log.Warning(this, "Received a prop changed event for an untracked prop: {0}.", prop.Name);
                return;
            }

            _sender(new UpdateElementBoolEvent
            {
                ElementHash = info.ElementHash,
                PropHash = info.PropHash,
                Value = next
            });
        }

        /// <summary>
        /// Called when prop is updated.
        /// </summary>
        /// <param name="prop">The prop.</param>
        /// <param name="prev">Previous value of the prop.</param>
        /// <param name="next">Next value of the prop.</param>
        private void Prop_OnChanged(ElementSchemaProp<int> prop, int prev, int next)
        {
            PropInfo info;
            if (!_props.TryGetValue(prop, out info))
            {
                Log.Warning(this, "Received a prop changed event for an untracked prop: {0}.", prop.Name);
                return;
            }

            _sender(new UpdateElementIntEvent
            {
                ElementHash = info.ElementHash,
                PropHash = info.PropHash,
                Value = next
            });
        }

        /// <summary>
        /// Called when prop is updated.
        /// </summary>
        /// <param name="prop">The prop.</param>
        /// <param name="prev">Previous value of the prop.</param>
        /// <param name="next">Next value of the prop.</param>
        private void Prop_OnChanged(ElementSchemaProp<float> prop, float prev, float next)
        {
            PropInfo info;
            if (!_props.TryGetValue(prop, out info))
            {
                Log.Warning(this, "Received a prop changed event for an untracked prop: {0}.", prop.Name);
                return;
            }

            _sender(new UpdateElementFloatEvent
            {
                ElementHash = info.ElementHash,
                PropHash = info.PropHash,
                Value = next
            });
        }

        /// <summary>
        /// Called when prop is updated.
        /// </summary>
        /// <param name="prop">The prop.</param>
        /// <param name="prev">Previous value of the prop.</param>
        /// <param name="next">Next value of the prop.</param>
        private void Prop_OnChanged(ElementSchemaProp<Vec3> prop, Vec3 prev, Vec3 next)
        {
            PropInfo info;
            if (!_props.TryGetValue(prop, out info))
            {
                Log.Warning(this, "Received a prop changed event for an untracked prop: {0}.", prop.Name);
                return;
            }

            _sender(new UpdateElementVec3Event
            {
                ElementHash = info.ElementHash,
                PropHash = info.PropHash,
                Value = next
            });
        }

        /// <summary>
        /// Called when prop is updated.
        /// </summary>
        /// <param name="prop">The prop.</param>
        /// <param name="prev">Previous value of the prop.</param>
        /// <param name="next">Next value of the prop.</param>
        private void Prop_OnChanged(ElementSchemaProp<Col4> prop, Col4 prev, Col4 next)
        {
            PropInfo info;
            if (!_props.TryGetValue(prop, out info))
            {
                Log.Warning(this, "Received a prop changed event for an untracked prop: {0}.", prop.Name);
                return;
            }

            _sender(new UpdateElementCol4Event
            {
                ElementHash = info.ElementHash,
                PropHash = info.PropHash,
                Value = next
            });
        }
    }
}