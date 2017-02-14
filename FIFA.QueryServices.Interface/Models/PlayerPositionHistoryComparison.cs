﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FIFA.QueryServices.Interface.Models
{
    public class PlayerPositionHistoryComparison
    {
        public string PlayerOneId { get; set; }

        public string PlayerOneName { get; set; }

        public IEnumerable<PlayerPosition> PlayerOnePositionHistory { get; set; }

        public string PlayerTwoId { get; set; }

        public string PlayerTwoName { get; set; }

        public IEnumerable<PlayerPosition> PlayerTwoPositionHistory { get; set; }
    }

    public class PlayerPosition
    {
        public DateTime Date { get; set; }

        public int Position { get; set; }
    }
}
