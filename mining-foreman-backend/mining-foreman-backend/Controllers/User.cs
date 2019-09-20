using Microsoft.AspNetCore.Mvc;

namespace mining_foreman_backend.Controllers {
    [Route("api/[controller]")]
    public class User : Controller {
        // GET
        public Models.User Index() {
            HttpContext.Session.ToString();
            var test = Request.Cookies["UserKey"];
            return null;
        }
    }
}