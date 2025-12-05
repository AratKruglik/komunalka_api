using System.IO;

namespace KomunalkaAPI.Services.Users;

public record UserAvatarFile(Stream FileStream, string MimeType);
