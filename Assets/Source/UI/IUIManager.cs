﻿using System;
using CreateAR.Commons.Unity.Async;

namespace CreateAR.EnkluPlayer
{
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
        /// Calls Open then removed the element beneath, effectively replacing the top.
        /// </summary>
        /// <param name="reference">Reference to a UI element.</param>
        /// <returns></returns>
        IAsyncToken<T> Replace<T>(UIReference reference) where T : IUIElement;

        /// <summary>
        /// Opens an overlay, which is not part of the stack.
        /// </summary>
        /// <param name="reference">Reference to a UI element.</param>
        /// <param name="id">An id used for overlays.</param>
        /// <returns></returns>
        IAsyncToken<T> OpenOverlay<T>(UIReference reference, out int id) where T : IUIElement;
        
        /// <summary>
        /// Moves down the stack, removing UI elements until the element with
        /// the passed in id is on top.
        /// </summary>
        /// <param name="stackId">The id to move along to in the stack.</param>
        bool Reveal(int stackId);

        /// <summary>
        /// Moves down the stack, removing UI elements includig the element
        /// that matches the passed in id.
        /// </summary>
        /// <param name="stackId">The id to move along to in the stack.</param>
        bool Close(int stackId);

        /// <summary>
        /// Closes top UI.
        /// </summary>
        /// <returns>Stack id of closed UI or -1 if nothing was removed.</returns>
        int Pop();
        
        /// <summary>
        /// Creates an object that will track all future pushes and pops. Does not apply to overlays.
        /// </summary>
        UIManagerFrame CreateFrame();
    }
}