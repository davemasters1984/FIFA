using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FIFA.Model
{
    public class LeagueTableSnapshot
    {
        public string Id { get; set; }

        public string LeagueId { get; set; }

        public DateTime SnapshotDate { get; set; }

        public List<SnapshotRow> Rows { get; set; }
    }
}
