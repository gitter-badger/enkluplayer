using System;
using System.Collections.Generic;
using System.Linq;
using CreateAR.Commons.Unity.Async;
using CreateAR.Commons.Unity.Logging;
using CreateAR.SpirePlayer.IUX;
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
        /// Element transactions currently tracked.
        /// </summary>
        private readonly Dictionary<Element, ElementTxn> _transactions = new Dictionary<Element, ElementTxn>();

        /// <summary>
        /// Active scene.
        /// </summary>
        public string Active { get; set; }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="txns">Transactions.</param>
        public ElementUpdateDelegate(IElementTxnManager txns)
        {
            _txns = txns;
            _txns.OnSceneAfterTracked += Txns_OnSceneTracked;
            _txns.OnSceneBeforeUntracked += Txns_OnSceneUntracked;
        }

        /// <inheritdoc />
        public IAsyncToken<Element> Create(ElementData data)
        {
            if (string.IsNullOrEmpty(Active))
            {
                return new AsyncToken<Element>(new Exception("Could not Create element: no active scene."));
            }

            return Async.Map(
                _txns.Request(new ElementTxn(Active).Create("root", data)),
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
                _txns.Request(txn);
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

            return Async.Map(
                _txns.Request(new ElementTxn(Active).Move(
                    element.Id,
                    parent.Id,
                    TransformedPosition(element, parent))),
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
        /// Retrieves a transformed position from one element to another.
        /// </summary>
        /// <param name="element">The element that is to be moved.</param>
        /// <param name="parent">The new parent.</param>
        /// <returns></returns>
        private Vec3 TransformedPosition(Element element, Element parent)
        {
            var unityElement = NearestUnityElement(element);
            var unityParent = NearestUnityElement(parent);

            // trivial case
            if (unityParent == unityElement)
            {
                return element.Schema.Get<Vec3>("position").Value;
            }

            // transform to new parent's local space
            var pos = unityElement.GameObject.transform.position;
            return unityParent
                .GameObject
                .transform
                .worldToLocalMatrix
                .MultiplyPoint3x4(pos)
                .ToVec();
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

        /// <summary>
        /// Called when a scene is untracked.
        /// </summary>
        /// <param name="id">Scene id.</param>
        private void Txns_OnSceneUntracked(string id)
        {
            if (Active == id)
            {
                Active = _txns.TrackedScenes.FirstOrDefault();
            }
        }

        /// <summary>
        /// Called when a scene is tracked.
        /// </summary>
        /// <param name="id">Scene id.</param>
        private void Txns_OnSceneTracked(string id)
        {
            Log.Info(this, "Scene tracked.");
            if (null == Active)
            {
                Log.Info(this, "Setting active");
                Active = id;
            }
        }
    }
}