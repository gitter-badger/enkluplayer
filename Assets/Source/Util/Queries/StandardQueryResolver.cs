namespace CreateAR.EnkluPlayer
{
    /// <summary>
    /// An <c>IQueryResolver</c> implementation that allows for AND, OR, and NOT.
    /// 
    /// Commas are ANDs, spaces are ORs, !s are NOTs.
    /// 
    /// Egs-
    /// 
    /// "a" will resolve true for a set of tags including "a"
    /// "a b" will resolve true for a set of tags including "a" OR "b"
    /// "a,b" will resolve true for a set of tags including "a" AND "b"
    /// "!a" will resolve true for a set of tags NOT including "a"
    /// 
    /// These can be composed for powerful effect:
    /// 
    /// "a b,!n,z !!b" is equivalent to: "(a || b) && !n && (z || b)"
    /// </summary>
    public class StandardQueryResolver : IQueryResolver
    {
        /// <inheritdoc cref="IQueryResolver"/>
        public bool Resolve(string query, ref string[] tags)
        {
            if (string.IsNullOrEmpty(query))
            {
                return true;
            }

            if (null == tags || 0 == tags.Length)
            {
                return false;
            }

            query = query.ToLower().Trim(' ');
            if (0 == query.Length)
            {
                return true;
            }

            var subqueries = query.Split(',');
            for (int i = 0, len = subqueries.Length; i < len; i++)
            {
                var subquery = subqueries[i].Trim(' ');
                if (string.IsNullOrEmpty(subquery))
                {
                    return true;
                }

                if (!MatchSubquery(subquery, ref tags))
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Matches an OR clause.
        /// </summary>
        /// <param name="subquery">The or query.</param>
        /// <param name="tags">The tags to match against.</param>
        /// <returns></returns>
        private bool MatchSubquery(string subquery, ref string[] tags)
        {
            var conditions = subquery.Split(' ');
            for (int i = 0, len = conditions.Length; i < len; i++)
            {
                var condition = conditions[i];
                var requirement = true;

                while (condition.StartsWith("!"))
                {
                    requirement = !requirement;
                    condition = condition.Substring(1);
                }

                if (Contains(ref tags, condition) == requirement)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Returns true iff the array of tags contains a specific tag.
        /// </summary>
        /// <param name="tags">Array of tags to search.</param>
        /// <param name="tag">Specific tag to look for.</param>
        /// <returns></returns>
        private bool Contains(ref string[] tags, string tag)
        {
            for (int i = 0, len = tags.Length; i < len; i++)
            {
                if (tags[i].ToLower() == tag)
                {
                    return true;
                }
            }

            return false;
        }
    }
}