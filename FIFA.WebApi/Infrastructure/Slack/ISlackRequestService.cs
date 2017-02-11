using FIFA.WebApi.Models.Slack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace FIFA.WebApi.Infrastructure.Slack
{
    public interface ISlackRequestService
    {
        Task ExecuteRequestAsync(SlackRequest request);
    }
}