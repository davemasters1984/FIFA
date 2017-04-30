using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FIFA.QueryServices.Interface.Models
{
    public class PlayerPositionHistory
    {
        public string PlayerId { get; set; }

        public string PlayerFace { get; set; }

        public string PlayerName { get; set; }

        public IEnumerable<PlayerPositionAtDate> History { get; set; }
    }
}
