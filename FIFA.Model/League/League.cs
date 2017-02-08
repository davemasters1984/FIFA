using System;
using System.Collections.Generic;
using System.Linq;

namespace FIFA.Model
{
    public class League
    {
        public string Id { get; set; }

        public DateTime CreatedDate { get; set; }

        public List<Participant> Participants { get; set; }

        public List<string> ResultIds { get; set; }

        public List<Fixture> Fixtures { get; set; }

        public void PostResult(Result result)
        {
            if (result == null)
                throw new ArgumentNullException("result");

            if (ResultIds == null)
                ResultIds = new List<string>();

            UpdateParticipantsForPostResult(result);

            ResultIds.Add(result.Id);
        }

        private void UpdateParticipantsForPostResult(Result result)
        {
            UpdateHomeParticipantForPostedResult(result);
            UpdateAwayParticipantForPostedResult(result);
        }

        private void UpdateHomeParticipantForPostedResult(Result result)
        {
            var participant = FindParticipant(result.HomePlayerId);

            participant.PostResultAsHomePlayer(result);
        }

        private void UpdateAwayParticipantForPostedResult(Result result)
        {
            var participant = FindParticipant(result.AwayPlayerId);

            participant.PostResultAsAwayPlayer(result);
        }

        private Participant FindParticipant(string playerId)
        {
            if (Participants == null)
                throw new Exception("This league has no participants");

            var participant = Participants.FirstOrDefault(p => p.PlayerId == playerId);

            if (participant == null)
                throw new Exception(string.Format("This league has no participant with a player Id of '{0}'", playerId));

            return participant;
        }
    }
}