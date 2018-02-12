using System.Collections.Generic;

namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// Interface for managing content.
    /// </summary>
    public interface IContentManager
    {
        /// <summary>
        /// Finds all <c>Content</c> instances for a given <c>ContentData</c> id.
        /// </summary>
        /// <param name="contentId">Unique id of the <c>ContentData</c>.</param>
        /// <param name="content">List to add instances to.</param>
        void FindAll(string contentId, List<ContentWidget> content);

        /// <summary>
        /// Finds the shared instance of <c>Content</c> for a unique
        /// <c>ContentData</c> id.
        /// </summary>
        /// <param name="contentId"><c>ContentData</c> id.</param>
        /// <returns></returns>
        ContentWidget FindShared(string contentId);

        /// <summary>
        /// Requests a <c>Content</c> instance by id.
        /// </summary>
        /// <param name="contentId">Unique id of <c>ContenData</c>.</param>
        /// <param name="tags">Associated meta. These are kept with the instance
        /// so that it may be cleaned up later.</param>
        /// <returns></returns>
        ContentWidget Request(string contentId, params string[] tags);

        /// <summary>
        /// Releases an instance of <c>Content</c>. If the instance is shared,
        /// nothing happens. If the instance is unique, Content::Destroy is
        /// called.
        /// </summary>
        /// <param name="content">The content to release.</param>
        void Release(ContentWidget content);

        /// <summary>
        /// Releases all <c>Content</c> that have no tags other than these. This
        /// releases both shared and unique instances.
        /// </summary>
        /// <param name="tags">The tags.</param>
        void ReleaseAll(params string[] tags);
    }
}