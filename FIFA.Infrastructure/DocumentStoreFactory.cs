using Raven.Client;
using Raven.Client.Document;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FIFA.Infrastructure
{
    public class DocumentStoreFactory
    {
        public static IDocumentStore CreateDocumentStore()
        {
            var documentStore = new DocumentStore
            {
                ConnectionStringName = "azure",
                DefaultDatabase = "FIFA",
            };

            documentStore.Initialize();
            return documentStore;
        }
    }
}
