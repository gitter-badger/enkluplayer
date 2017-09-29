using System.Collections.Generic;

namespace CreateAR.Spire
{
    /// <summary>
    /// Creates, caches, releases, and provides searching for <c>Content</c>.
    /// </summary>
    public class ContentManager
    {
        /// <summary>
        /// Internal record for tracking <c>Content</c>.
        /// </summary>
        private class ContentRecord
        {
            /// <summary>
            /// Unique id of the ContentData.
            /// </summary>
            public readonly string Id;

            /// <summary>
            /// Content instance.
            /// </summary>
            public readonly Content Content;

            /// <summary>
            /// Set of unique tags. These are used to cleanup references.
            /// </summary>
            public readonly HashSet<string> Tags = new HashSet<string>();

            /// <summary>
            /// Creates a new record.
            /// </summary>
            /// <param name="content">The content instance.</param>
            public ContentRecord(Content content)
            {
                Id = content.Data.Id;
                Content = content;
            }

            /// <summary>
            /// Adds a set of tags.
            /// </summary>
            /// <param name="tags">The tags to add.</param>
            public void Tag(params string[] tags)
            {
                for (int i = 0, len = tags.Length; i < len; i++)
                {
                    Tags.Add(tags[i]);
                }
            }

            /// <summary>
            /// Removes a set of tags.
            /// </summary>
            /// <param name="tags">The tags to remove.</param>
            public void Untag(params string[] tags)
            {
                for (int i = 0, len = tags.Length; i < len; i++)
                {
                    Tags.Remove(tags[i]);
                }
            }
        }

        /// <summary>
        /// For data lookups.
        /// </summary>
        private readonly IAppDataManager _appData;

        /// <summary>
        /// Object that creates <c>Content</c>.
        /// </summary>
        private readonly IContentFactory _factory;

        /// <summary>
        /// List of all <c>Content</c> instances.
        /// </summary>
        private readonly List<ContentRecord> _all = new List<ContentRecord>();

        /// <summary>
        /// List of only non-unique (shared) <c>Content</c> instances. All
        /// elements in this list are also in _all.
        /// </summary>
        private readonly List<ContentRecord> _shared = new List<ContentRecord>();

        /// <summary>
        /// Creates a new <c>ContentManager</c>.
        /// </summary>
        public ContentManager(
            IContentFactory factory,
            IAppDataManager appData)
        {
            _factory = factory;
            _appData = appData;
        }
        
        /// <summary>
        /// Finds all <c>Content</c> instances for a given <c>ContentData</c> id.
        /// </summary>
        /// <param name="contentId">Unique id of the <c>ContentData</c>.</param>
        /// <param name="content">List to add instances to.</param>
        public void FindAll(string contentId, List<Content> content)
        {
            for (int i = 0, len = _all.Count; i < len; i++)
            {
                var record = _all[i];
                if (record.Id == contentId)
                {
                    content.Add(record.Content);
                }
            }
        }

        /// <summary>
        /// Finds the shared instance of <c>Content</c> for a unique
        /// <c>ContentData</c> id.
        /// </summary>
        /// <param name="contentId"><c>ContentData</c> id.</param>
        /// <returns></returns>
        public Content FindShared(string contentId)
        {
            for (int i = 0, len = _shared.Count; i < len; i++)
            {
                var record = _shared[i];
                if (record.Id == contentId)
                {
                    return record.Content;
                }
            }

            return null;
        }

        /// <summary>
        /// Requests a <c>Conten</c> instance by id.
        /// </summary>
        /// <param name="contentId">Unique id of <c>ContenData</c>.</param>
        /// <param name="tags">Associated meta. These are kept with the instance
        /// so that it may be cleaned up later.</param>
        /// <returns></returns>
        public Content Request(string contentId, params string[] tags)
        {
            var data = _appData.Get<ContentData>(contentId);
            if (null == data)
            {
                return null;
            }

            if (data.Unique)
            {
                return RequestUnique(data, tags);
            }

            return RequestShared(data, tags);
        }

        /// <summary>
        /// Releases an instance of <c>Content</c>. If the instance is shared,
        /// nothing happens. If the instance is unique, Content::Destroy is
        /// called.
        /// </summary>
        /// <param name="content">The content to release.</param>
        public void Release(Content content)
        {
            // only remove unique content instances
            if (content.Data.Unique)
            {
                var record = FindRecord(content.Data.Id, _all);

                // Purposefully not null-checking-- calling Release() more than once
                // is code smell.

                _all.Remove(record);
            }
        }

        /// <summary>
        /// Releases all <c>Content</c> that have no tags other than these. This
        /// releases both shared and unique instances.
        /// </summary>
        /// <param name="tags">The tags.</param>
        public void ReleaseAll(params string[] tags)
        {
            for (var i = _all.Count - 1; i >= 0; i--)
            {
                var record = _all[i];
                record.Untag(tags);

                if (0 == record.Tags.Count)
                {
                    // destroy
                    record.Content.Destroy();

                    // stop tracking
                    _all.RemoveAt(i);
                    _shared.Remove(record);
                }
            }
        }

        /// <summary>
        /// Requests a unique instance.
        /// </summary>
        /// <param name="data">The data to create the instance with.</param>
        /// <param name="tags">Tags with which to tag the <c>Content</c> for later cleanup.</param>
        /// <returns></returns>
        private Content RequestUnique(ContentData data, params string[] tags)
        {
            var record = CreateRecord(data);
            _all.Add(record);

            record.Tag(tags);
            
            return record.Content;
        }

        /// <summary>
        /// Requests a shared instance. Either returns the existing one or creates
        /// a new one.
        /// </summary>
        /// <param name="data">The data to create the instance with.</param>
        /// <param name="tags">Tags with which to tag the <c>Content</c> for later cleanup.</param>
        /// <returns></returns>
        private Content RequestShared(ContentData data, params string[] tags)
        {
            var contentId = data.Id;
            var record = FindRecord(contentId, _shared);
            if (null == record)
            {
                record = CreateRecord(data);

                _all.Add(record);
                _shared.Add(record);
            }

            record.Tag(tags);

            return record.Content;
        }

        /// <summary>
        /// Creates a <c>ContentRecord</c> from a <c>ContentData</c>.
        /// </summary>
        /// <param name="data">The data to create the instance from.</param>
        /// <returns></returns>
        private ContentRecord CreateRecord(ContentData data)
        {
            var content = _factory.Instance(data);
            if (null == content)
            {
                return null;
            }

            return new ContentRecord(content);
        }

        /// <summary>
        /// Locates a record within a list of records.
        /// </summary>
        /// <param name="id">Unique <c>ContentData</c> id.</param>
        /// <param name="records">The list of <c>ContentRecord</c> instances to search through.</param>
        /// <returns></returns>
        private ContentRecord FindRecord(
            string id,
            List<ContentRecord> records)
        {
            for (int i = 0, len = records.Count; i < len; i++)
            {
                var record = records[i];
                if (record.Id == id)
                {
                    return record;
                }
            }

            return null;
        }
    }
}