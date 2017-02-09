using FIFA.CommandServices.Interface;
using FIFA.Infrastructure;
using FIFA.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FIFA.CommandServices
{
    public class TeamCommandService : ITeamCommandService
    {
        private IRepository _repository;

        public TeamCommandService(IRepository repository)
        {
            _repository = repository;
        }

        public void SetBadge(SetTeamBadgeCommand command)
        {
            using (var unitOfWork = UnitOfWorkFactory.CreateUnitOfWork())
            {
                var team = _repository.Load<Team>(command.TeamId);

                if (team == null)
                    throw new Exception("Team not found");

                team.Badge = command.Badge;

                _repository.Store(team);
            }
        }
    }
}
