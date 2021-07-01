using System.Text.RegularExpressions;

namespace Youtube_Viewers.Core.Utils
{
    internal static class RegularExpressions
    {
        public static Regex Viewers = new Regex(
            @"viewCount\"":{\""videoViewCountRenderer\"":{\""viewCount\"":{\""runs\"":\[{\""text\"":\""(.+?)\""}",
            RegexOptions.Compiled
        );

        public static Regex Title = new Regex(
            @"\""title\"":{\""runs\"":\[{\""text\"":\""(.+?)\""}",
            RegexOptions.Compiled
        );

        public static Regex WatchtimeUrl = new Regex(
            @"videostatsWatchtimeUrl\"":{\""baseUrl\"":\""(.+?)\""}", 
            RegexOptions.Compiled
        );
    }
}