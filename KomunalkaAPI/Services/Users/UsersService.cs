using System.IO;
using KomunalkaAPI.DTO;
using KomunalkaAPI.Models;
using KomunalkaAPI.Repositories;
using KomunalkaAPI.Services.Common;
using KomunalkaAPI.Services.Image;

namespace KomunalkaAPI.Services.Users;

public class UsersService(IUnitOfWork unitOfWork, IImageService imageService, IFileStorageService fileStorage) : IUserService
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly IImageService _imageService = imageService;
    private readonly IFileStorageService _fileStorage = fileStorage;

    public async Task<IReadOnlyList<UserDto>> GetAllAsync()
    {
        var users = await _unitOfWork.Users.GetAllAsync();
        var userList = users.ToList();
        var dtos = userList.Select(user => new UserDto
        {
            Id = user.Id,
            Username = user.Username,
            FirstName = user.FirstName,
            LastName = user.LastName,
            PhoneNumber = user.PhoneNumber,
            Email = user.Email,
            AvatarUrl = BuildAvatarUrl(user.Id, user.AvatarOptimizedPath),
            AvatarThumbnailUrl = BuildAvatarThumbnailUrl(user.Id, user.AvatarThumbnailPath),
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
        var user = await _unitOfWork.Users.GetByIdAsync(id);
        if (user == null)
        {
            return ServiceResult<UserDto>.NotFoundResult("Користувача не знайдено");
        }

        var dto = new UserDto
        {
            Id = user.Id,
            Username = user.Username,
            FirstName = user.FirstName,
            LastName = user.LastName,
            PhoneNumber = user.PhoneNumber,
            Email = user.Email,
            AvatarUrl = BuildAvatarUrl(user.Id, user.AvatarOptimizedPath),
            AvatarThumbnailUrl = BuildAvatarThumbnailUrl(user.Id, user.AvatarThumbnailPath),
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

        var entityEntry = await _unitOfWork.Users.AddAsync(user);
        await _unitOfWork.CompleteAsync();

        var createdUser = entityEntry.Entity;
        return new UserDto
        {
            Id = createdUser.Id,
            Username = createdUser.Username,
            FirstName = createdUser.FirstName,
            LastName = createdUser.LastName,
            PhoneNumber = createdUser.PhoneNumber,
            Email = createdUser.Email,
            AvatarUrl = BuildAvatarUrl(createdUser.Id, createdUser.AvatarOptimizedPath),
            AvatarThumbnailUrl = BuildAvatarThumbnailUrl(createdUser.Id, createdUser.AvatarThumbnailPath),
            CreatedAt = createdUser.CreatedAt,
            UpdatedAt = createdUser.UpdatedAt,
        };
    }

    public async Task<ServiceResult<UserDto>> UpdateAsync(int id, UpdateUserRequest request)
    {
        var user = await _unitOfWork.Users.GetByIdAsync(id);
        if (user == null)
        {
            return ServiceResult<UserDto>.NotFoundResult("User not found");
        }

        // Update basic user info
        user.Username = request.Username;
        user.FirstName = request.FirstName;
        user.LastName = request.LastName;
        user.PhoneNumber = request.PhoneNumber;
        user.Email = request.Email;

        // Handle password change if requested
        if (!string.IsNullOrEmpty(request.NewPassword))
        {
            // Validate that CurrentPassword is provided
            if (string.IsNullOrEmpty(request.CurrentPassword))
            {
                return ServiceResult<UserDto>.Fail("Current password is required to change password");
            }

            // Verify current password
            if (!BCrypt.Net.BCrypt.Verify(request.CurrentPassword, user.Password))
            {
                return ServiceResult<UserDto>.Fail("Current password is incorrect");
            }

            // Hash and update new password
            user.Password = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
        }

        if (request.Avatar != null)
        {
            // Process new avatar and cleanup previous files if needed
            var previousOptimized = user.AvatarOptimizedPath;
            var previousThumbnail = user.AvatarThumbnailPath;

            using var stream = request.Avatar.OpenReadStream();
            var (optimizedPath, thumbnailPath, optimizedSize, _, width, height) =
                await _imageService.ProcessAvatarImageAsync(stream, request.Avatar.FileName, user.Id);

            if (!string.IsNullOrEmpty(previousOptimized))
            {
                await _fileStorage.DeleteFileAsync(previousOptimized, StorageScope.Avatar);
            }

            if (!string.IsNullOrEmpty(previousThumbnail))
            {
                await _fileStorage.DeleteFileAsync(previousThumbnail, StorageScope.Avatar);
            }

            user.AvatarOptimizedPath = optimizedPath;
            user.AvatarThumbnailPath = thumbnailPath;
            user.AvatarMimeType = _imageService.GetImageMimeType(Path.GetExtension(request.Avatar.FileName));
            user.AvatarSizeInBytes = optimizedSize;
            user.AvatarWidth = width;
            user.AvatarHeight = height;
        }

        user.UpdatedAt = DateTime.UtcNow;

        _unitOfWork.Users.Update(user);
        await _unitOfWork.CompleteAsync();

        var updatedDto = new UserDto
        {
            Id = user.Id,
            Username = user.Username,
            FirstName = user.FirstName,
            LastName = user.LastName,
            PhoneNumber = user.PhoneNumber,
            Email = user.Email,
            AvatarUrl = BuildAvatarUrl(user.Id, user.AvatarOptimizedPath),
            AvatarThumbnailUrl = BuildAvatarThumbnailUrl(user.Id, user.AvatarThumbnailPath),
            CreatedAt = user.CreatedAt,
            UpdatedAt = user.UpdatedAt,
        };
        return ServiceResult<UserDto>.Ok(updatedDto);
    }

    public async Task<ServiceResult<bool>> DeleteAsync(int id)
    {
        var user = await _unitOfWork.Users.GetByIdAsync(id);
        if (user == null)
        {
            return ServiceResult<bool>.NotFoundResult("Користувача не знайдено");
        }
        _unitOfWork.Users.Delete(user);
        await _unitOfWork.CompleteAsync();
        return ServiceResult<bool>.Ok(true);
    }

    public async Task<ServiceResult<UserAvatarFile>> GetAvatarAsync(int id, bool thumbnail)
    {
        var user = await _unitOfWork.Users.GetByIdAsync(id);
        if (user == null)
        {
            return ServiceResult<UserAvatarFile>.NotFoundResult("Користувача не знайдено");
        }

        var path = thumbnail ? user.AvatarThumbnailPath : user.AvatarOptimizedPath;
        if (string.IsNullOrEmpty(path))
        {
            return ServiceResult<UserAvatarFile>.NotFoundResult("Аватар ще не завантажено");
        }

        var fileStream = await _fileStorage.GetFileAsync(path, StorageScope.Avatar);
        if (fileStream == null)
        {
            return ServiceResult<UserAvatarFile>.Fail("Файл аватара недоступний");
        }

        var mimeType = user.AvatarMimeType
                       ?? _imageService.GetImageMimeType(Path.GetExtension(path));

        return ServiceResult<UserAvatarFile>.Ok(new UserAvatarFile(fileStream, mimeType));
    }

    private static string? BuildAvatarUrl(int userId, string? path)
    {
        return string.IsNullOrEmpty(path) ? null : $"/api/v1/users/{userId}/avatar";
    }

    private static string? BuildAvatarThumbnailUrl(int userId, string? path)
    {
        return string.IsNullOrEmpty(path) ? null : $"/api/v1/users/{userId}/avatar/thumbnail";
    }
}
