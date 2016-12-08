﻿using System;
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
        private decimal _ratingRangeFrom;
        private decimal _ratingRangeTo;

        public HandicappedTeamAssigner(IEnumerable<Player> players, 
            IEnumerable<FifaTeam> teams, 
            IEnumerable<int> possibleTeamRatings)
        {
            _teams = teams;
            _possibleTeamRatings = possibleTeamRatings;

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
                if (player.IsPlayerBanned)
                    continue;

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
            var eligibleTeamsForPlayer 
                = GetEligibleTeamsFromPlayersAssignedRatings(eligibleTeamRatings);

            var randomEligibleTeam
                = GetRandomTeamFromEligibleTeams(eligibleTeamsForPlayer);

            return new TeamAssignment
            {
                Player = player,
                EligibleTeamRatings = eligibleTeamRatings,
                Team = randomEligibleTeam
            };
        }

        private List<FifaTeam> GetEligibleTeamsFromPlayersAssignedRatings(List<int> playersAssignedRatings)
        {
            return _teams.Where(t => playersAssignedRatings.Any(r => r == t.OverallRating))
                .ToList();
        }

        private FifaTeam GetRandomTeamFromEligibleTeams(List<FifaTeam> eligibleTeams)
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
            return (_possibleTeamRatings.Max() - _possibleTeamRatings.Min()) / _playersOrderedByOverallRanking.Count();
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
