using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FIFA.Model.Services
{
    public interface IResultService
    {
        void PostResult(PostResultArgs args);
    }
}
