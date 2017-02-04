using Raven.Client;
using System.Collections.Generic;
using System.Linq;

namespace FIFA.WebApi.Extensions
{
    public static class RavenPagingExtension
    {
        public static List<T> GetAll<T>(this IDocumentSession session)
        {
            const int size = 1024;
            int page = 0;

            RavenQueryStatistics stats;
            List<T> objects = session.Query<T>()
                                  .Statistics(out stats)
                                  .Skip(page * size)
                                  .Take(size)
                                  .ToList();

            page++;

            while ((page * size) <= stats.TotalResults)
            {
                objects.AddRange(session.Query<T>()
                             .Skip(page * size)
                             .Take(size)
                             .ToList());
                page++;
            }

            return objects;
        }
    }
}
