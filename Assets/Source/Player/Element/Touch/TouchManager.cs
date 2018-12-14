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
        private class TouchRecord
        {
            public readonly Element Element;
            public readonly ITouchDelegate Delegate;

            public Collider Collider;

            public bool IsHit { get; private set; }
            public uint HitPointer { get; private set; }

            public TouchRecord(Element element, ITouchDelegate @delegate)
            {
                Element = element;
                Delegate = @delegate;
            }

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

        private readonly List<TouchRecord> _records = new List<TouchRecord>();
        private readonly List<TouchRecord> _removeQueue = new List<TouchRecord>();
        private readonly IGestureManager _gestures;
        private readonly MainCamera _camera;

        private readonly List<uint> _removedPointerIdQueue = new List<uint>();
        private readonly List<uint> _pointerIds = new List<uint>();

        public TouchManager(
            IGestureManager gestures,
            MainCamera camera)
        {
            _gestures = gestures;
            _camera = camera;

            _gestures.OnPointerStarted += Gestures_OnPointerStarted;
            _gestures.OnPointerEnded += Gestures_OnPointerEnded;
        }

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

            SetupElement(element);
            SetupContent(content);

            // add
            _records.Add(new TouchRecord(element, @delegate));

            return true;
        }

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

        private void DetectTouches(uint pointerId)
        {
            Vector3 position;
            if (!_gestures.TryGetPointerOrigin(pointerId, out position))
            {
                return;
            }

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
                        record.Delegate.TouchDragged(record.Element, hit.point);
                    }
                    else if (record.Start(pointerId))
                    {
                        record.Delegate.TouchStarted(record.Element, hit.point);
                    }
                }
                else if (record.IsHit && record.Stop(pointerId))
                {
                    record.Delegate.TouchStopped(record.Element);
                }
            }
        }

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

        private void SetupElement(Element element)
        {
            element.OnDestroyed += Element_OnDestroyed;
        }

        private void TeardownElement(Element element)
        {
            element.OnDestroyed -= Element_OnDestroyed;
        }

        private void SetupContent(ContentWidget content)
        {
            // listen for asset
            content.OnLoaded.OnSuccess(Content_OnAssetLoaded);
        }

        private void TeardownContent(ContentWidget content)
        {
            // stop listening for asset
            content.OnLoaded.Remove(Content_OnAssetLoaded);
        }

        private void Element_OnDestroyed(Element el)
        {
            Unregister(el);
        }

        private void Gestures_OnPointerStarted(uint id)
        {
            _pointerIds.Add(id);
        }

        private void Gestures_OnPointerEnded(uint id)
        {
            _removedPointerIdQueue.Add(id);
        }

        private void Content_OnAssetLoaded(ContentWidget content)
        {
            var collider = content.Asset.AddComponent<MeshCollider>();
            collider.convex = true;
            collider.isTrigger = true;

            var record = RecordForElement(content);
            record.Collider = collider;
        }
    }
}
