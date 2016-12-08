using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FIFAData
{
    public class League
    {
        public string Name { get; set; }

        public List<LeagueParticipant> Participants { get; set; }
    }

    public class LeagueParticipant
    {
        public int ParticipantId { get; set; }

        public int TeamId { get; set; }
    }
}
