using Common.Infrastructure.Persistence.Repositories;
using Common.Infrastructure.Persistence.UnitOfWork;
using Microsoft.EntityFrameworkCore;
using RatDefender.Domain.Entities;
using RatDefender.Domain.Repositories.Abstractions;
using RatDefender.Infrastructure.Persistence.DbContexts;

namespace RatDefender.Infrastructure.Persistence.Repositories;

public class RatDetectionRepository(
    RatDefenderDbContext ctx,
    ActiveDbContextCollection active)
    : EfRepository<RatDetectionId, RatDetection, IRatDetectionRepository>(ctx,
        active), IRatDetectionRepository
{
}