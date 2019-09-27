using Microsoft.AspNetCore.Mvc;

namespace mining_foreman_backend.Controllers {
    [Route("api/[controller]")]
    public class User : Controller {
        // GET
        public Models.Database.User Index() {
            var user = DataAccess.User.SelectUserByAPIToken(Request.Cookies["APIToken"]);
            return user;
        }
    }
}