﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FIFA.QueryServices.Interface.Models
{
    public class PredictedLeagueTableRow
    {
        public string LeagueId { get; set; }

        public string PlayerId { get; set; }

        public string PlayerName { get; set; }

        public string TeamName { get; set; }

        public string TeamBadge { get; set; }

        public string TeamId { get; set; }

        public string PlayerFace { get; set; }

        public int Points { get; set; }
    }
}
