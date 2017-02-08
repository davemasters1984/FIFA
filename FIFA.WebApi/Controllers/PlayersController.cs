using FIFA.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;

namespace FIFA.WebApi.Controllers
{
    public class PlayersController :BaseController
    {
        public IHttpActionResult Get()
        {
            using (var session = DocumentStore.OpenSession())
            {
                var players = session.Query<Player>()
                    .ToList();

                return Ok(players);
            }
        }
    }
}