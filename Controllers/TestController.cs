using Microsoft.AspNetCore.Mvc;
using nauth_asp.DbContexts;

namespace nauth_asp.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TestController : ControllerBase
    {
        private readonly NauthDbContext _context;

        public TestController(NauthDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult Get()
        {
            return Ok(new { message = "Database connection successful!", userCount = _context.Users.Count() });
        }
    }
}
