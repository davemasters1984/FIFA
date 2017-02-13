﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FIFA.WebApi.Models.Slack
{
    public class SlackRequest
    {
        public string token { get; set; }

        public string team_id { get; set; }

        public string team_domain { get; set; }

        public string channel_id { get; set; }

        public string channel_name { get; set; }

        public string user_id { get; set; }

        public string user_name { get; set; }

        public string command { get; set; }

        public string text { get; set; }

        public string response_url { get; set; }
    }
}