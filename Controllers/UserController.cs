using BookStoreApi.Models;
using BookStoreApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BookStoreApi.Controllers;

[Authorize]
[Route("api/[controller]")]
[ApiController]
public class UserController : Controller
{
    private readonly UserService _userService;

    public UserController(UserService userService)
    {
        _userService = userService;
    }

    [HttpGet]
    public async Task<List<User>> Get() => await _userService.GetAsync();

    [HttpGet("{id:length(24)}")]
    public async Task<ActionResult<User>> Get(string id)
    {
        var user = await _userService.GetAsync(id);

        if (user is null)
        {
            return NotFound();
        }

        return Json(user);
    }

    [HttpPost]
    public async Task<ActionResult<User>> Post(User newUser)
    {
        await _userService.CreateAsync(newUser);

        return CreatedAtAction(nameof(Get), new { id = newUser.Id }, newUser);
    }

    [AllowAnonymous]
    [Route("authenticate")]
    [HttpPost]
    public async Task<ActionResult> Login([FromBody] User user)
    {
        var token = await _userService.Authenticate(user.Email, user.Password);

        if (token is null)
        {
            return Unauthorized();
        }

        return Ok(new {token, user});
    }
}