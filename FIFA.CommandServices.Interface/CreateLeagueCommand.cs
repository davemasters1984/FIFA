using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FIFA.CommandServices.Interface
{
    public class CreateLeagueCommand
    {
        public IEnumerable<string> ParticipantFaces { get; set; }

        public string Name { get; set; }

        public decimal MaximumTeamRating { get; set; }

        public decimal MinimumTeamRating { get; set; }
    }
}
