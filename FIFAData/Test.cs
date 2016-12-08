using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FIFAData
{
    class Test
    {
        public object DoSomething()
        {
            if (!someCondition)
                return;
            if (someVariable != SomeStaticClass.SomeConstant)
                return;
            if (!somethingMore.Any(x => x.IsSomething))
                return;

            DoThatThing();
        }

        public object DoSomething()
        {
            if (someCondition)
            {
                if (someVariable == SomeStaticClass.SomeConstant)
                {
                    if (somethingMore.Any(x => x.IsSomething))
                    {
                        DoThatThing();
                    }
                }
            }

            return thatThing;
        }
    }
}
