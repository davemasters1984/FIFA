using FIFA.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FIFA.Model.Services
{
    public class ResultService : IResultService
    {
        #region Private Fields

        private IRepository _repository;

        #endregion

        #region Constructor

        public ResultService(IRepository repository)
        {
            _repository = repository;
        }

        #endregion

        #region Public Methods

        public void PostResult(PostResultArgs args)
        {
            var league = _repository.Load<League>(args.LeagueId);

            league.PostResult(args);

            _repository.Store(league);
        }

        #endregion

    }
}
