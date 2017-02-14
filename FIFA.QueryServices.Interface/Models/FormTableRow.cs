using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FIFA.QueryServices.Interface.Models
{
    public class FormTableRow
    {
        public string LeagueId { get; set; }

        public string PlayerId { get; set; }

        public string TeamId { get; set; }

        public string PlayerFace { get; set; }

        public string TeamBadge { get; set; }

        public IEnumerable<Res> Results { get; set; }

        public int TotalPoints { get; set; }
    }

    public class Res
    {
        public int Points { get; set; }
    }
}
