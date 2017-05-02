using FIFA.Model;
using FIFA.QueryServices.Interface;
using Raven.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using FIFA.QueryServices.Interface.Models;
using FIFA.Infrastructure;

namespace FIFA.QueryServices.Services
{
    public class PlayerQueryService : IPlayerQueryService
    {
        private IDocumentStore _documentStore;
        private const string _playerFaceLookupCacheKey = "player-face-dictionary";

        public PlayerQueryService(IDocumentStore documentStore)
        {
            _documentStore = documentStore;
        }

        public string ResolvePlayerId(string face)
        {
            var dictionary = GetPlayerFaceDictionary();

            if (!dictionary.ContainsKey(face))
                throw new Exception(string.Format("No player found with a face of {0}", face));

            return dictionary[face];
        }

        public void InitialiseCache()
        {
            using (var session = _documentStore.OpenSession())
            {
                var players = session.Query<Player>().ToList();

                var dictionary = new Dictionary<string, string>();

                foreach (var player in players)
                    dictionary.Add(player.Face, player.Id);

                HttpRuntime.Cache.Add(_playerFaceLookupCacheKey,
                    dictionary,
                    null,
                    System.Web.Caching.Cache.NoAbsoluteExpiration,
                    System.Web.Caching.Cache.NoSlidingExpiration,
                    System.Web.Caching.CacheItemPriority.Normal,
                    null);
            }
        }

        private IDictionary<string, string> GetPlayerFaceDictionary()
        { 
            IDictionary<string, string> dictionary
                = HttpRuntime.Cache.Get(_playerFaceLookupCacheKey) as IDictionary<string, string>;

            if (dictionary == null)
            {
                InitialiseCache();
                return GetPlayerFaceDictionary();
            }
                
            return dictionary;
        }

        public IEnumerable<PlayerSummary> GetPlayers()
        {
            using (var session = _documentStore.OpenSession())
            {
                return session.GetAll<Player>().Select(p => new PlayerSummary
                {
                    Face = p.Face,
                    Name = p.Name,
                    OverallScore = p.OverallScore
                });
            }
        }
    }
}
