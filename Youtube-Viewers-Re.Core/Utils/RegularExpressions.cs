using System.Text.RegularExpressions;

namespace Youtube_Viewers_Re.Core.Utils
{
    internal static class RegularExpressions
    {
        public static readonly Regex Viewers = new Regex(
            @"viewCount\"":{\""videoViewCountRenderer\"":{\""viewCount\"":{\""runs\"":\[{\""text\"":\""(.+?)\""}",
            RegexOptions.Compiled
        );

        public static readonly Regex Title = new Regex(
            @"\""title\"":{\""runs\"":\[{\""text\"":\""(.+?)\""}",
            RegexOptions.Compiled
        );

        public static readonly Regex WatchtimeUrl = new Regex(
            @"videostatsWatchtimeUrl\"":{\""baseUrl\"":\""(.+?)\""}", 
            RegexOptions.Compiled
        );
    }
}