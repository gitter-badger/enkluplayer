namespace CreateAR.SpirePlayer
{
    public class TagResolver
    {
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
                var subquery = subqueries[i];
                if (!MatchSubquery(subquery, ref tags))
                {
                    return false;
                }
            }

            return true;
        }

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