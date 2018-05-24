using System;
using System.Collections.Generic;
using CreateAR.Commons.Unity.Logging;

namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// Object that stores UI stack ids from a certain point onward.
    /// </summary>
    public class UIManagerFrame
    {
        /// <summary>
        /// The UI implementation.
        /// </summary>
        private readonly IUIManager _ui;
        
        /// <summary>
        /// Stack ids.
        /// </summary>
        private readonly Stack<int> _ids = new Stack<int>();
        
        /// <summary>
        /// True iff already released.
        /// </summary>
        private bool _isReleased;

        /// <summary>
        /// True iff already aborted.
        /// </summary>
        private bool _isAborted;

        /// <summary>
        /// Constructor.
        /// </summary>
        public UIManagerFrame(IUIManager ui)
        {
            _ui = ui;
            
            _ui.OnPop += UI_OnPop;
            _ui.OnPush += UI_OnPush;
        }

        /// <summary>
        /// Discards frame without popping.
        /// </summary>
        public void Abort()
        {
            if (_isReleased)
            {
                throw new Exception("Frame already released.");
            }

            if (_isAborted)
            {
                throw new Exception("Frame already aborted.");
            }

            _isAborted = true;
            
            _ui.OnPop -= UI_OnPop;
            _ui.OnPush -= UI_OnPush;
        }

        /// <summary>
        /// Triggers removal of all stored ui.
        /// </summary>
        public void Release()
        {
            if (_isReleased)
            {
                throw new Exception("Frame already released.");
            }

            if (_isAborted)
            {
                throw new Exception("Frame already aborted.");
            }

            _isReleased = true;

            _ui.OnPop -= UI_OnPop;
            _ui.OnPush -= UI_OnPush;

            while (_ids.Count > 0)
            {
                _ids.Pop();
                _ui.Pop();
            }
        }
        
        /// <summary>
        /// Called when the UI pops something off the stack.
        /// </summary>
        private void UI_OnPop()
        {
            var stackId = _ids.Pop();
            
            Log.Info(this, "Ppop {0}", stackId);
        }
        
        /// <summary>
        /// Called when the UI pushes something onto the stack.
        /// </summary>
        /// <param name="stackId">Id of the element on the stack.</param>
        private void UI_OnPush(int stackId)
        {
            Log.Info(this, "Push {0}", stackId);
            _ids.Push(stackId);
        }
    }
}