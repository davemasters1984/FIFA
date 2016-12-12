using Raven.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FIFAData
{
    public static class PagingExtensions
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
