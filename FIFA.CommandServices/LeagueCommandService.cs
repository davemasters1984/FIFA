using FIFA.CommandServices.Interface;
using FIFA.Infrastructure;
using FIFA.Model.Services;
using Raven.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FIFA.CommandServices
{
    public class LeagueCommandService : BaseCommandService, ILeagueCommandService
    {
        private IResultService _resultService;
        private IRepository _repository;

        public void CreateLeague(CreateLeagueCommand command)
        {
            var helper = new CreateLeagueHelper();

            var args = helper.CreateLeagueArgs(command.ParticipantFaces);

            var leagueService = new LeagueService();

            var league = leagueService.CreateNewLeague(args);

            using (var unitOfWork = UnitOfWorkFactory.CreateUnitOfWork())
            {
                _repository.Store(league);
            }
        }

        public void PostResult(PostResultCommand command)
        {
            using (var unitOfWork = UnitOfWorkFactory.CreateUnitOfWork())
            {
                _resultService.PostResult(command.AsArgs());
            }
        }
    }
}
