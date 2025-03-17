using Microsoft.AspNetCore.Mvc;
using Services.Interfaces;
using Shared.DTOs;
namespace Api.Controllers
{
    [Route("api/user")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;

        public UserController(IUserService userService)
        {
            _userService = userService;
        }

        // GET: api/users
        [HttpGet]
        public async Task<IActionResult> GetAllUsers()
        {
            var users = await _userService.GetAllUsersAsync();
            return Ok(users);
        }

        // GET: api/user/{id}
        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetUserById(Guid id)
        {
            var user = await _userService.GetUserByIdAsync(id);
            if (user == null) return NotFound();
            return Ok(user);
        }

        // UPDATE: api/user/{id}
        [HttpPut("{id:guid}")]
        public async Task<IActionResult> UpdateUser(Guid id, [FromBody] UserDto userDto)
        {
            if (userDto == null) return BadRequest("User data is required.");

            var updatedUser = await _userService.UpdateUserAsync(id, userDto);
            if (updatedUser == null) return NotFound();
            return Ok(updatedUser);
        }

        // DELETE: api/user/{id}
        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> DeleteUser(Guid id)
        {
            var deletedUser = await _userService.DeleteUserAsync(id);
            if (deletedUser == null) return NotFound();
            return Ok(deletedUser);
        }
    }
}
