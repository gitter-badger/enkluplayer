using System;
using System.Collections.Generic;
using CreateAR.Commons.Unity.Logging;
using CreateAR.EnkluPlayer.IUX;
using Enklu.Orchid;

namespace CreateAR.EnkluPlayer.Scripting
{
    /// <summary>
    /// JS interface for multiplayer functionality relating to a specific element.
    /// </summary>
    public class MultiplayerContextJs
    {
        /// <summary>
        /// Caches js wrappers.
        /// </summary>
        private readonly IElementJsCache _elements;

        /// <summary>
        /// Controls multiplayer.
        /// </summary>
        private readonly IMultiplayerController _multiplayer;

        /// <summary>
        /// Application wide configuration.
        /// </summary>
        private readonly ApplicationConfig _config;

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
            IElementJsCache elements,
            ApplicationConfig config,
            Element element)
        {
            _elements = elements;
            _multiplayer = multiplayer;
            _config = config;
            _element = element;
        }

        public void autoToggle(string property, bool startingValue, int milliseconds)
        {
            _multiplayer.AutoToggle(_element.Id, property, startingValue, milliseconds);
        }

        public void own(IJsCallback cb)
        {
            _multiplayer.Own(_element.Id, success =>
            {
                try
                {
                    if (!success)
                    {
                        cb.Apply(this, "Could not own element");
                    }
                    else
                    {
                        cb.Apply(this);
                    }
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
        
        public ElementBuilderJs builder(ElementJs parent)
        {
            return new ElementBuilderJs(
                _multiplayer,
                _elements,
                _config.Network.Credentials.UserId,
                parent.id);
        }
    }
}