﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FIFA.CommandServices.Interface
{
    public interface ITeamCommandService 
    {
        void SetBadge(SetTeamBadgeCommand command);
    }
}