using KomunalkaAPI.DTO;
using KomunalkaAPI.Models;
using KomunalkaAPI.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace KomunalkaAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsersController(IUnitOfWork unitOfWork) : ControllerBase
{
    [HttpGet(Name = "users")]
    public async Task<ActionResult<ApiResponse<List<UserDto>>>> GetAll()
    {
        var users = await unitOfWork.Users.GetAllAsync();
        IEnumerable<User> userList = users.ToList();
        
        if (!userList.Any())
        {
            return NotFound(ApiResponse<List<UserDto>>.Fail(new[] { "Користувачів не знайдено" }, "Not Found"));
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
                RegionId = address.RegionId,
                ZipCode = address.ZipCode,
                City = address.City,
                Street = address.Street,
                BuildingNumber = address.BuildingNumber,
                ApartmentNumber = address.ApartmentNumber,
                Notes = address.Notes,
                IsPrimary = address.IsPrimary,
                AddressTypeId = address.AddressTypeId,
                CreatedAt = address.CreatedAt,
                UpdatedAt = address.UpdatedAt,
                DeletedAt = address.DeletedAt
            }).ToList(),
            CreatedAt = user.CreatedAt,
            UpdatedAt = user.UpdatedAt,
        }).ToList();
    
        return Ok(ApiResponse<List<UserDto>>.Success(userDtos, "Користувачів отримано"));
    }
    
    [HttpGet("{id:int}", Name = "user")]
    public async Task<ActionResult<ApiResponse<UserDto>>> GetById(int id)
    {
        var user = await unitOfWork.Users.GetByIdAsync(id);
        
        if (user == null)
        {
            return NotFound(ApiResponse<UserDto>.Fail(new[] { "Користувача не знайдено" }, "Not Found"));
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
                RegionId = address.RegionId,
                ZipCode = address.ZipCode,
                City = address.City,
                Street = address.Street,
                BuildingNumber = address.BuildingNumber,
                ApartmentNumber = address.ApartmentNumber,
                Notes = address.Notes,
                IsPrimary = address.IsPrimary,
                AddressTypeId = address.AddressTypeId,
                CreatedAt = address.CreatedAt,
                UpdatedAt = address.UpdatedAt,
                DeletedAt = address.DeletedAt
            }).ToList(),
            CreatedAt = user.CreatedAt,
            UpdatedAt = user.UpdatedAt,
        };
        
        return Ok(ApiResponse<UserDto>.Success(userDto, "Користувача отримано"));
    }
    
    [HttpPost(Name = "createUser")]
    public async Task<ActionResult<ApiResponse<UserDto>>> Create(UserDto userDto)
    {
        var user = new User
        {
            Username = userDto.Username,
            Password = userDto.Password ?? string.Empty,
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
        
        return CreatedAtRoute("user", new { id = createdUserDto.Id }, ApiResponse<UserDto>.Success(createdUserDto, "Користувача створено"));
    }
    
    [HttpPut("{id:int}", Name = "updateUser")]
    public async Task<ActionResult<ApiResponse<UserDto>>> Update(int id, UserDto userDto)
    {
        var user = await unitOfWork.Users.GetByIdAsync(id);
        
        if (user == null)
        {
            return NotFound(ApiResponse<UserDto>.Fail(new[] { "Користувача не знайдено" }, "Not Found"));
        }

        user.Username = userDto.Username;
        user.Password = userDto.Password ?? user.Password;
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
        
        return Ok(ApiResponse<UserDto>.Success(updatedUserDto, "Користувача оновлено"));
    }
    
    [HttpDelete("{id:int}", Name = "deleteUser")]
    public async Task<ActionResult<ApiResponse<object>>> Delete(int id)
    {
        var user = await unitOfWork.Users.GetByIdAsync(id);
        
        if (user == null)
        {
            return NotFound(ApiResponse<object>.Fail(new[] { "Користувача не знайдено" }, "Not Found"));
        }

        unitOfWork.Users.Delete(user);
        await unitOfWork.CompleteAsync();
        
        return Ok(ApiResponse<object>.Success(null, "Користувача видалено"));
    }
}