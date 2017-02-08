using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FIFA.Infrastructure
{
    public static class UnitOfWorkManager
    {
        private const string CurrentUnitOfWorkKey = "CurrentUnitOfWork.Key";

        private static IUnitOfWork CurrentUnitOfWork
        {
            get { return Local.Data[CurrentUnitOfWorkKey] as IUnitOfWork; }
            set { Local.Data[CurrentUnitOfWorkKey] = value; }
        }

        public static IUnitOfWork Current
        {
            get
            {
                IUnitOfWork unitOfWork = CurrentUnitOfWork;

                if (unitOfWork == null)
                    throw new InvalidOperationException("You are not in a unit of work");

                return unitOfWork;
            }
            set { CurrentUnitOfWork = value; }
        }

        public static bool IsStarted
        {
            get { return (CurrentUnitOfWork != null); }
        }
    }
}
