using FIFA.Model;
using FIFA.QueryServices.Models;
using Raven.Client.Indexes;
using System.Linq;

namespace FIFA.QueryServices.Indexes
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