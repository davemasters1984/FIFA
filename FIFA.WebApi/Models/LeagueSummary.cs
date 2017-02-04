using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FIFA.WebApi.Models
{
    public class LeagueSummary
    {
        public string Id { get; set; }

        public DateTime CreatedDate { get; set; }

        public int NumberOfParticipants { get; set; }

        public decimal PercentComplete { get; set; }

        public decimal AverageGamesPerDay { get; set; }
    }
}