using Microsoft.EntityFrameworkCore;

namespace Common.Infrastructure.Persistence.DbContexts;

public class BaseDbContexts(DbContextOptions options) : DbContext(options)
{
}