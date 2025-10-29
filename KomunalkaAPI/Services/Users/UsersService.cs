using KomunalkaAPI.DTO;
using KomunalkaAPI.Models;
using KomunalkaAPI.Repositories;
using KomunalkaAPI.Services.Common;

namespace KomunalkaAPI.Services.Users;

public class UsersService(IUnitOfWork unitOfWork) : IUserService
{
    public async Task<IReadOnlyList<UserDto>> GetAllAsync()
    {
        var users = await unitOfWork.Users.GetAllAsync();
        var userList = users.ToList();
        var dtos = userList.Select(user => new UserDto
        {
            Id = user.Id,
            Username = user.Username,
            Email = user.Email,
            Addresses = user.UserAddresses?.Select(ua => new AddressDto
            {
                Id = ua.Address.Id,
                UserId = ua.UserId,
                RegionId = ua.Address.RegionId,
                ZipCode = ua.Address.ZipCode,
                City = ua.Address.City,
                Street = ua.Address.Street,
                BuildingNumber = ua.Address.BuildingNumber,
                ApartmentNumber = ua.Address.ApartmentNumber,
                Notes = ua.Address.Notes,
                IsPrimary = ua.IsPrimary,
                AddressTypeId = ua.Address.AddressTypeId,
                CreatedAt = ua.Address.CreatedAt,
                UpdatedAt = ua.Address.UpdatedAt,
                DeletedAt = ua.Address.DeletedAt
            }).ToList(),
            CreatedAt = user.CreatedAt,
            UpdatedAt = user.UpdatedAt,
        }).ToList();
        return dtos;
    }

    public async Task<ServiceResult<UserDto>> GetByIdAsync(int id)
    {
        var user = await unitOfWork.Users.GetByIdAsync(id);
        if (user == null)
        {
            return ServiceResult<UserDto>.NotFoundResult("Користувача не знайдено");
        }

        var dto = new UserDto
        {
            Id = user.Id,
            Username = user.Username,
            Email = user.Email,
            Addresses = user.UserAddresses?.Select(ua => new AddressDto
            {
                Id = ua.Address.Id,
                UserId = ua.UserId,
                RegionId = ua.Address.RegionId,
                ZipCode = ua.Address.ZipCode,
                City = ua.Address.City,
                Street = ua.Address.Street,
                BuildingNumber = ua.Address.BuildingNumber,
                ApartmentNumber = ua.Address.ApartmentNumber,
                Notes = ua.Address.Notes,
                IsPrimary = ua.IsPrimary,
                AddressTypeId = ua.Address.AddressTypeId,
                CreatedAt = ua.Address.CreatedAt,
                UpdatedAt = ua.Address.UpdatedAt,
                DeletedAt = ua.Address.DeletedAt
            }).ToList(),
            CreatedAt = user.CreatedAt,
            UpdatedAt = user.UpdatedAt,
        };
        return ServiceResult<UserDto>.Ok(dto);
    }

    public async Task<UserDto> CreateAsync(UserDto dto)
    {
        var user = new User
        {
            Username = dto.Username,
            Password = BCrypt.Net.BCrypt.HashPassword(dto.Password ?? string.Empty),
            Email = dto.Email,
            Role = "User"
        };

        var entityEntry = await unitOfWork.Users.AddAsync(user);
        await unitOfWork.CompleteAsync();

        var createdUser = entityEntry.Entity;
        return new UserDto
        {
            Id = createdUser.Id,
            Username = createdUser.Username,
            Email = createdUser.Email,
            CreatedAt = createdUser.CreatedAt,
            UpdatedAt = createdUser.UpdatedAt,
        };
    }

    public async Task<ServiceResult<UserDto>> UpdateAsync(int id, UserDto dto)
    {
        var user = await unitOfWork.Users.GetByIdAsync(id);
        if (user == null)
        {
            return ServiceResult<UserDto>.NotFoundResult("Користувача не знайдено");
        }

        user.Username = dto.Username;
        if (!string.IsNullOrEmpty(dto.Password))
        {
            user.Password = BCrypt.Net.BCrypt.HashPassword(dto.Password);
        }
        user.Email = dto.Email;
        user.UpdatedAt = DateTime.UtcNow;

        unitOfWork.Users.Update(user);
        await unitOfWork.CompleteAsync();

        var updatedDto = new UserDto
        {
            Id = user.Id,
            Username = user.Username,
            Email = user.Email,
            CreatedAt = user.CreatedAt,
            UpdatedAt = user.UpdatedAt,
        };
        return ServiceResult<UserDto>.Ok(updatedDto);
    }

    public async Task<ServiceResult<bool>> DeleteAsync(int id)
    {
        var user = await unitOfWork.Users.GetByIdAsync(id);
        if (user == null)
        {
            return ServiceResult<bool>.NotFoundResult("Користувача не знайдено");
        }
        unitOfWork.Users.Delete(user);
        await unitOfWork.CompleteAsync();
        return ServiceResult<bool>.Ok(true);
    }
}