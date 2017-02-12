using FIFA.WebApi.Infrastructure.Slack;
using FIFA.WebApi.Models.Slack;
using Raven.Client;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Web.Http;

namespace FIFA.WebApi.Controllers
{
    [RoutePrefix("api/slack")]
    public class SlackController : ApiController
    {
        private ISlackRequestService _slackRequestService;

        public SlackController(ISlackRequestService slackRequestService)
        {
            _slackRequestService = slackRequestService;
        }

        [HttpPost]
        [Route("")]
        public HttpResponseMessage ProcessSlackCommand([FromBody] SlackRequest request)
        {
            var validationResult = _slackRequestService.ValidateRequest(request);

            if (validationResult.IsValid)
                _slackRequestService.ExecuteRequestAsync(request);

            var responseMessage = new HttpResponseMessage(HttpStatusCode.OK);

            responseMessage.Content = new StringContent(validationResult.Message, Encoding.UTF8, "text/plain");

            return responseMessage;
        }
    }
}