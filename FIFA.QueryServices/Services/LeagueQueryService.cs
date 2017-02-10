using FIFA.Model;
using FIFA.QueryServices.Indexes;
using FIFA.QueryServices.Interface;
using FIFA.QueryServices.Interface.Models;
using Raven.Client;
using System.Collections.Generic;
using System.Linq;

namespace FIFA.QueryServices.Services
{
    public class LeagueQueryService : ILeagueQueryService
    {
        private IDocumentStore _documentStore;

        public LeagueQueryService(IDocumentStore documentStore)
        {
            _documentStore = documentStore;
        }

        public IEnumerable<LeagueTableRow> GetCurrentLeagueTable()
        {
            using (var session = _documentStore.OpenSession())
            {
                var leagueId = session.Query<League>()
                    .OrderByDescending(l => l.CreatedDate)
                    .Select(l => l.Id)
                    .FirstOrDefault();

                return GetLeagueTable(session, leagueId);
            }
        }

        public IEnumerable<LeagueTableRow> GetLeagueTable(string leagueId)
        {
            using (var session = _documentStore.OpenSession())
                return GetLeagueTable(session, leagueId);
        }

        private IEnumerable<LeagueTableRow> GetLeagueTable(IDocumentSession session, string leagueId)
        {
            var leagueTable
                = session.Query<LeagueTableRow, LeagueTableIndex>()
                    .Where(l => l.LeagueId == leagueId)
                    .OrderByDescending(l => l.Points)
                    .ToList();

            return leagueTable;
        }
    }
}
