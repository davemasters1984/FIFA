﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FIFA.CommandServices.Interface
{
    public interface ILeagueCommandService
    {
        void CreateLeague(CreateLeagueCommand command);

        void PostResult(PostResultCommand command);
    }
}