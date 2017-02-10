using FIFA.WebApi.Models.Slack;
using Raven.Client;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Web.Http;

namespace FIFA.WebApi.Controllers
{
    [RoutePrefix("api/slack")]
    public class SlackController : BaseController
    {
        public SlackController(IDocumentStore documentStore)
            :base(documentStore)
        {

        }

        [HttpPost]
        [Route("")]
        public HttpResponseMessage ProcessSlackCommand([FromBody] SlackRequest request)
        {
            var slackResponse = SlackCommand.ExecuteRequest(request);

            var responseMessage = new HttpResponseMessage(HttpStatusCode.OK);

            responseMessage.Content = new StringContent(slackResponse, Encoding.UTF8, "text/plain");

            return responseMessage;
        }
    }
}