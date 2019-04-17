using System;
using System.Collections;
using CreateAR.Commons.Unity.Async;
using CreateAR.Commons.Unity.Http;
using CreateAR.Commons.Unity.Logging;
using CreateAR.EnkluPlayer.IUX;
using Enklu.Data;
using Enklu.Mycelium.Messages.Experience;
using UnityEngine;
using Void = CreateAR.Commons.Unity.Async.Void;

namespace CreateAR.EnkluPlayer
{
    /// <summary>
    /// <c>IMultiplayerController</c> implementation for the web editor.
    /// </summary>
    public class WebMultiplayerController : IMultiplayerController
    {
        /// <summary>
        /// Dependencies.
        /// </summary>
        private readonly IElementManager _elements;
        private readonly IElementFactory _elementFactory;
        private readonly IBootstrapper _bootstrapper;

        /// <inheritdoc />
        public bool IsConnected { get { return false; } }

        /// <inheritdoc />
        public event Action<bool> OnConnectionChanged;

        /// <summary>
        /// Constructor.
        /// </summary>
        public WebMultiplayerController(
            IElementManager elements,
            IElementFactory elementFactory,
            IBootstrapper bootstrapper)
        {
            _elements = elements;
            _elementFactory = elementFactory;
            _bootstrapper = bootstrapper;
        }
        
        /// <inheritdoc />
        public IAsyncToken<Void> Initialize()
        {
            return new AsyncToken<Void>(Void.Instance);
        }

        /// <inheritdoc />
        public void ApplyDiff(IAppDataLoader appData)
        {
            //             
        }

        /// <inheritdoc />
        public void Play()
        {
            // 
        }

        /// <inheritdoc />
        public void Disconnect()
        {
            if (null != OnConnectionChanged)
            {
                OnConnectionChanged(false);
            }
        }

        public IAsyncToken<Element> Create(
            string parentId,
            ElementData element,
            string owner = null,
            ElementExpirationType expiration = ElementExpirationType.Session)
        {
            var parent = _elements.ById(parentId);
            if (null == parentId)
            {
                return new AsyncToken<Element>(new Exception("Could not find parent."));
            }

            Element newElement;
            try
            {
                newElement = _elementFactory.Element(new ElementDescription
                {
                    Elements = new[] {element},
                    Root = new ElementRef
                    {
                        Id = element.Id
                    }
                });

                parent.AddChild(newElement);
            }
            catch (Exception exception)
            {
                return new AsyncToken<Element>(exception);
            }

            return new AsyncToken<Element>(newElement);
        }

        public IAsyncToken<Void> Destroy(string id)
        {
            // find and destroy
            var element = _elements.ById(id);
            if (null == element)
            {
                return new AsyncToken<Void>(new Exception("Could not find element by id."));
            }

            element.Destroy();

            return new AsyncToken<Void>(Void.Instance);
        }

        /// <inheritdoc />
        public void AutoToggle(string elementId, string prop, bool value, int milliseconds)
        {
            // find element
            var element = _elements.ById(elementId);
            if (null == element)
            {
                Log.Warning(this, "Could not find element by id {0} to autotoggle.", elementId);
                return;
            }

            // set prop
            element.Schema.Set(prop, value);

            // set timer to reset
            _bootstrapper.BootstrapCoroutine(Wait(
                milliseconds / 1000f,
                () => element.Schema.Set(prop, !value)));
        }

        /// <inheritdoc />
        public void Sync(string elementId, ElementSchemaProp prop)
        {
            // 
        }

        /// <inheritdoc />
        public void UnSync(string elementId, ElementSchemaProp prop)
        {
            // 
        }

        /// <inheritdoc />
        public void Own(string elementId, Action<bool> callback)
        {
            callback(true);
        }

        /// <inheritdoc />
        public void Forfeit(string elementId)
        {
            // 
        }

        /// <summary>
        /// Waits for a time before executing the action on the main thread.
        /// </summary>
        /// <param name="seconds">The number of seconds to wait.</param>
        /// <param name="action">The action to call.</param>
        /// <returns></returns>
        private IEnumerator Wait(float seconds, Action action)
        {
            yield return new WaitForSecondsRealtime(seconds);

            action();
        }
    }
}