using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FIFA.QueryServices.Interface.Models
{
    public class LeagueTableRow
    {
        public string LeagueId { get; set; }

        public string PlayerName { get; set; }

        public string TeamName { get; set; }

        public string TeamBadge { get; set; }

        public string TeamId { get; set; }

        public string PlayerFace { get; set; }

        public int Position { get; set; }

        public int GamesPlayed { get; set; }

        public int Points { get; set; }

        public int GamesWon { get; set; }

        public int GamesLost { get; set; }

        public int GamesDrawn { get; set; }

        public int GoalsFor { get; set; }

        public int GoalsAgainst { get; set; }

        public int TeamRating { get; set; }
    }
}
