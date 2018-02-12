﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using CreateAR.Commons.Unity.Async;
using CreateAR.Commons.Unity.Logging;
using CreateAR.SpirePlayer.IUX;
using UnityEngine;
using Void = CreateAR.Commons.Unity.Async.Void;

namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// Manages a set of prop data + the corresponding controllers.
    /// </summary>
    public class PropSet
    {
        /// <summary>
        /// Creates elements.
        /// </summary>
        private readonly IElementFactory _elements;
        
        /// <summary>
        /// Receives update events.
        /// </summary>
        private readonly IPropUpdateDelegate _propDelegate;

        /// <summary>
        /// Received propset update events.
        /// </summary>
        private readonly IPropSetUpdateDelegate _propSetDelegate;

        /// <summary>
        /// Backing property for Props.
        /// </summary>
        private readonly List<PropController> _props = new List<PropController>();

        /// <summary>
        /// The unique id of this propset.
        /// </summary>
        public string Id { get; private set; }

        /// <summary>
        /// All props.
        /// </summary>
        public ReadOnlyCollection<PropController> Props { get; private set; }

        /// <summary>
        /// Constructor.
        /// </summary>
        public PropSet(
            IElementFactory elements,
            IPropUpdateDelegate propDelegate,
            IPropSetUpdateDelegate propSetDelegate,
            string id,
            ElementData[] data)
        {
            _elements = elements;
            _propDelegate = propDelegate;
            _propSetDelegate = propSetDelegate;

            Id = id;
            Props = new ReadOnlyCollection<PropController>(_props);

            // create controllers
            for (var i = 0; i < data.Length; i++)
            {
                var propData = data[i];
                var controller = CreateInternal(propData);
                if (null == controller)
                {
                    Log.Warning(this,
                        "Could not create PropController from ElementData {0}.",
                        propData);
                    continue;
                }
                
                _props.Add(controller);
            }
        }

        /// <summary>
        /// Creates a PropController from a PropData. The PropData is expected
        /// to have a valid Content Id.
        /// </summary>
        /// <param name="data">The propdata.</param>
        /// <returns></returns>
        public IAsyncToken<PropController> Create(ElementData data)
        {
            PropController controller = null;
            
            return Async.Map(
                _propSetDelegate
                    .Add(this, data)
                    .OnSuccess(_ =>
                    {
                        controller = CreateInternal(data);

                        _props.Add(controller);
                    }),
                _ => controller);
        }
        
        /// <summary>
        /// Destroys a Prop by id.
        /// </summary>
        /// <param name="id">The id of the prop.</param>
        /// <returns></returns>
        public IAsyncToken<Void> Destroy(string id)
        {
            var prop = ById(id);
            if (null == prop)
            {
                return new AsyncToken<Void>(new Exception("Could not find prop by id."));
            }

            return _propSetDelegate
                .Remove(this, prop.Element)
                .OnSuccess(_ =>
                {
                    _props.Remove(prop);

                    DestroyInternal(prop);
                });
        }

        /// <summary>
        /// Destroys all props.
        /// </summary>
        /// <returns></returns>
        public IAsyncToken<Void> DestroyAll()
        {
            return Async.Map(
                Async.All(
                    _props
                        .ToArray()
                        .Select(prop => Destroy(prop.Element.Id))
                        .ToArray()),
                _ => Void.Instance);
        }

        /// <summary>
        /// Internal Create method which creates a <c>ContentWidget</c> as well.
        /// </summary>
        /// <param name="data">The data.</param>
        /// <returns></returns>
        private PropController CreateInternal(ElementData data)
        {
            var element = (ContentWidget) _elements.Element(new ElementDescription
            {
                Root = new ElementRef
                {
                    Id = data.Id
                },
                Elements = new[]
                {
                    data
                }
            });

            /*var content = (ContentWidget) _elements.Element(string.Format(
                @"<Content assetSrc='{0}' />",
                data.AssetId));*/
            
            var controller = element.GameObject.AddComponent<PropController>();
            controller.Initialize(element,  _propDelegate);

            return controller;
        }

        /// <summary>
        /// Safely destroys a prop.
        /// </summary>
        /// <param name="prop"></param>
        private void DestroyInternal(PropController prop)
        {
            var content = prop.Element;

            prop.Uninitialize();

            // destroy content after prop, as GameObject is destroyed
            content.Destroy();
        }

        /// <summary>
        /// Retrieves a <c>PropController</c> by id.
        /// </summary>
        /// <param name="id">The unique id of the content.</param>
        /// <returns></returns>
        private PropController ById(string id)
        {
            for (var i = 0; i < _props.Count; i++)
            {
                var controller = _props[i];
                if (controller.Element.Id == id)
                {
                    return controller;
                }
            }

            return null;
        }
    }
}