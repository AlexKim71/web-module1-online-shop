using Domain.Models;

namespace Domain.Interfaces;

public interface IRequestRepository
{
    Task<IReadOnlyList<Request>> GetAllAsync(CancellationToken ct = default);
    Task<int> CreateAsync(int resourceId, string customerName, int quantity, CancellationToken ct = default);
    Task<bool> CancelAsync(int requestId, CancellationToken ct = default);
}