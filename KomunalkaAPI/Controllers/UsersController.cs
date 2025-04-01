using KomunalkaAPI.DTO;
using KomunalkaAPI.Models;
using KomunalkaAPI.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace KomunalkaAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsersController(IUnitOfWork unitOfWork) : ControllerBase
{
    [HttpGet(Name = "users")]
    public async Task<ActionResult<UserDto>> GetAll()
    {
        var users = await unitOfWork.Users.GetAllAsync();
        IEnumerable<User> userList = users.ToList();
        
        if (!userList.Any())
        {
            return NotFound();
        }
        await unitOfWork.CompleteAsync();

        var userDtos = userList.Select(user => new UserDto
        {
            Id = user.Id,
            Username = user.Username,
            Email = user.Email,
            Addresses = user.Addresses?.Select(address => new AddressDto
            {
                Id = address.Id,
                UserId = address.UserId,
                ZipCode = address.ZipCode,
                City = address.City ?? string.Empty,
                Street = address.Street,
                Building = address.Building ?? string.Empty,
                CreatedAt = address.CreatedAt,
                UpdatedAt = address.UpdatedAt,
                DeletedAt = address.DeletedAt
            }).ToList(),
            CreatedAt = user.CreatedAt,
            UpdatedAt = user.UpdatedAt,
        }).ToList();
    
        return Ok(userDtos);
    }
    
    [HttpGet("{id:int}", Name = "user")]
    public async Task<ActionResult<UserDto>> GetById(int id)
    {
        var user = await unitOfWork.Users.GetByIdAsync(id);
        
        if (user == null)
        {
            return NotFound();
        }
        await unitOfWork.CompleteAsync();

        var userDto = new UserDto
        {
            Id = user.Id,
            Username = user.Username,
            Email = user.Email,
            Addresses = user.Addresses?.Select(address => new AddressDto
            {
                Id = address.Id,
                UserId = address.UserId,
                ZipCode = address.ZipCode,
                City = address.City ?? string.Empty,
                Street = address.Street,
                Building = address.Building ?? string.Empty,
                CreatedAt = address.CreatedAt,
                UpdatedAt = address.UpdatedAt,
                DeletedAt = address.DeletedAt
            }).ToList(),
            CreatedAt = user.CreatedAt,
            UpdatedAt = user.UpdatedAt,
        };
        
        return Ok(userDto);
    }
    
    [HttpPost(Name = "createUser")]
    public async Task<ActionResult<UserDto>> Create(UserDto userDto)
    {
        var user = new User
        {
            Username = userDto.Username,
            Password = userDto.Password,
            Email = userDto.Email,
        };
        
        var entityEntry = await unitOfWork.Users.AddAsync(user);
        await unitOfWork.CompleteAsync();
        
        var createdUser = entityEntry.Entity;
        var createdUserDto = new UserDto
        {
            Id = createdUser.Id,
            Username = createdUser.Username,
            Email = createdUser.Email,
            CreatedAt = createdUser.CreatedAt,
            UpdatedAt = createdUser.UpdatedAt,
        };
        
        return CreatedAtRoute("user", new { id = createdUserDto.Id }, createdUserDto);
    }
    
    [HttpPut("{id:int}", Name = "updateUser")]
    public async Task<ActionResult<UserDto>> Update(int id, UserDto userDto)
    {
        var user = await unitOfWork.Users.GetByIdAsync(id);
        
        if (user == null)
        {
            return NotFound();
        }

        user.Username = userDto.Username;
        user.Password = userDto.Password;
        user.Email = userDto.Email;
        user.UpdatedAt = DateTime.UtcNow;
        
        unitOfWork.Users.Update(user);
        await unitOfWork.CompleteAsync();
        
        var updatedUserDto = new UserDto
        {
            Id = user.Id,
            Username = user.Username,
            Email = user.Email,
            CreatedAt = user.CreatedAt,
            UpdatedAt = user.UpdatedAt,
        };
        
        return Ok(updatedUserDto);
    }
    
    [HttpDelete("{id:int}", Name = "deleteUser")]
    public async Task<ActionResult> Delete(int id)
    {
        var user = await unitOfWork.Users.GetByIdAsync(id);
        
        if (user == null)
        {
            return NotFound();
        }

        unitOfWork.Users.Delete(user);
        await unitOfWork.CompleteAsync();
        
        return NoContent();
    }
}