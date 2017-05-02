using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FIFA.QueryServices.Interface.Models
{
    public class PlayerSummary
    {
        public string Face { get; set; }

        public string Name { get; set; }

        public decimal OverallScore { get;set; }
    }
}
