using FIFA.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace FIFA.WebApi.Controllers
{
    public class TeamsController : BaseController
    {
        public IHttpActionResult Get()
        {
            using (var session = DocumentStore.OpenSession())
            {
                var teams = session.Query<Team>()
                    .ToList();

                return Ok(teams);
            }
        }
    }
}
