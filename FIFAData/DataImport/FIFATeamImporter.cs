using FIFA.Model;
using FileHelpers;
using Raven.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace FIFAData.DataImport
{
    public class FIFATeamImporter
    {
        public static void Import(IDocumentStore documentStore, string csvFilePath)
        {
            var engine = new FileHelperEngine<TeamCsvItem>();
            var records = engine.ReadFile(csvFilePath);
            var newTeams = new List<string>();
            var updatedTeams = new List<string>();
            var sameRatings = new List<string>();

            using (var session = documentStore.OpenSession())
            {
                var existingTeams = session.GetAll<Team>();
                Regex rgx = new Regex("[^a-zA-Z0-9 -]");

                foreach (var team in records)
                {
                    var normalisedCsvTeamName = rgx.Replace(team.TeamName, "");

                    var teamDoc = existingTeams
                        .Where(t => rgx.Replace(t.TeamName, "") == normalisedCsvTeamName)
                        .FirstOrDefault();

                    if (teamDoc != null)
                    {
                        if (teamDoc.OverallRating != team.OverallRating)
                        {
                            updatedTeams.Add($"{teamDoc.TeamName} - was {teamDoc.OverallRating} now {team.OverallRating}");
                            teamDoc.Updated = DateTime.Now;
                            teamDoc.OverallRating = team.OverallRating;
                        }
                        else
                        {
                            sameRatings.Add($"{teamDoc.TeamName} - was {teamDoc.OverallRating} now {team.OverallRating}");
                        }
                    }
                    else
                    {
                        teamDoc = team.ToTeam();
                        teamDoc.Created = DateTime.Now;
                        newTeams.Add(teamDoc.TeamName);
                    }

                    session.Store(teamDoc);
                }

                session.SaveChanges();
            }

            Console.WriteLine("{0} teams with the same team rating", sameRatings.Count);

            Console.WriteLine("{0} updated teams:", updatedTeams.Count);
            updatedTeams.ForEach(u => Console.WriteLine(u));

            Console.WriteLine("{0} new teams:", newTeams.Count);
            newTeams.ForEach(n => Console.WriteLine(n));
        }
    }
}
