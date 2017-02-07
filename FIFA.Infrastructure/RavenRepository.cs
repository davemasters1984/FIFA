using Raven.Client;
using System.Linq;

namespace FIFA.Infrastructure
{
    public class RavenRepository : IRepository
    {
        protected IDocumentSession Session
        {
            get {  return ((RavenDbUnitOfWork)UnitOfWorkManager.Current).DocumentSession; }
        }

        public void Delete(string id)
        {
            Session.Delete(id);
        }

        public void Delete<T>(T entity)
        {
            Session.Delete(entity);
        }

        public T Load<T>(string id)
        {
            return Session.Load<T>(id);
        }

        public IQueryable<T> Query<T>()
        {
            return Session.Query<T>();
        }

        public void Store<T>(T entity)
        {
            Session.Store(entity);
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
