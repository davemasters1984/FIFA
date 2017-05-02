using FIFA.QueryServices.Interface.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FIFA.QueryServices.Interface
{
    public interface IPlayerQueryService
    {
        string ResolvePlayerId(string face);

        void InitialiseCache();

        IEnumerable<PlayerSummary> GetPlayers();
    }
}
