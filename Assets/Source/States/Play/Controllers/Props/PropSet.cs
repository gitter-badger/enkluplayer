using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using CreateAR.Commons.Unity.Async;
using CreateAR.Commons.Unity.Logging;
using CreateAR.SpirePlayer.IUX;
using Void = CreateAR.Commons.Unity.Async.Void;

namespace CreateAR.SpirePlayer
{
    public class PropSet
    {
        private readonly IElementFactory _elements;
        private readonly IPropUpdateDelegate _propDelegate;
        private readonly IPropSetUpdateDelegate _propSetDelegate;
        private readonly List<PropController> _props = new List<PropController>();

        public string Id { get; private set; }

        public ReadOnlyCollection<PropController> Props { get; private set; }

        public PropSet(
            IElementFactory elements,
            IPropUpdateDelegate propDelegate,
            IPropSetUpdateDelegate propSetDelegate,
            PropData[] data)
        {
            _elements = elements;
            _propDelegate = propDelegate;
            _propSetDelegate = propSetDelegate;

            Props = new ReadOnlyCollection<PropController>(_props);

            // create controllers
            for (var i = 0; i < data.Length; i++)
            {
                var propData = data[i];
                var controller = CreateInternal(propData);
                if (null == controller)
                {
                    Log.Warning(this,
                        "Could not create PropController from PropData {0}.",
                        propData);
                    continue;
                }
                
                _props.Add(controller);
            }
        }

        public IAsyncToken<PropController> Create(PropData data)
        {
            if (_propSetDelegate.Add(data))
            {
                var controller = CreateInternal(data);
                if (null == controller)
                {
                    return new AsyncToken<PropController>(new Exception(string.Format(
                        "Could not create controller from data {0}.",
                        data)));
                }

                return Async.Map(
                    _propSetDelegate.Save(),
                    _ => controller);
            }

            return new AsyncToken<PropController>(new Exception("Could not add new prop data."));
        }

        public IAsyncToken<Void> Destroy(string id)
        {
            var prop = ById(id);
            if (null == prop)
            {
                return new AsyncToken<Void>(new Exception("Could not find prop by id."));
            }

            if (_propSetDelegate.Remove(prop.Data))
            {
                _props.Remove(prop);

                DestroyInternal(prop);

                return _propSetDelegate.Save();
            }

            return new AsyncToken<Void>(new Exception("Found prop but delegate could not remove it."));
        }

        private PropController CreateInternal(PropData data)
        {
            var content = (ContentWidget) _elements.Element(string.Format(
                @"<Content src='{0}' />",
                data.ContentId));
            if (null == content)
            {
                Log.Error(this, "Could not create content.");

                return null;
            }

            var controller = content.GameObject.AddComponent<PropController>();
            controller.Initialize(data, content, _propDelegate);

            return controller;
        }

        private void DestroyInternal(PropController prop)
        {
            var content = prop.Content;

            prop.Uninitialize();

            // destroy content after prop, as GameObject is destroyed
            content.Destroy();
        }

        private PropController ById(string id)
        {
            for (var i = 0; i < _props.Count; i++)
            {
                var controller = _props[i];
                if (controller.Data.Id == id)
                {
                    return controller;
                }
            }

            return null;
        }
    }
}