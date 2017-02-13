using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FIFA.QueryServices.Interface.Models
{
    public class FixtureSummary
    {
        public string LeagueId { get; set; }

        public string HomePlayerId { get; set; }

        public string HomePlayerName { get; set; }

        public string HomePlayerFace { get; set; }

        public string HomeTeamBadge { get; set; }

        public string HomeTeamName { get; set; }

        public string AwayTeamBadge { get; set; }

        public string AwayTeamName { get; set; }

        public string AwayPlayerId { get; set; }

        public string AwayPlayerName { get; set; }

        public string AwayPlayerFace { get; set; }
    }
}
