using KomunalkaAPI.DTO;
using KomunalkaAPI.Services.Users;
using Microsoft.AspNetCore.Mvc;

namespace KomunalkaAPI.Controllers;

[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
[Asp.Versioning.ApiVersion("1.0")]
public class UsersController(IUserService userService) : ControllerBase
{
    [HttpGet(Name = "users")]
    public async Task<ActionResult<IReadOnlyList<UserDto>>> GetAll()
    {
        var users = await userService.GetAllAsync();

        if (users == null || !users.Any())
        {
            return NotFound();
        }

        return Ok(users);
    }

    [HttpGet("{id:int}", Name = "user")]
    public async Task<ActionResult<UserDto>> GetById(int id)
    {
        var result = await userService.GetByIdAsync(id);

        if (!result.Success)
        {
            if (result.NotFound)
                return NotFound(result.Errors.FirstOrDefault());
            return BadRequest(result.Errors);
        }

        return Ok(result.Data);
    }

    [HttpPost(Name = "createUser")]
    public async Task<ActionResult<UserDto>> Create(UserDto userDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var createdUser = await userService.CreateAsync(userDto);
        return CreatedAtRoute("user", new { id = createdUser.Id }, createdUser);
    }

    [HttpPut("{id:int}", Name = "updateUser")]
    public async Task<ActionResult<UserDto>> Update(int id, UserDto userDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var result = await userService.UpdateAsync(id, userDto);

        if (!result.Success)
        {
            if (result.NotFound)
                return NotFound(result.Errors.FirstOrDefault());
            return BadRequest(result.Errors);
        }

        return Ok(result.Data);
    }

    [HttpDelete("{id:int}", Name = "deleteUser")]
    public async Task<ActionResult> Delete(int id)
    {
        var result = await userService.DeleteAsync(id);

        if (!result.Success)
        {
            if (result.NotFound)
                return NotFound(result.Errors.FirstOrDefault());
            return BadRequest(result.Errors);
        }

        return NoContent();
    }
}
