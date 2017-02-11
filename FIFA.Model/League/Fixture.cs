using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FIFA.Model
{
    public class Fixture
    {
        public string HomePlayerId { get; set; }

        public string AwayPlayerId { get; set; }

        public Result Result { get; set; }
    }
}
