
using BromcomReset.Api.Models;

namespace BromcomReset.Api.Services
{
    public interface IPasswordResetService
    {
        Task<(string newPassword, string userId)> ResetAsync(ResetRequest request, CancellationToken ct);
    }
}
