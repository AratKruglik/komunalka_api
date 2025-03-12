using Microsoft.EntityFrameworkCore;

namespace KomunalkaAPI.Data;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : DbContext(options);