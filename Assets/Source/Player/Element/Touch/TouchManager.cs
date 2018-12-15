using System.Collections.Generic;
using CreateAR.Commons.Unity.Logging;
using CreateAR.EnkluPlayer.IUX;
using UnityEngine;

namespace CreateAR.EnkluPlayer
{
    /// <summary>
    /// Basic implementation of <c>ITouchManager</c> that supports multiple
    /// hands.
    /// </summary>
    public class TouchManager : ITouchManager
    {
        /// <summary>
        /// Class for internal record-keeping.
        /// </summary>
        private class TouchRecord
        {
            /// <summary>
            /// Element.
            /// </summary>
            public Element Element { get; private set; }

            /// <summary>
            /// The object to push events to.
            /// </summary>
            public ITouchDelegate Delegate { get; private set; }
            
            /// <summary>
            /// True iff the object is currently hit by an intersecting ray.
            /// </summary>
            public bool IsHit { get; private set; }

            /// <summary>
            /// The id of the hit pointer.
            /// </summary>
            public uint HitPointer { get; private set; }

            /// <summary>
            /// The collider.
            /// </summary>
            public Collider Collider { get; set; }

            /// <summary>
            /// Constructor.
            /// </summary>
            public TouchRecord(Element element, ITouchDelegate @delegate)
            {
                Element = element;
                Delegate = @delegate;
            }

            /// <summary>
            /// Attempts to start a hit with a pointer id. Only one pointer can
            /// own this at a time.
            /// </summary>
            /// <param name="pointerId">The id of the pointer.</param>
            /// <returns></returns>
            public bool Start(uint pointerId)
            {
                // already owned by another pointer
                if (0 != HitPointer)
                {
                    return false;
                }

                HitPointer = pointerId;
                IsHit = true;

                return true;
            }

            /// <summary>
            /// Attempts to stop a hit with a pointer id. Only the pointer that
            /// successfully hit the element may stop the hit.
            /// </summary>
            /// <param name="pointerId">The id of the pointer.</param>
            /// <returns></returns>
            public bool Stop(uint pointerId)
            {
                // only allow owning pointer to stop
                if (HitPointer == pointerId)
                {
                    IsHit = false;
                    HitPointer = 0;

                    return true;
                }

                return false;
            }
        }

        /// <summary>
        /// All the objects currently watched.
        /// </summary>
        private readonly List<TouchRecord> _records = new List<TouchRecord>();

        /// <summary>
        /// A queue of objects that will be removed at the beginning of next frame.
        /// </summary>
        private readonly List<TouchRecord> _removeQueue = new List<TouchRecord>();

        /// <summary>
        /// Gesture API.
        /// </summary>
        private readonly IGestureManager _gestures;

        /// <summary>
        /// Instead of using Camera.main (which may be the scene camera or null),
        /// use the correct one.
        /// </summary>
        private readonly MainCamera _camera;

        /// <summary>
        /// All of the pointer ids that we are watching for gestures.
        /// </summary>
        private readonly List<uint> _pointerIds = new List<uint>();

        /// <summary>
        /// A queue of pointers that have been removed since last frame. This
        /// lets the system handle stop events.
        /// </summary>
        private readonly List<uint> _removedPointerIdQueue = new List<uint>();

        /// <inheritdoc />
        public Vec2 FingerOffset { get; set; }

        /// <summary>
        /// Constructor.
        /// </summary>
        public TouchManager(
            IGestureManager gestures,
            MainCamera camera)
        {
            _gestures = gestures;
            _camera = camera;

            _gestures.OnPointerStarted += Gestures_OnPointerStarted;
            _gestures.OnPointerEnded += Gestures_OnPointerEnded;

            FingerOffset = new Vec2(0, 0.1f);
        }
        
        /// <inheritdoc />
        public bool Register(Element element, ITouchDelegate @delegate)
        {
            var record = RecordForElement(element);
            if (null != record)
            {
                Log.Warning(this, "Element is already registered.");
                return false;
            }

            // content is all that is supported atm
            var content = element as ContentWidget;
            if (null == content)
            {
                Log.Warning(this, "Unsupported element type.");
                return false;
            }

            // add first
            _records.Add(new TouchRecord(element, @delegate));
            
            SetupElement(element);
            SetupContent(content);

            return true;
        }

        /// <inheritdoc />
        public bool Unregister(Element element)
        {
            var record = RecordForElement(element);
            if (null == record)
            {
                Log.Warning(this, "Could not unregistered an element that has not been registered.");
                return false;
            }

            // content is all the is supported atm
            var content = element as ContentWidget;
            if (null != content)
            {
                TeardownContent(content);
            }

            TeardownElement(element);
            
            _removeQueue.Add(record);

            return true;
        }

