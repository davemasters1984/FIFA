using System;
using System.Collections.Generic;
using System.Linq;

namespace FIFA.Model.Services
{
    public class HandicappedTeamAssigner
    {
        private readonly IEnumerable<Player> _playersOrderedByOverallRanking;
        private readonly IEnumerable<Team> _teams;
        private readonly IEnumerable<int> _possibleTeamRatings;
        private readonly IDictionary<Player, List<int>> _ratingRangesForPlayers = new Dictionary<Player, List<int>>();
        private readonly List<TeamAssignment> _assignments = new List<TeamAssignment>();
        private IEnumerable<League> _previousLeagues;
        private decimal _ratingsRangeIncrement;
        private decimal _ratingRangeFrom;
        private decimal _ratingRangeTo;

        public HandicappedTeamAssigner(IEnumerable<Player> players, 
            IEnumerable<Team> teams, 
            IEnumerable<int> possibleTeamRatings,
            IEnumerable<League> previousLeagues)
        {
            _teams = teams;
            _possibleTeamRatings = possibleTeamRatings;
            _previousLeagues = previousLeagues;

            _playersOrderedByOverallRanking = players
                .Where(p => !p.IsNew)
                .OrderByDescending(p => p.OverallScore)
                .ToList();
        }

        public IEnumerable<TeamAssignment> GetAssignments()
        {
            ClearAnyPreviousAssignments();

            InitialiseRatingsRange();

            AssignRatingRangesToPlayers();

            AssignEligibleTeamsToPlayersFromRatingRanges();

            return _assignments;
        }

        private void InitialiseRatingsRange()
        {
            _ratingsRangeIncrement = CalculateRatingsRangeIncrement();
            _ratingRangeFrom = _possibleTeamRatings.Min();
            _ratingRangeTo = _ratingRangeFrom + _ratingsRangeIncrement;
        }

        private void AssignRatingRangesToPlayers()
        {
            foreach (Player player in _playersOrderedByOverallRanking)
            {
                var eligibleTeamRatingsForPlayer = _possibleTeamRatings
                    .Where(IsWithinCurrentRatingsRange)
                    .ToList();

                _ratingRangesForPlayers.Add(player, eligibleTeamRatingsForPlayer);

                MoveToNextRatingsRange();
            }
        }

        private void AssignEligibleTeamsToPlayersFromRatingRanges()
        {
            foreach(var playerWithRatingsRange in _ratingRangesForPlayers)
            {
                var assignment
                    = GetTeamAssignmentForPlayerFromEligibleRatings(playerWithRatingsRange.Key, 
                        playerWithRatingsRange.Value);

                _assignments.Add(assignment);
            }
        }

        private TeamAssignment GetTeamAssignmentForPlayerFromEligibleRatings(Player player, 
            List<int> eligibleTeamRatings)
        {
            var eligibleTeamsFromAssignedRatings 
                = GetEligibleTeamsFromPlayersAssignedRatings(eligibleTeamRatings);

            var eligibleTeamsNotPreviouslyAssignedToPlayer
                = eligibleTeamsFromAssignedRatings
                    .Where(team => !IsPreviouslyAssignedToPlayer(player, team))
                    .ToList();

            var eligableTeams = (eligibleTeamsNotPreviouslyAssignedToPlayer.Any())
                ? eligibleTeamsNotPreviouslyAssignedToPlayer
                : eligibleTeamsFromAssignedRatings;

            var randomEligibleTeam
                = GetRandomTeamFromEligibleTeams(eligableTeams);

            return new TeamAssignment
            {
                Player = player,
                EligibleTeamRatings = eligibleTeamRatings,
                Team = randomEligibleTeam
            };
        }

        private bool IsPreviouslyAssignedToPlayer(Player player, Team team)
        {
            foreach(var league in _previousLeagues)
                if (league.Participants.Any(p => p.PlayerId == player.Id && p.TeamId == team.Id))
                    return true;

            return false;
        }

        private List<Team> GetEligibleTeamsFromPlayersAssignedRatings(List<int> playersAssignedRatings)
        {
            return _teams.Where(t => playersAssignedRatings.Any(r => r == t.OverallRating))
                .ToList();
        }

        private Team GetRandomTeamFromEligibleTeams(List<Team> eligibleTeams)
        {
            var rnd = new Random();
            var randomTeamIndex = rnd.Next(eligibleTeams.Count - 1);

            return eligibleTeams[randomTeamIndex];
        }

        private void ClearAnyPreviousAssignments()
        {
            _assignments.Clear();
        }

        private decimal CalculateRatingsRangeIncrement()
        {
            return ((decimal)_possibleTeamRatings.Max() - (decimal)_possibleTeamRatings.Min()) / (decimal)_playersOrderedByOverallRanking.Count();
        }

        private bool IsWithinCurrentRatingsRange(int possibleTeamRating)
        {
            return possibleTeamRating >= _ratingRangeFrom && possibleTeamRating <= _ratingRangeTo;
        }

        private void MoveToNextRatingsRange()
        {
            _ratingRangeFrom = _ratingRangeTo;
            _ratingRangeTo += _ratingsRangeIncrement;
        }
    }
}
