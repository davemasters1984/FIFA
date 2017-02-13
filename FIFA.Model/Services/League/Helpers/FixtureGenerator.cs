using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FIFA.Model.Services
{
    public class FixtureGenerator
    {
        private List<Fixture> _fixtures = new List<Fixture>();
        private IEnumerable<Participant> _participants;

        public FixtureGenerator(IEnumerable<Participant> participants)
        {
            _participants = participants;
        }

        public List<Fixture> GenerateFixtures()
        {
            Initialise();

            GenerateHomeFixtures();

            GenerateAwayFixtures();

            return _fixtures;
        }

        public void Initialise()
        {
            _fixtures = new List<Fixture>();
        }

        private void GenerateHomeFixtures()
        {
            foreach (var homePlayer in _participants)
            {
                foreach (var awayPlayer in _participants)
                {
                    if (IsFixtureForSamePlayer(homePlayer.PlayerId, awayPlayer.PlayerId))
                        continue;

                    if (DoesFixtureExist(homePlayer.PlayerId, awayPlayer.PlayerId))
                        continue;

                    _fixtures.Add(new Fixture
                    {
                        HomePlayerId = homePlayer.PlayerId,
                        AwayPlayerId = awayPlayer.PlayerId
                    });
                }
            }
        }

        private void GenerateAwayFixtures()
        {
            foreach (var awayPlayer in _participants)
            {
                foreach (var homePlayer in _participants)
                {
                    if (IsFixtureForSamePlayer(homePlayer.PlayerId, awayPlayer.PlayerId))
                        continue;

                    if (DoesFixtureExist(homePlayer.PlayerId, awayPlayer.PlayerId))
                        continue;

                    _fixtures.Add(new Fixture
                    {
                        HomePlayerId = homePlayer.PlayerId,
                        AwayPlayerId = awayPlayer.PlayerId
                    });
                }
            }
        }

        private bool DoesFixtureExist(string homePlayerId, string awayPlayerId)
        {
            return _fixtures.Any(f => f.HomePlayerId == homePlayerId && f.AwayPlayerId == awayPlayerId);
        }

        private bool IsFixtureForSamePlayer(string homePlayerId, string awayPlayerId)
        {
            return homePlayerId == awayPlayerId;
        }
    }
}
