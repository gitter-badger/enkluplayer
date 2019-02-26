using System;
using System.Collections;
using System.Collections.Generic;
using CreateAR.Commons.Unity.Http;
using Jint;
using Jint.Native;
using Jint.Runtime;

namespace CreateAR.EnkluPlayer.Scripting
{
    /// <summary>
    /// JavaScript interface for setting timers.
    /// 
    /// TODO: call callbacks with same this context as setTimeout was called with.
    /// </summary>
    [JsInterface("timers")]
    public class TimerJsInterface
    {
        /// <summary>
        /// SetTimeout record.
        /// </summary>
        private class TimeoutRecord
        {
            /// <summary>
            /// Unique id.
            /// </summary>
            public readonly int Id;

            /// <summary>
            /// Calling engine-- this is used for this context.
            /// </summary>
            public readonly Engine Engine;

            /// <summary>
            /// Callback to call.
            /// </summary>
            public readonly Func<JsValue, JsValue[], JsValue> Callback;

            /// <summary>
            /// Ms to wait.
            /// </summary>
            public readonly int Ms;

            /// <summary>
            /// Defines time this record was created.
            /// </summary>
            public readonly DateTime StartTime = DateTime.Now;

            /// <summary>
            /// Constructor.
            /// </summary>
            public TimeoutRecord(int id, Engine engine, Func<JsValue, JsValue[], JsValue> callback, int ms)
            {
                Id = id;
                Engine = engine;
                Callback = callback;
                Ms = ms;
            }
        }

        /// <summary>
        /// Used to create session unique ids.
        /// </summary>
        private static int IDS = 1;

        /// <summary>
        /// Records to process every frame.
        /// </summary>
        private readonly List<TimeoutRecord> _records = new List<TimeoutRecord>();

        /// <summary>
        /// Records awaiting to be moved to processing list.
        /// </summary>
        private readonly List<TimeoutRecord> _addWaitList = new List<TimeoutRecord>();

        /// <summary>
        /// Records awaiting removal.
        /// </summary>
        private readonly List<int> _removeWaitList = new List<int>();

        /// <summary>
        /// Null parameters we pass to each callback.
        /// </summary>
        private readonly JsValue[] _nullParams = new JsValue[0];

        /// <summary>
        /// True iff currently working through update loop.
        /// </summary>
        private bool _isUpdating;

        /// <summary>
        /// True iff Destroy has not been called.
        /// </summary>
        private bool _isAlive;

        /// <summary>
        /// Constructor.
        /// </summary>
        public TimerJsInterface(IBootstrapper bootstrapper)
        {
            bootstrapper.BootstrapCoroutine(Update());
        }

        /// <summary>
        /// Destroys the object. No callbacks will be called.
        /// </summary>
        [DenyJsAccess]
        public void Destroy()
        {
            _isAlive = false;
        }

        /// <summary>
        /// Calls a Js function some amount of milliseconds from call.
        /// 
        /// The timeout is guaranteed to have expired, but will not be precisely
        /// timed. The lower bound on MS resolution is given by the current
        /// framerate.
        ///  
        /// This function is guaranteed to delay calling callback for at least
        /// one frame.
        /// 
        /// This function is guaranteed to be available at any time-- even
        /// within callbacks of itself.
        /// </summary>
        /// <param name="engine">The calling engine.</param>
        /// <param name="callback">The callback.</param>
        /// <param name="ms">Number of milliseconds to delay.</param>
        /// <returns>An id unique to this timeout.</returns>
        public int setTimeout(Engine engine, Func<JsValue, JsValue[], JsValue> callback, int ms)
        {
            var record = new TimeoutRecord(IDS++, engine, callback, ms);

            if (_isUpdating)
            {
                _addWaitList.Add(record);
            }
            else
            {
                _records.Add(record);
            }

            return record.Id;
        }

        /// <summary>
        /// Clears timeout from being called.
        /// 
        /// This function can be called at any time before a callback is called
        /// and the callback will be cleared.
        /// </summary>
        /// <param name="id">The unique id returned by setTimeout.</param>
        public void clearTimeout(int id)
        {
            if (_isUpdating)
            {
                _removeWaitList.Add(id);
            }
            else
            {
                RemoveRecord(id);
            }
        }

        /// <summary>
        /// Runs every frame to update records.
        /// </summary>
        /// <returns></returns>
        private IEnumerator Update()
        {
            _isAlive = true;

            while (_isAlive)
            {
                // block adds and removes
                _isUpdating = true;

                var now = DateTime.Now;
                
                // call callbacks
                for (var i = 0; i < _records.Count;)
                {
                    var record = _records[i];

                    // if a clearTimeout has been called while the update loop is processing
                    if (_removeWaitList.Contains(record.Id))
                    {
                        _records.RemoveAt(i);
                    }
                    // if timeout has expired
                    else if (now.Subtract(record.StartTime).TotalMilliseconds >= record.Ms)
                    {
                        _records.RemoveAt(i);

                        record.Callback(
                            JsValue.FromObject(record.Engine, this),
                            _nullParams);
                    }
                    // if the timeout is still waiting
                    else
                    {
                        i++;
                    }
                }

                // remove awaiting removes from both add list and real list
                var len = _removeWaitList.Count;
                if (len > 0)
                {
                    for (var i = 0; i < len; i++)
                    {
                        RemoveRecord(_removeWaitList[i]);
                    }
                    _removeWaitList.Clear();
                }

                // move from add list to real list
                len = _addWaitList.Count;
                if (len > 0)
                {
                    for (var i = 0; i < len; i++)
                    {
                        _records.Add(_addWaitList[i]);
                    }
                    _addWaitList.Clear();
                }

                // unlock
                _isUpdating = false;

                yield return null;
            }
        }

        /// <summary>
        /// Removes a record from the record list or add wait list by id.
        /// </summary>
        /// <param name="id">The id.</param>
        private void RemoveRecord(int id)
        {
            for (int i = 0, len = _records.Count; i < len; i++)
            {
                if (_records[i].Id == id)
                {
                    _records.RemoveAt(i);

                    return;
                }
            }

            for (int i = 0, len = _addWaitList.Count; i < len; i++)
            {
                if (_addWaitList[i].Id == id)
                {
                    _addWaitList.RemoveAt(i);

                    return;
                }
            }
        }
    }
}