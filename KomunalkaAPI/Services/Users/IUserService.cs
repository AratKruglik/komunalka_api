using KomunalkaAPI.DTO;
using KomunalkaAPI.Services.Common;

namespace KomunalkaAPI.Services.Users;

public interface IUserService
{
    Task<IReadOnlyList<UserDto>> GetAllAsync();
    Task<ServiceResult<UserDto>> GetByIdAsync(int id);
    Task<UserDto> CreateAsync(UserDto dto);
    Task<ServiceResult<UserDto>> UpdateAsync(int id, UpdateUserRequest request);
    Task<ServiceResult<bool>> DeleteAsync(int id);
    Task<ServiceResult<UserAvatarFile>> GetAvatarAsync(int id, bool thumbnail);
}
