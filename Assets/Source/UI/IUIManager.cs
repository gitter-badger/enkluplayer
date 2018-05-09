using System;
using System.Collections.Generic;
using CreateAR.Commons.Unity.Async;

namespace CreateAR.SpirePlayer
{
    public class UIManagerFrame
    {
        private readonly IUIManager _ui;
        private readonly Stack<int> _ids = new Stack<int>();
        private bool _isReleased;

        public UIManagerFrame(IUIManager ui)
        {
            _ui = ui;
            
            _ui.OnPop += UI_OnPop;
            _ui.OnPush += UI_OnPush;
        }

        private void UI_OnPop()
        {
            _ids.Pop();
        }
        
        private void UI_OnPush(int stackId)
        {
            _ids.Push(stackId);
        }

        public void Release()
        {
            if (_isReleased)
            {
                throw new Exception("Frame already released.");
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
    }

    /// <summary>
    /// Describes an object that creates and manages UI.
    /// </summary>
    public interface IUIManager
    {
        /// <summary>
        /// Called when a new element has been opened and added to the stack.
        /// </summary>
        event Action<int> OnPush;
        
        /// <summary>
        /// Called when an element has been removed from the stack.
        /// </summary>
        event Action OnPop;
        
        /// <summary>
        /// Opens a new UI element.
        /// </summary>
        /// <param name="reference">Reference to a UI element.</param>
        /// <param name="stackId">Stack id used to reference element in API.</param>
        /// <returns></returns>
        IAsyncToken<T> Open<T>(UIReference reference, out int stackId) where T : IUIElement;
        
        /// <summary>
        /// Opens a new UI element and discards the id.
        /// </summary>
        /// <param name="reference">Reference to a UI element.</param>
        /// <returns></returns>
        IAsyncToken<T> Open<T>(UIReference reference) where T : IUIElement;

        /// <summary>
        /// Moves down the stack, removing UI elements until the element with
        /// the passed in id is on top.
        /// </summary>
        /// <param name="stackId">The id to move along to in the stack.</param>
        void Reveal(int stackId);

        /// <summary>
        /// Moves down the stack, removing UI elements includig the element
        /// that matches the passed in id.
        /// </summary>
        /// <param name="stackId">The id to move along to in the stack.</param>
        void Close(int stackId);

        /// <summary>
        /// Closes top UI.
        /// </summary>
        /// <returns>Stack id of closed UI or -1 if nothing was removed.</returns>
        int Pop();

        /// <summary>
        /// Creates an object that will track all future pushes and pops.
        /// </summary>
        UIManagerFrame CreateFrame();
    }
}