using System;
using CreateAR.Trellis.Messages.SearchPublishedApps;

namespace CreateAR.EnkluPlayer
{
    /// <summary>
    /// Describes a UIView in which a user can search for apps.
    /// </summary>
    public interface IAppSearchUIView : IUIElement
    {
        /// <summary>
        /// The query.
        /// </summary>
        string Query { get; }
        
        /// <summary>
        /// Called when query has been updated.
        /// </summary>
        event Action<string> OnQueryUpdated;

        /// <summary>
        /// Called when app has been selected.
        /// </summary>
        event Action<string> OnAppSelected;

        /// <summary>
        /// Called to view private apps.
        /// </summary>
        event Action OnPrivateApps;

        /// <summary>
        /// Initializes the view with apps.
        /// </summary>
        /// <param name="apps">The apps.</param>
        void Init(Body[] apps);
    }
}