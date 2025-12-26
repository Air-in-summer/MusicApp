using Microsoft.AspNetCore.Mvc;
using MusicAppBackend.Models;
using MusicAppBackend.Services;
using System.Threading.Tasks;

namespace MusicAppBackend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;

        public UserController(IUserService userService)
        {
            _userService = userService;
        }

        // tìm kiếm user dựa trên query 
        [HttpGet("search")]
        public async Task<IActionResult> SearchUsers([FromQuery] string query)
        {
            
            var result = await _userService.SearchUsersAsync(query);

            return Ok(result);
        }
    }
}
