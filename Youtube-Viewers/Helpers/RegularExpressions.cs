using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Youtube_Viewers.Helpers
{
    static class RegularExpressions
    {
        public static Regex Viewers = new Regex(@"viewCount\"":{\""videoViewCountRenderer\"":{\""viewCount\"":{\""runs\"":\[{\""text\"":\""(.+?)\""}", RegexOptions.Compiled);
        public static Regex Title = new Regex(@"\""title\"":{\""runs\"":\[{\""text\"":\""(.+?)\""}", RegexOptions.Compiled);
        public static Regex ViewUrl = new Regex(@"videostatsWatchtimeUrl\"":{\""baseUrl\"":\""(.+?)\""}", RegexOptions.Compiled);

        public static Regex Trash = new Regex(@"[=/\-+]", RegexOptions.Compiled);
    }
}
