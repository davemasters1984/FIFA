using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FIFA.QueryServices.Models
{
    public class LeagueTableRow
    {
        public string LeagueId { get; set; }

        public string PlayerName { get; set; }

        public string TeamName { get; set; }

        public int Position { get; set; }

        public int GamesPlayed { get; set; }

        public int Points { get; set; }

        public int GoalsFor { get; set; }

        public int GoalsAgainst { get; set; }

        public int TeamRating { get; set; }
    }
}