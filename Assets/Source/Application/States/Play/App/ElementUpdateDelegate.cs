using System;
using System.Collections.Generic;
using System.Linq;
using CreateAR.Commons.Unity.Async;
using CreateAR.Commons.Unity.Logging;
using CreateAR.SpirePlayer.IUX;
using UnityEngine;
using Void = CreateAR.Commons.Unity.Async.Void;

namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// Tracks requests to update elements.
    /// </summary>
    public class ElementUpdateDelegate : IElementUpdateDelegate
    {
        /// <summary>
        /// Transactions.
        /// </summary>
        private readonly IElementTxnManager _txns;

        /// <summary>
        /// Tracks scenes.
        /// </summary>
        private readonly IAppSceneManager _scenes;

        /// <summary>
        /// Element transactions currently tracked.
        /// </summary>
        private readonly Dictionary<Element, ElementTxn> _transactions = new Dictionary<Element, ElementTxn>();

        /// <summary>
        /// Backing variable for Active property.
        /// </summary>
        private string _active;

        /// <summary>
        /// Active scene.
        /// </summary>
        public string Active
        {
            get
            {
                if (string.IsNullOrEmpty(_active))
                {
                    _active = _scenes.All.FirstOrDefault();
                }

                return _active;
            }
            set
            {
                _active = value;
            }
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        public ElementUpdateDelegate(
            IElementTxnManager txns,
            IAppSceneManager scenes)
        {
            _txns = txns;
            _scenes = scenes;
        }

        /// <inheritdoc />
        public IAsyncToken<Element> Create(ElementData data)
        {
            return Create(data, "root");
        }

        /// <inheritdoc />
        public IAsyncToken<Element> Create(ElementData data, string parentId)
        {
            if (string.IsNullOrEmpty(Active))
            {
                return new AsyncToken<Element>(new Exception("Could not Create element: no active scene."));
            }

            return Async.Map(
                _txns.Request(new ElementTxn(Active).Create(parentId, data)),
                response => response.Elements[0]);
        }

        /// <inheritdoc />
        public IAsyncToken<Element> Destroy(Element element)
        {
            if (string.IsNullOrEmpty(Active))
            {
                return new AsyncToken<Element>(new Exception("Could not Destroy element: no active scene."));
            }

            return Async.Map(
                _txns.Request(new ElementTxn(Active).Delete(element.Id)),
                response => response.Elements[0]);
        }

        /// <inheritdoc />
        public IAsyncToken<Void> DestroyAll()
        {
            return new AsyncToken<Void>(new NotImplementedException());
        }

        /// <inheritdoc />
        public void Update(Element element, string key, string value)
        {
            if (string.IsNullOrEmpty(Active))
            {
                Log.Warning(this, "Could not Update element: no active scene.");
                return;
            }

            Txn(element).Update(element.Id, key, value);
        }

        /// <inheritdoc />
        public void Update(Element element, string key, int value)
        {
            if (string.IsNullOrEmpty(Active))
            {
                Log.Warning(this, "Could not Update element: no active scene.");
                return;
            }

            Txn(element).Update(element.Id, key, value);
        }

        /// <inheritdoc />
        public void Update(Element element, string key, float value)
        {
            if (string.IsNullOrEmpty(Active))
            {
                Log.Warning(this, "Could not Update element: no active scene.");
                return;
            }

            Txn(element).Update(element.Id, key, value);
        }

        /// <inheritdoc />
        public void Update(Element element, string key, bool value)
        {
            if (string.IsNullOrEmpty(Active))
            {
                Log.Warning(this, "Could not Update element: no active scene.");
                return;
            }

            Txn(element).Update(element.Id, key, value);
        }

        /// <inheritdoc />
        public void Update(Element element, string key, Vec3 value)
        {
            if (string.IsNullOrEmpty(Active))
            {
                Log.Warning(this, "Could not Update element: no active scene.");
                return;
            }

            Txn(element).Update(element.Id, key, value);
        }

        /// <inheritdoc />
        public void Update(Element element, string key, Col4 value)
        {
            if (string.IsNullOrEmpty(Active))
            {
                Log.Warning(this, "Could not Update element: no active scene.");
                return;
            }

            Txn(element).Update(element.Id, key, value);
        }

        /// <inheritdoc />
        public void FinalizeUpdate(Element element)
        {
            if (string.IsNullOrEmpty(Active))
            {
                Log.Warning(this, "Could not Finalize element: no active scene.");
                return;
            }
            
            var txn = Txn(element);
            if (0 != txn.Actions.Count)
            {
                _txns
                    .Request(txn)
                    .OnFailure(exception => Log.Error(this, "Could not authenticate request : {0}.", exception));
            }

            _transactions.Remove(element);
        }

        /// <summary>
        /// Reparents an element.
        /// </summary>
        /// <param name="element">The element to move.</param>
        /// <param name="parent">The new parent.</param>
        /// <returns></returns>
        public IAsyncToken<Element> Reparent(
            Element element,
            Element parent)
        {
            if (string.IsNullOrEmpty(Active))
            {
                return new AsyncToken<Element>(new Exception("Could not Reparent element: no active scene."));
            }

            Vec3 pos, rot, scale;
            TransformedTrs(element, parent, out pos, out rot, out scale);

            Log.Info(this,
                "Local position changes from {0} -> {1}.",
                element.Schema.Get<Vec3>("position").Value,
                pos);

            var txn = new ElementTxn(Active).Move(
                element.Id,
                parent.Id,
                pos, rot, scale);

            Log.Info(this, "Requesting reparent with txn:\n{0}", txn);

            return Async.Map(
                _txns.Request(txn),
                response => element);
        }

        /// <summary>
        /// Creates or retrieves an element txn.
        /// </summary>
        /// <param name="element">The element in question</param>
        /// <returns></returns>
        private ElementTxn Txn(Element element)
        {
            ElementTxn txn;
            if (!_transactions.TryGetValue(element, out txn))
            {
                txn = _transactions[element] = new ElementTxn(Active);
            }

            return txn;
        }

        /// <summary>
        /// Retrieves the transformed position, rotation, and scale after
        /// reparenting.
        /// </summary>
        /// <param name="element">The element that is to be moved.</param>
        /// <param name="parent">The new parent.</param>
        /// <param name="position">The transformed position.</param>
        /// <param name="rotation">The transformed rotation.</param>
        /// <param name="scale">The transformed scale.</param>
        /// <returns></returns>
        private void TransformedTrs(
            Element element,
            Element parent,
            out Vec3 position,
            out Vec3 rotation,
            out Vec3 scale)
        {
            var unityElement = NearestUnityElement(element);
            var unityParent = NearestUnityElement(parent);

            Log.Info(this, "Nearest Unity Element from element: {0}", unityElement);
            Log.Info(this, "Nearest Unity Element from new parent: {0}", unityParent);

            // trivial case
            if (unityParent == unityElement)
            {
                position = element.Schema.Get<Vec3>("position").Value;
                rotation = element.Schema.Get<Vec3>("rotation").Value;
                scale = element.Schema.Get<Vec3>("scale").Value;

                return;
            }

            // transform to new parent's local space
            var transform = unityElement.GameObject.transform;
            var parentTransform = unityParent.GameObject.transform;
            
            position = parentTransform.worldToLocalMatrix.MultiplyPoint3x4(transform.position).ToVec();
            rotation = (Quaternion.Inverse(parentTransform.rotation) * transform.rotation).eulerAngles.ToVec();

            var a = transform.lossyScale;
            var b = parentTransform.lossyScale;
            scale = new Vec3(
                a.x / b.x,
                a.y / b.y,
                a.z / b.z);
        }

        /// <summary>
        /// Traverses upward till a unity element is found. The initial element
        /// is also tested.
        /// </summary>
        /// <param name="element">The element.</param>
        /// <returns></returns>
        private IUnityElement NearestUnityElement(Element element)
        {
            while (null != element)
            {
                var unityElement = element as IUnityElement;
                if (null != unityElement)
                {
                    return unityElement;
                }

                element = element.Parent;
            }

            return null;
        }
    }
}