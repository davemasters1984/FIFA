using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FIFA.CommandServices.Interface
{
    public class UpdatePlayerCommand
    {
        public string PlayerId { get; set; }

        public string Face { get; set; }

        public string Name { get; set; }

        public string SlackUsername { get; set; }
    }
}
