using Domain.Models;

namespace Domain.Interfaces;

public interface IResourceRepository
{
    Task<IReadOnlyList<Resource>> GetAllAsync(CancellationToken ct = default);
    Task<Resource?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<Resource> AddAsync(Resource resource, CancellationToken ct = default);
    Task<bool> UpdateAsync(Resource resource, CancellationToken ct = default);
    Task<bool> DeleteAsync(int id, CancellationToken ct = default);
}