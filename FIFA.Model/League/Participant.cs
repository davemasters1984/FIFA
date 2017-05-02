using System;
using System.Collections.Generic;
using System.Linq;

namespace FIFA.Model
{
    public class Participant
    {
        public string PlayerId { get; set; }

        public string TeamId { get; set; }

        public IEnumerable<int> EligibleTeamRatings { get; set; }

        public int Position { get; set; }

        public List<PositionInHistory> PositionHistory { get; set; }

        public int GamesPlayed { get; set; }

        public int Points { get; set; }

        public int GoalsFor { get; set; }

        public int GoalsAgainst { get; set; }

        public int GamesWon { get; set; }

        public int GamesDrawn { get; set; }

        public int GamesLost { get; set; }

        public void PostResultAsHomePlayer(Result result)
        {
            Points += result.HomePoints;
            GoalsFor += result.HomePlayerGoals;
            GoalsAgainst += result.AwayPlayerGoals;
            GamesPlayed++;

            if (result.HomePoints == 3)
                GamesWon++;
            if (result.HomePoints == 0)
                GamesLost++;
            if (result.HomePoints == 1)
                GamesDrawn++;
        }

        public void PostResultAsAwayPlayer(Result result)
        {
            Points += result.AwayPoints;
            GoalsFor += result.AwayPlayerGoals;
            GoalsAgainst += result.HomePlayerGoals;
            GamesPlayed++;

            if (result.AwayPoints == 3)
                GamesWon++;
            if (result.AwayPoints == 0)
                GamesLost++;
            if (result.AwayPoints == 1)
                GamesDrawn++;
        }

        public void UpdatePositionFromResult(int position, Fixture fixture)
        {
            Position = position;

            if (GamesPlayed == 0)
                return;

            if (PositionHistory == null)
                PositionHistory = new List<PositionInHistory>();

            var positionInHistory = PositionHistory.FirstOrDefault(p => p.GamesPlayed == GamesPlayed);

            if (positionInHistory == null)
            {
                positionInHistory = new PositionInHistory { GamesPlayed = GamesPlayed, Position = Position };
                PositionHistory.Add(positionInHistory);
            }
                
            if (fixture.AwayPlayerId == PlayerId || fixture.HomePlayerId == PlayerId)
            {
                positionInHistory.AwayPlayerId = fixture.AwayPlayerId;
                positionInHistory.HomePlayerId = fixture.HomePlayerId;
            }
        }

        
    }

    public class PositionInHistory
    {
        public int GamesPlayed { get; set; }

        public int Position { get; set; }

        public string HomePlayerId { get; set; }

        public string AwayPlayerId { get; set; }
    }
}
