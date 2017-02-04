using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FIFA.Model
{
    public class League
    {
        public string Id { get; set; }

        public DateTime CreatedDate { get; set; }

        public List<Participant> Participants { get; set; }

        public List<string> ResultIds { get; set; }

        public List<Fixture> Fixtures { get; set; }
    }
}