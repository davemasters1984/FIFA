using Raven.Client;
using Raven.Client.Linq;
using System.Collections.Generic;
using System.Linq;


namespace FIFAData
{
    public class ExampleFive
    {
        private IDocumentStore _db;

        public IEnumerable<TeamAssignment> GetTeamAssignmentsForNewLeague(IEnumerable<string> particpantNames)
        {
            var teamAssigner = new TeamAssigner(_db);

            return teamAssigner.GetAssignments(particpantNames);
        }
    }


}
