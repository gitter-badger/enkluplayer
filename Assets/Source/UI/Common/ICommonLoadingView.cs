﻿namespace CreateAR.EnkluPlayer
{
    /// <summary>
    /// Loading view.
    /// </summary>
    public interface ICommonLoadingView : IUIElement
    {
        /// <summary>
        /// Get/set status.
        /// </summary>
        string Status { get; set; }
    }
}