using Microsoft.AspNetCore.Mvc;
using MusicAppBackend.Data;
using Microsoft.EntityFrameworkCore;

namespace MusicAppBackend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TestController : ControllerBase
    {
        public TestController()
        {

        }

        [HttpGet("ping")]
        public IActionResult Ping()
        {
            return Ok("pong");
        }

    }
}