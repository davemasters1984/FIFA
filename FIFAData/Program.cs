using FileHelpers;
using HtmlAgilityPack;
using Raven.Client;
using Raven.Client.Document;
using Raven.Client.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace FIFAData
{
    class Program
    {
        static IDocumentStore _db;

        static void Main(string[] args)
        {
            _db = CreateDocumentStore();

            //InstallTeams();
            //InstallParticipants();

            AssignPlayersToTeams(":neil:", 
                ":daveb:", 
                ":mattw:", 
                ":tristan:", 
                ":dom:", 
                ":matt:", 
                ":liam:", 
                ":james:", 
                ":louie:", 
                ":dave:", 
                ":craig:", 
                ":ash:", 
                ":jakub:", 
                ":mogg:", 
                ":luke:");

            Console.Read();
        }

        private static void AssignPlayersToTeams(params string[] playerNames)
        {
            var players = GetParticipantsByName(playerNames);
            var newPlayers = players.Where(p => p.IsNew).ToList();
            var ratedPlayers = players.Where(p => !p.IsNew).ToList();

            var possibleRatings = GetDistinctTeamRatings();

            var helper = new HandicappedTeamAssigner(ratedPlayers, possibleRatings);

            var autoAssignments = helper.GetTeamAssignments();

            var alreadyAssignedTeams = new List<FifaTeam>();


            Console.WriteLine("Handicaped Team Assignments:");
            Console.WriteLine("______________________________________________________________________________");

            foreach (var assignment in autoAssignments)
            {
                var ratings = string.Join(",", assignment.Value.Select(r => r.ToString()).ToArray());
                var team = GetRandomTeamForRating(assignment.Value);

                alreadyAssignedTeams.Add(team);

                Console.WriteLine($"{assignment.Key.Name} - {ratings} - {team.TeamName} ({team.OverallRating}) ");
            }

            var fourStarAssignments = AssignFourStarTeamsToNewbies(newPlayers, alreadyAssignedTeams);

            Console.WriteLine("");
            Console.WriteLine("");

            Console.WriteLine("New Players 4 Star Team Assignments:");
            Console.WriteLine("______________________________________________________________________________");

            foreach (var assignment in fourStarAssignments)
            {
                Console.WriteLine($"{assignment.Key.Name} - {assignment.Value.TeamName} ({assignment.Value.OverallRating}) ");
            }
        }

        private static Dictionary<FIFAData.Player, FifaTeam> AssignFourStarTeamsToNewbies(List<FIFAData.Player> newbies, List<FifaTeam> alreadySelectedTeams)
        {
            var newbieAssignments = new Dictionary<FIFAData.Player, FifaTeam>();

            using (var session = _db.OpenSession())
            {
                var fourStarTeams = session.Query<FifaTeam>()
                    .Where(t => t.Stars == 4m)
                    .ToList();

                
                foreach(var n00b in newbies)
                {
                    FifaTeam randomFourStarTeam = GetRandomTeam(fourStarTeams, alreadySelectedTeams);

                    alreadySelectedTeams.Add(randomFourStarTeam);

                    newbieAssignments.Add(n00b, randomFourStarTeam);
                }
            }

            return newbieAssignments;
                
        }

        private static List<FIFAData.Player> GetParticipantsByName(params string[] faces)
        {
            var allPlayers = GetParticipants();
            var facesAsLowerCase = faces.Select(n => n.ToLower());

            return allPlayers
                .Where(p => facesAsLowerCase.Contains(p.Face.ToLower()))
                .ToList();
        }

        private static FifaTeam GetRandomTeam(List<FifaTeam> teams, List<FifaTeam> alreadySelectedTeams)
        {
            var alreadySelectedTeamNames = alreadySelectedTeams.Select(t => t.TeamName);

            var eligableTeams 
                = teams.Where(t => !t.TeamName.In<string>(alreadySelectedTeamNames))
                .ToList();

            var rnd = new Random();
            var randomTeamIndex = rnd.Next(eligableTeams.Count - 1);

            var team = eligableTeams[randomTeamIndex];

            alreadySelectedTeams.Add(team);

            return team;
        }

        private static Dictionary<FIFAData.Player, FifaTeam> GetParticipantsAssignedToTeams()
        {
            var assignments = new Dictionary<FIFAData.Player, FifaTeam>();

            IDictionary<FIFAData.Player, int> participantsWithAssignedTeamRatings 
                = GetParticipantsWithAssignedTeamRatings();

            foreach (KeyValuePair<FIFAData.Player, int> participant in participantsWithAssignedTeamRatings)
            {
                FifaTeam team = GetRandomTeamForRating(participant.Value);
                assignments.Add(participant.Key, team);
            }

            return assignments;
        }

        private static FifaTeam GetRandomTeamForRating(List<int> teamRating)
        {
            using (var session = _db.OpenSession())
            {
                var teams = session.Query<FifaTeam>()
                    .Where(t => t.OverallRating.In<int>(teamRating))
                    .ToList();

                var rnd = new Random();
                var randomTeamIndex = rnd.Next(teams.Count - 1);

                return teams[randomTeamIndex];
            }
        }

        private static FifaTeam GetRandomTeamForRating(int teamRating)
        {
            using (var session = _db.OpenSession())
            {
                var teams = session.Query<FifaTeam>()
                    .Where(t => t.OverallRating == teamRating)
                    .ToList();

                var rnd = new Random();
                var randomTeamIndex = rnd.Next(teams.Count - 1);

                return teams[randomTeamIndex];
            }
        }
            
        private static Dictionary<FIFAData.Player, int> GetParticipantsWithAssignedTeamRatings()
        {
            var participants = GetParticipantsByPositionPercentages();
            var distinctTeamRatings = GetTeamsRatingsByPositionPercentages();
            var participantsWithAssignedRating = new Dictionary<FIFAData.Player, int>();

            foreach(var partipant in participants)
            {
                var participantsAssignedRating
                    = GetTeamRatingByClosestPositionalPercentage(partipant.Value, distinctTeamRatings);

                participantsWithAssignedRating.Add(partipant.Key, participantsAssignedRating);
            }

            return participantsWithAssignedRating;
        }

        private static int GetTeamRatingByClosestPositionalPercentage(decimal number, 
            Dictionary<decimal, int> distinctTeamRatings)
        {
            decimal closestPercentage 
                = distinctTeamRatings.Keys.Aggregate((x, y) => Math.Abs(x - number) < Math.Abs(y - number) ? x : y);

            return distinctTeamRatings[closestPercentage];
        }

        private static List<FIFAData.Player> GetParticipants()
        {
            using (var session = _db.OpenSession())
            {
                var participants = session.Query<Player>()
                    .ToList();

                return participants;
            }
        }

        private static List<int> GetDistinctTeamRatings()
        {
            using (var session = _db.OpenSession())
            {
                var distinctRatings = session.Query<FifaTeam>()
                    .Select(t => t.OverallRating)
                    .Distinct()
                    .ToList();

                return distinctRatings;
            }
        }

        private static Dictionary<decimal, int> GetTeamsRatingsByPositionPercentages()
        {
            var teamRatings = GetDistinctTeamRatings();

            teamRatings.Remove(0);

            var orderedTeamRatings = teamRatings
                .OrderByDescending(r => r)
                .ToList();

            var dict = new Dictionary<decimal, int>();

            foreach (var rating in orderedTeamRatings.OrderBy(r => r))
            {
                var position
                    = (orderedTeamRatings.IndexOf(rating) + 1);

                decimal ratingAsPositionalPercentage
                    = ((decimal)position / (decimal)orderedTeamRatings.Count) * 100;

                dict.Add(ratingAsPositionalPercentage, rating);
            }

            return dict;
        }

        private static IDictionary<FIFAData.Player, decimal> GetParticipantsByPositionPercentages()
        {
            var participants = GetParticipants();
            var dict = new Dictionary<FIFAData.Player, decimal>();

            var orderedParticipants = participants
                .OrderByDescending(p => p.OverallScore)
                .ToList();

            foreach (var participant in orderedParticipants)
            {
                var positionInOveralStandings 
                    = (orderedParticipants.IndexOf(participant) + 1);

                decimal participantsPositionalPercentage
                    = ((decimal)positionInOveralStandings / (decimal)orderedParticipants.Count) * 100;

                dict.Add(participant, 100-participantsPositionalPercentage);
            }

            return dict;
        }    

        private static void InstallTeams()
        {
            var engine = new FileHelperEngine<FifaTeamCsvItem>();
            var records = engine.ReadFile("C:\\Users\\davemasters\\Desktop\\FIFA.csv");

            using (var session = _db.OpenSession())
            {
                foreach (var team in records)
                    session.Store(team.ToFifaTeam());

                session.SaveChanges();
            }
        }

        private static void InstallParticipants()
        {
            using (var session = _db.OpenSession())
            {
                foreach (var participant in FIFAData.Player.Participants)
                    session.Store(participant);

                session.SaveChanges();
            }
        }

        public static IDocumentStore CreateDocumentStore()
        {
            var documentStore = new DocumentStore();

            documentStore.Url = "http://localhost";
            documentStore.DefaultDatabase = "FIFA";
            documentStore.Initialize();

            return documentStore;
        }


        private static void AssignPlayersToTeamsForNewLeagueBad(params string[] playerNamesForNewLeague)
        {
            var allParticipants = GetParticipants();

            var facesAsLowerCase = playerNamesForNewLeague.Select(n => n.ToLower());

            var participants = allParticipants
                .Where(p => facesAsLowerCase.Contains(p.Face.ToLower()))
                .ToList();

            var newPlayers = participants.Where(p => p.IsNew).ToList();
            var ratedPlayers = participants.Where(p => !p.IsNew).ToList();
            var allPossibleTeamRatings = GetDistinctTeamRatings();

            var teamFinder = new HandicappedTeamAssigner(ratedPlayers, allPossibleTeamRatings);

            var teamAssignments = teamFinder.GetTeamAssignments();

            var alreadyAssignedTeams = new List<FifaTeam>();

            foreach (var assignment in teamAssignments)
            {
                var ratings = string.Join(",", assignment.Value.Select(r => r.ToString()).ToArray());
                var team = GetRandomTeamForRating(assignment.Value);

                alreadyAssignedTeams.Add(team);

                Console.WriteLine($"{assignment.Key.Name} - {ratings} - {team.TeamName} ({team.OverallRating}) ");
            }

            var fourStarAssignments = AssignFourStarTeamsToNewbies(newPlayers, alreadyAssignedTeams);

            foreach (var assignment in fourStarAssignments)
            {
                Console.WriteLine($"{assignment.Key.Name} - {assignment.Value.TeamName} ({assignment.Value.OverallRating}) ");
            }
        }

        


    }
}
