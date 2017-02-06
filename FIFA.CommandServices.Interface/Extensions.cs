using FIFA.Model.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FIFA.CommandServices.Interface
{
    public static class Extensions
    {
        public static PostResultArgs AsArgs(this PostResultCommand command)
        {
            return new PostResultArgs
            {
                AwayPlayerGoals = command.AwayPlayerGoals,
                AwayPlayerId = command.AwayPlayerId,
                HomePlayerGoals = command.HomePlayerGoals,
                HomePlayerId = command.HomePlayerId
            };
        }

    }
}
