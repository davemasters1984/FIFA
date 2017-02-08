using FileHelpers;
using Raven.Client;

namespace FIFAData.DataImport
{
    public class FIFATeamImporter
    {
        public static void Import(IDocumentStore documentStore, string csvFilePath)
        {
            var engine = new FileHelperEngine<TeamCsvItem>();
            var records = engine.ReadFile(csvFilePath);

            using (var session = documentStore.OpenSession())
            {
                foreach (var team in records)
                    session.Store(team.ToTeam());

                session.SaveChanges();
            }
        }
    }
}
