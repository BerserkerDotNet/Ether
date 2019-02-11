using System.Linq;

namespace Ether.Core.Constants
{
    public static class WorkItemTags
    {
        public const string OnHold = "onhold";
        public const string Blocked = "blocked";
        public const string CodeReview = "codereview";

        public static bool ContainsTag(string tags, string tag)
        {
            if (string.IsNullOrEmpty(tags))
                return false;

            return tags.Split(';')
                .Select(t => t.Replace(" ", "").ToLower())
                .Contains(tag);
        }
    }
}