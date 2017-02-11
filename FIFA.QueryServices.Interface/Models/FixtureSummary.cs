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

        public string AwayPlayerId { get; set; }

        public string AwayPlayerName { get; set; }

        public string AwayPlayerFace { get; set; }
    }
}
