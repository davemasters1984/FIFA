using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FIFA.Model
{
    public class Team
    {
        public string Id { get; set; }

        public decimal Stars { get; set; }

        public string LogoUrl { get; set; }

        public string TeamName { get; set; }

        public string Badge { get; set; }

        public string League { get; set; }

        public int OverallRating { get; set; }
    }
}