        /// <inheritdoc />
        public void Update()
        {
            // remove queued removes
            var removeLen = _removeQueue.Count;
            if (removeLen > 0)
            {
                for (var i = 0; i < removeLen; i++)
                {
                    _records.Remove(_removeQueue[i]);
                }

                _removeQueue.Clear();
            }

            // remove queued pointers
            var removePointerLen = _removedPointerIdQueue.Count;
            if (removePointerLen > 0)
            {
                for (var i = 0; i < removePointerLen; i++)
                {
                    var pointer = _removedPointerIdQueue[i];
                    _pointerIds.Remove(pointer);

                    // stop all
                    for (int j = 0, jlen = _records.Count; j < jlen; j++)
                    {
                        var record = _records[j];
                        if (record.Stop(pointer))
                        {
                            record.Delegate.TouchStopped(record.Element);
                        }
                    }
                }

                _removedPointerIdQueue.Clear();
            }
            
            // detect for all active pointers
            for (int i = 0, len = _pointerIds.Count; i < len; i++)
            {
                DetectTouches(_pointerIds[i]);
            }
        }

        /// <summary>
        /// Detects touches for a specific pointer id.
        /// </summary>
        /// <param name="pointerId">The pointer id.</param>
        private void DetectTouches(uint pointerId)
        {
            Vector3 position;
            if (!_gestures.TryGetPointerOrigin(pointerId, out position))
            {
                return;
            }

            var cameraUp = _camera.transform.up;
            var cameraRight = _camera.transform.right;
            position += FingerOffset.x * cameraRight + FingerOffset.y * cameraUp;

            var cameraPosition = _camera.transform.position;
            var v = position - cameraPosition;
            var s = v.magnitude;
            var ray = new Ray(position, v.normalized);
            
            for (int i = 0, len = _records.Count; i < len; i++)
            {
                var record = _records[i];
                var collider = record.Collider;
                if (null == collider)
                {
                    continue;
                }
                
                RaycastHit hit;
                var isHit = collider.Raycast(ray, out hit, s);
                if (isHit)
                {
                    if (record.IsHit)
                    {
                        record.Delegate.TouchDragged(record.Element, hit.point, hit.normal);
                    }
                    else if (record.Start(pointerId))
                    {
                        record.Delegate.TouchStarted(record.Element, hit.point, hit.normal);
                    }
                }
                else if (record.IsHit && record.Stop(pointerId))
                {
                    record.Delegate.TouchStopped(record.Element);
                }
            }

            var handle = Render.Handle("Touch");
            if (null != handle)
            {
                handle.Draw(ctx =>
                {
                    ctx.Color(Color.green);
                    ctx.Cube(position, 0.05f);

                    ctx.Color(Color.magenta);
                    for (int i = 0, len = _records.Count; i < len; i++)
                    {
                        var record = _records[i];
                        var collider = record.Collider;
                        if (null == collider)
                        {
                            continue;
                        }

                        ctx.Prism(collider.bounds);
                    }
                });
            }
        }

        /// <summary>
        /// Retrieves the internal record for a specific element.
        /// </summary>
        /// <param name="element">The element in question.</param>
        /// <returns></returns>
        private TouchRecord RecordForElement(Element element)
        {
            for (int i = 0, len = _records.Count; i < len; i++)
            {
                var record = _records[i];
                if (record.Element == element)
                {
                    return record;
                }
            }

            return null;
        }

        /// <summary>
        /// Sets up an element.
        /// </summary>
        /// <param name="element">The element.</param>
        private void SetupElement(Element element)
        {
            element.OnDestroyed += Element_OnDestroyed;
        }

        /// <summary>
        /// Tears down an element.
        /// </summary>
        /// <param name="element"></param>
        private void TeardownElement(Element element)
        {
            element.OnDestroyed -= Element_OnDestroyed;
        }

        /// <summary>
        /// Sets up content specifically.
        /// </summary>
        /// <param name="content">The content.</param>
        private void SetupContent(ContentWidget content)
        {
            content.OnLoaded.OnSuccess(Content_OnAssetLoaded);
        }

        /// <summary>
        /// Tears down content listening.
        /// </summary>
        /// <param name="content">The content.</param>
        private void TeardownContent(ContentWidget content)
        {
            content.OnLoaded.Remove(Content_OnAssetLoaded);
        }

        /// <summary>
        /// Called when an element has been destroyed.
        /// </summary>
        /// <param name="el">The element.</param>
        private void Element_OnDestroyed(Element el)
        {
            Unregister(el);
        }

        /// <summary>
        /// Called when a pointer gesture has been started.
        /// </summary>
        /// <param name="id">The id of the pointer.</param>
        private void Gestures_OnPointerStarted(uint id)
        {
            Log.Info(this, "Adding pointer.");

            _pointerIds.Add(id);
        }

        /// <summary>
        /// Called when a pointer gesture has ended.
        /// </summary>
        /// <param name="id">The id of the pointer.</param>
        private void Gestures_OnPointerEnded(uint id)
        {
            Log.Info(this, "Removing pointer.");

            _removedPointerIdQueue.Add(id);
        }

        /// <summary>
        /// Called when the asset has been loaded for an element.
        /// </summary>
        /// <param name="content">The content element.</param>
        private void Content_OnAssetLoaded(ContentWidget content)
        {
            var record = RecordForElement(content);
            if (null == record)
            {
                Log.Error(this, "Somehow there is no record for loaded asset.");
                return;
            }

            var collider = content.Asset.AddComponent<MeshCollider>();
            collider.convex = true;
            collider.isTrigger = true;
            record.Collider = collider;

            Log.Info(this, "Added collider for {0}.", content.Id);
        }
    }
}
