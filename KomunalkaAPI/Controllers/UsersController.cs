using KomunalkaAPI.DTO;
using KomunalkaAPI.Services.Users;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KomunalkaAPI.Controllers;

[ApiController]
[Authorize]
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
    [Consumes("multipart/form-data")]
    public async Task<ActionResult<UserDto>> Update(int id, [FromForm] UpdateUserRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var result = await userService.UpdateAsync(id, request);

        if (!result.Success)
        {
            if (result.NotFound)
                return NotFound(result.Errors.FirstOrDefault());
            return BadRequest(result.Errors);
        }

        return Ok(result.Data);
    }

    [AllowAnonymous]
    [HttpGet("{id:int}/avatar", Name = "userAvatar")]
    public async Task<IActionResult> GetAvatar(int id)
    {
        var result = await userService.GetAvatarAsync(id, false);

        if (!result.Success)
        {
            if (result.NotFound)
                return NotFound(result.Errors?.FirstOrDefault());
            return BadRequest(result.Errors);
        }

        var avatar = result.Data;
        if (avatar == null)
        {
            return NotFound();
        }

        return File(avatar.FileStream, avatar.MimeType);
    }

    [AllowAnonymous]
    [HttpGet("{id:int}/avatar/thumbnail", Name = "userAvatarThumbnail")]
    public async Task<IActionResult> GetAvatarThumbnail(int id)
    {
        var result = await userService.GetAvatarAsync(id, true);

        if (!result.Success)
        {
            if (result.NotFound)
                return NotFound(result.Errors?.FirstOrDefault());
            return BadRequest(result.Errors);
        }

        var avatar = result.Data;
        if (avatar == null)
        {
            return NotFound();
        }

        return File(avatar.FileStream, avatar.MimeType);
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
