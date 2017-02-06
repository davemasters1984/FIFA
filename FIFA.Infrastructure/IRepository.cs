using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FIFA.Infrastructure
{
    public interface IRepository
    {
        T Load<T>(string id);

        IQueryable<T> Query<T>();

        void Store<T>(T entity);

        void Delete(string id);

        void Delete<T>(T entity);
    }
}
