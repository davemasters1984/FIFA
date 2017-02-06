using FIFA.Infrastructure.IoC;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Practices.Unity;

namespace FIFA.Infrastructure
{
    public static class UnitOfWorkFactory
    {
        public static IUnitOfWork CreateUnitOfWork()
        {
            return UnityHelper.Container.Resolve<IUnitOfWork>();
        }
    }
}
