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
        private static IUnityContainer _container;

        public static IUnityContainer Container
        {
            get { return _container; }
        }

        public static IUnityContainer GetFluentlyConfiguredContainer(Action<IUnityContainer> registerServiceMethod)
        {
            if (_container != null)
                return _container;

            _container = new UnityContainer();

            registerServiceMethod(_container);

            return _container;
        }
    }
}
