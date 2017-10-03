using System.Collections.Generic;

namespace CreateAR.Spire
{
    public interface IScriptManager
    {
        SpireScript FindOne(string id);

        void FindAll(string id, List<SpireScript> scripts);

        /// <summary>
        /// Requests a new <c>SpireScript</c> instance.
        /// </summary>
        /// <param name="info">Script info.</param>
        /// <param name="tags">Associated meta. These are kept with the instance
        /// so that it may be cleaned up later.</param>
        /// <returns></returns>
        SpireScript Create(ScriptInfo info, params string[] tags);

        /// <summary>
        /// Releases an instance of <c>Content</c>. If the instance is shared,
        /// nothing happens. If the instance is unique, Content::Destroy is
        /// called.
        /// </summary>
        /// <param name="content">The content to release.</param>
        void Release(Content content);

        /// <summary>
        /// Releases all <c>Content</c> that have no tags other than these. This
        /// releases both shared and unique instances.
        /// </summary>
        /// <param name="tags">The tags.</param>
        void ReleaseAll(params string[] tags);
    }
}