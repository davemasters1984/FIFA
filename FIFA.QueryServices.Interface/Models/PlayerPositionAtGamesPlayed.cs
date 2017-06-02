using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FIFA.QueryServices.Interface.Models
{
    public class PlayerPositionAtGamesPlayed
    {
        public int Position { get; set; }

        public int GamesPlayed { get; set; }

        public string HomePlayerId { get; set; }

        public string HomePlayerName { get; set; }

        public string HomePlayerFace { get; set; }

        public string HomePlayerTeamBadge { get; set; }

        public string HomePlayerTeamName { get; set; }

        public string AwayPlayerId { get; set; }

        public string AwayPlayerName { get; set; }

        public string AwayPlayerFace { get; set; }

        public string AwayPlayerTeamBadge { get; set; }

        public string AwayPlayerTeamName { get; set; }

        public DateTime ResultDate { get; set; }

        public int HomeGoals { get; set; }

        public int AwayGoals { get; set; }

        public bool IsWin { get;set;}

        public bool IsLoss { get; set; }

        public bool IsDraw { get; set; }
    }
}
