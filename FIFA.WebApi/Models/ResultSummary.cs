using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FIFA.WebApi.Models
{
    public class ResultSummary
    {
        public string LeagueId { get; set; }

        public string HomePlayerId { get; set; }

        public string HomePlayerName { get; set; }

        public string HomePlayerFace { get; set; }

        public int HomePlayerGoals { get; set; }

        public string AwayPlayerId { get; set; }

        public string AwayPlayerName { get; set; }

        public string AwayPlayerFace { get; set; }

        public int AwayPlayerGoals { get; set; }
    }
}