using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FIFA.CommandServices.Interface
{
    public class PostResultCommand
    {
        public string LeagueId { get; set; }

        public string HomePlayerId { get; set; }

        public int HomePlayerGoals { get; set; }

        public string AwayPlayerId { get; set; }

        public int AwayPlayerGoals { get; set; }
    }
}
