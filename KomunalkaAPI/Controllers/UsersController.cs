using KomunalkaAPI.DTO;
using KomunalkaAPI.Services.Users;
using Microsoft.AspNetCore.Mvc;

namespace KomunalkaAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsersController(IUserService userService) : ControllerBase
{
    [HttpGet(Name = "users")]
    public async Task<ActionResult<ApiResponse<List<UserDto>>>> GetAll()
    {
        var users = await userService.GetAllAsync();
        if (!users.Any())
        {
            return NotFound(ApiResponse<List<UserDto>>.Fail(new[] { "Користувачів не знайдено" }, "Not Found"));
        }

        return Ok(ApiResponse<List<UserDto>>.Success(users.ToList(), "Користувачів отримано"));
    }

    [HttpGet("{id:int}", Name = "user")]
    public async Task<ActionResult<ApiResponse<UserDto>>> GetById(int id)
    {
        var result = await userService.GetByIdAsync(id);

        if (result.NotFound)
        {
            return NotFound(ApiResponse<UserDto>.Fail(new[] { "Користувача не знайдено" }, "Not Found"));
        }

        return Ok(ApiResponse<UserDto>.Success(result.Data!, "Користувача отримано"));
    }

    [HttpPost(Name = "createUser")]
    public async Task<ActionResult<ApiResponse<UserDto>>> Create(UserDto userDto)
    {
        var createdUserDto = await userService.CreateAsync(userDto);
        return CreatedAtRoute("user", new { id = createdUserDto.Id }, ApiResponse<UserDto>.Success(createdUserDto, "Користувача створено"));
    }

    [HttpPut("{id:int}", Name = "updateUser")]
    public async Task<ActionResult<ApiResponse<UserDto>>> Update(int id, UserDto userDto)
    {
        var result = await userService.UpdateAsync(id, userDto);

        if (result.NotFound)
        {
            return NotFound(ApiResponse<UserDto>.Fail(new[] { "Користувача не знайдено" }, "Not Found"));
        }

        return Ok(ApiResponse<UserDto>.Success(result.Data!, "Користувача оновлено"));
    }

    [HttpDelete("{id:int}", Name = "deleteUser")]
    public async Task<ActionResult<ApiResponse<object>>> Delete(int id)
    {
        var result = await userService.DeleteAsync(id);

        if (result.NotFound)
        {
            return NotFound(ApiResponse<object>.Fail(new[] { "Користувача не знайдено" }, "Not Found"));
        }

        return Ok(ApiResponse<object>.Success(null, "Користувача видалено"));
    }
}