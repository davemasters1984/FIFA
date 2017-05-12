using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FIFA.QueryServices.Interface.Models
{
    public class LeagueTable
    {
        public string LeagueName { get; set; }

        public bool IsTopLeague { get; set; }

        public bool IsBottomLeague { get; set; }

        public IEnumerable<LeagueTableRow> Rows { get; set; }
        
    }
}
