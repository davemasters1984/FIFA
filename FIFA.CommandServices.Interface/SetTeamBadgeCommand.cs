using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FIFA.CommandServices.Interface
{
    public class SetTeamBadgeCommand
    {
        public string TeamId { get; set; }

        public string Badge { get; set; }
    }
}
