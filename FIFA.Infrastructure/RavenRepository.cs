using Raven.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FIFA.Infrastructure
{
    public class RavenRepository : IRepository
    {
        private readonly IDocumentSession _documentSession;

        protected RavenRepository()
        {
            _documentSession = ((RavenDbUnitOfWork)UnitOfWorkManager.Current).DocumentSession;
        }

        public void Delete(string id)
        {
            _documentSession.Delete(id);
        }

        public void Delete<T>(T entity)
        {
            _documentSession.Delete(entity);
        }

        public T Load<T>(string id)
        {
            return _documentSession.Load<T>(id);
        }

        public IQueryable<T> Query<T>()
        {
            return _documentSession.Query<T>();
        }

        public void Store<T>(T entity)
        {
            _documentSession.Store(entity);
        }

        protected string TranslateId<T>(int id)
        {
            return typeof(T).Name + "s/" + id.ToString();
        }

        protected string TranslateId<T>(string id)
        {
            return typeof(T).Name + "s/" + id;
        }
    }
}
