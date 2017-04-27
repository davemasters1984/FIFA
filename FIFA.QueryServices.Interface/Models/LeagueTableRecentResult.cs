using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FIFA.QueryServices.Interface.Models
{
    public class LeagueTableRecentResult
    {
        public bool IsWin { get { return OpponentGoals < PlayerGoals; } }

        public bool IsLoss { get { return OpponentGoals > PlayerGoals; } }

        public bool IsDraw { get { return OpponentGoals == PlayerGoals; } }

        public string OpponentTeamName { get; set; }

        public string OpponentPlayerName { get; set; }

        public string OpponentPlayerFace { get; set; }

        public int OpponentGoals { get; set; }

        public int PlayerGoals { get; set; }

        public DateTime ResultDate { get; set; }
    }
}
