using FIFA.Model;
using FIFA.WebApi.Infrastructure;
using Raven.Client;
using Raven.Client.Document;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using static FIFA.WebApi.Infrastructure.LeagueTableIndex;

namespace FIFA.WebApi.Helpers
{
    public class LeagueRepository
    {
        private IDocumentStore _documentStore;

        public LeagueRepository()
        {
            CreateDocumentStore();
        }

        public IEnumerable<LeagueTableRow> GetCurrentLeague()
        {
            using (var session = _documentStore.OpenSession())
            {
                return session.Query<LeagueTableRow, LeagueTableIndex>()
                    .ToList();
            }
        }

        private void CreateDocumentStore()
        {
            _documentStore = new DocumentStore
            {
                ConnectionStringName = "RavenHQ",
                DefaultDatabase = "FIFA",
            };

            _documentStore.Initialize();
        }
    }
}