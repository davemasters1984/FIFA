using FIFA.Model;
using FIFA.WebApi.Models;
using Raven.Client.Indexes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FIFA.WebApi.Infrastructure
{
    public class LeagueSummaryIndex : AbstractIndexCreationTask<League, LeagueSummary>
    {
        public LeagueSummaryIndex()
        {
            Map =
                leagues =>
                from league in leagues
                select new LeagueSummary
                {
                    Id = league.Id,
                    CreatedDate = league.CreatedDate,
                    NumberOfParticipants = league.Participants.Count
                };
        }
    }
}