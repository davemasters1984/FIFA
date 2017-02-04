using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FIFA.Model
{
    public class Result
    {
        public string Id { get; set; }

        public string LeagueId { get; set; }

        public DateTime Date { get; set; }

        public string HomePlayerId { get; set; }

        public int HomePlayerGoals { get; set; }

        public string AwayPlayerId { get; set; }

        public int AwayPlayerGoals { get; set; }
    }
}
