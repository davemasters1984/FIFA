using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FIFA.Model
{
    public class Fixture
    {
        public string Id { get; set; }

        public string LeagueId { get; set; }

        public string HomePlayerId { get; set; }

        public string AwayPlayerId { get; set; }
    }
}
