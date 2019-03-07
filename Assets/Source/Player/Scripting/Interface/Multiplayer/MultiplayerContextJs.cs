using System;
using System.Collections.Generic;
using CreateAR.Commons.Unity.Logging;
using CreateAR.EnkluPlayer.IUX;
using Jint;
using Jint.Native;

namespace CreateAR.EnkluPlayer.Scripting
{
    /// <summary>
    /// JS interface for multiplayer functionality relating to a specific element.
    /// </summary>
    public class MultiplayerContextJs
    {
        /// <summary>
        /// Controls multiplayer.
        /// </summary>
        private readonly IMultiplayerController _multiplayer;

        /// <summary>
        /// The associated element.
        /// </summary>
        private readonly Element _element;

        /// <summary>
        /// List of props that we are synchronizing.
        /// </summary>
        private readonly List<ElementSchemaProp> _synchronizedProps = new List<ElementSchemaProp>();

        /// <summary>
        /// Constructor.
        /// </summary>
        public MultiplayerContextJs(
            IMultiplayerController multiplayer,
            Element element)
        {
            _multiplayer = multiplayer;
            _element = element;
        }

        public void autoToggle(string property, bool startingValue, int milliseconds)
        {
            _multiplayer.AutoToggle(_element.Id, property, startingValue, milliseconds);
        }

        public void own(Engine engine, Func<JsValue, JsValue[], JsValue> cb)
        {
            _multiplayer.Own(_element.Id, success =>
            {
                var parameters = new JsValue[0];
                if (!success)
                {
                    parameters = new [] { new JsValue("Could not own element.") };
                }

                try
                {
                    cb(
                        JsValue.FromObject(engine, this),
                        parameters);
                }
                catch (Exception exception)
                {
                    Log.Error(this, "Exception thrown when calling own js callback: {0}.", exception);
                }
            });
        }

        public void sync(string name)
        {
            var props = _element.Schema.GetOwnProps();
            for (int i = 0, len = props.Count; i < len; i++)
            {
                var prop = props[i];
                if (prop.Name == name)
                {
                    if (_synchronizedProps.Contains(prop))
                    {
                        // we may already be synchronizing this prop
                        return;
                    }

                    // keep track of what we are synchronizing
                    _synchronizedProps.Add(prop);

                    _multiplayer.Sync(prop);

                    return;
                }
            }

            Log.Warning(this, "Could not synchronize prop '{0}' because the prop doesn't exist.", name);
        }

        public void unsync(string name)
        {
            for (int i = 0, len = _synchronizedProps.Count; i < len; i++)
            {
                var prop = _synchronizedProps[i];
                if (prop.Name == name)
                {
                    // remove fromm the list
                    _synchronizedProps.RemoveAt(i);

                    _multiplayer.UnSync(prop);

                    return;
                }
            }

            Log.Warning(this, "Could not unsynchronize prop '{0}' because we were not synchronizing it.", name);
        }
    }
}