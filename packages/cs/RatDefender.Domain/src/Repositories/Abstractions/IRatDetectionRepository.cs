using Common.Domain.Repositories.Abstractions;
using RatDefender.Domain.Entities;

namespace RatDefender.Domain.Repositories.Abstractions;

public interface IRatDetectionRepository : IRepository<RatDetectionId,
    RatDetection, IRatDetectionRepository>;