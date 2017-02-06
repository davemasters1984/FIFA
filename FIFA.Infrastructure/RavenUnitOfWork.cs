using Raven.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FIFA.Infrastructure
{
    public class RavenDbUnitOfWork : IUnitOfWork
    {
        private readonly IDocumentStore _documentStore;
        private readonly IDocumentSession _documentSession;
        private readonly Guid _transactionId;

        public RavenDbUnitOfWork(IDocumentStore documentStore)
        {
            _transactionId = Guid.NewGuid();
            _documentStore = documentStore;
            _documentSession = _documentStore.OpenSession();

            UnitOfWorkManager.Current = this;
        }

        public object Key
        {
            get
            {
                return _transactionId;
            }
        }

        public IDocumentSession DocumentSession
        {
            get
            {
                return _documentSession;
            }
        }

        public void Commit()
        {
            try
            {
                _documentSession.SaveChanges();
            }
            finally
            {
                _documentSession.Dispose();
            }

        }

        public void Dispose()
        {
            Commit();
        }
    }
}
