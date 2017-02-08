using Microsoft.Practices.Unity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FIFA.Infrastructure.IoC
{
    public class UnityHelper
    {
        public static IUnityContainer Container
        {
            get; private set;
        }

        public static void SetContainer(IUnityContainer container)
        {
            Container = container;
        }
    }
}
