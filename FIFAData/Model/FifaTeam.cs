﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FIFAData
{
    public class FifaTeam
    {
        public decimal Stars { get; set; }

        public string LogoUrl { get; set; }

        public string TeamName { get; set; }

        public string League { get; set; }

        public int OverallRating { get; set; }
    }
}