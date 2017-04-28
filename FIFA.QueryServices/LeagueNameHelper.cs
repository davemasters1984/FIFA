using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FIFA.QueryServices
{
    public static class LeagueNameHelper
    {
        private static List<string> _premiershipAliases
            = new List<string>
            {
                "p",
                "prem",
                "premiership"
            };

        private static List<string> _championshipAliases
            = new List<string>
            {
                "c",
                "champ",
                "championship"
            };

        public static string ResolveLeagueName(string name)
        {
            if (_premiershipAliases.Any(p => p == name.ToLower()))
                return "premiership";

            if (_premiershipAliases.Any(p => p == name.ToLower()))
                return "championship";

            return name;
        }
    }
}
