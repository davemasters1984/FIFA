using System;
using System.Collections.Generic;
using System.Linq;

namespace FIFAData
{
    public class HandicappedTeamAssigner
    {
        private readonly IEnumerable<Player> _playersOrderedByOverallRanking;
        private readonly IEnumerable<FifaTeam> _teams;
        private readonly IEnumerable<int> _possibleTeamRatings;
        private readonly IDictionary<Player, List<int>> _ratingRangesForPlayers = new Dictionary<Player, List<int>>();
        private readonly List<TeamAssignment> _assignments = new List<TeamAssignment>();
        private decimal _ratingsRangeIncrement;
        private decimal _ratingFrom;
        private decimal _ratingTo;

        public HandicappedTeamAssigner(IEnumerable<Player> players, IEnumerable<FifaTeam> teams, IEnumerable<int> possibleTeamRatings)
        {
            _teams = teams;
            _possibleTeamRatings = possibleTeamRatings;

            _playersOrderedByOverallRanking = players
                .Where(p => !p.IsNew)
                .OrderByDescending(p => p.OverallScore)
                .ToList();
        }

        public IEnumerable<TeamAssignment> Assign()
        {
            ClearAnyPreviousAssignments();

            InitialiseRatingsRange();

            AssignRatingRangesForPlayers();

            GenerateRandomTeamsFromPlayersRatings();

            return _assignments;
        }

        private IEnumerable<TeamAssignment> GenerateRandomTeamsFromPlayersRatings()
        {
            foreach(var playerWithRatingsRange in _ratingRangesForPlayers)
            {
                var randomTeamFromRatingsRange = GetRandomTeamFromRatingsRange(playerWithRatingsRange.Value);

                yield return new TeamAssignment
                {
                    Player = playerWithRatingsRange.Key,
                    PossibleTeamRatings = playerWithRatingsRange.Value,
                    Team = randomTeamFromRatingsRange
                };
            }
        }

        private FifaTeam GetRandomTeamFromRatingsRange(List<int> rangeOfRatings)
        {
            var possibleTeamsForPlayer 
                = _teams.Where(t => rangeOfRatings.Any(r => r == t.OverallRating))
                    .ToList();

            var rnd = new Random();
            var randomTeamIndex = rnd.Next(possibleTeamsForPlayer.Count - 1);

            return possibleTeamsForPlayer[randomTeamIndex];
        }

        private void ClearAnyPreviousAssignments()
        {
            _assignments.Clear();
        }

        private void InitialiseRatingsRange()
        {
            _ratingsRangeIncrement = GetRatingsRangeIncrement();
            _ratingFrom = _possibleTeamRatings.Min();
            _ratingTo = _ratingFrom + _ratingsRangeIncrement;
        }

        private decimal GetRatingsRangeIncrement()
        {
            return (_possibleTeamRatings.Max() - _possibleTeamRatings.Min()) / _playersOrderedByOverallRanking.Count();
        }

        private void AssignRatingRangesForPlayers()
        {
            foreach (Player player in _playersOrderedByOverallRanking)
            {
                var teamRatingsForPlayer = _possibleTeamRatings
                    .Where(IsWithinCurrentRange)
                    .ToList();

                _ratingRangesForPlayers.Add(player, teamRatingsForPlayer);

                MoveToNextRatingsRange();
            }
        }

        private bool IsWithinCurrentRange(int possibleTeamRating)
        {
            return possibleTeamRating >= _ratingFrom && possibleTeamRating <= _ratingTo;
        }

        private void MoveToNextRatingsRange()
        {
            _ratingFrom = _ratingTo;
            _ratingTo += _ratingsRangeIncrement;
        }
    }
}
