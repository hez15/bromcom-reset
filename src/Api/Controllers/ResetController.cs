
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using BromcomReset.Api.Models;
using BromcomReset.Api.Services;
using BromcomReset.Api.Auth;

namespace BromcomReset.Api.Controllers
{
    [ApiController]
    [Route("api/reset")]
    [Authorize(Policy = AuthorizationExtensions.HelpdeskPolicyName)]
    public class ResetController : ControllerBase
    {
        private readonly IPasswordResetService _resetService;
        private readonly AuditLogger _audit;

        public ResetController(IPasswordResetService resetService, AuditLogger audit)
        {
            _resetService = resetService;
            _audit = audit;
        }

        [HttpPost("graph")]
        public async Task<ActionResult<ResetResponse>> ResetViaGraph([FromBody] ResetRequest request, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(request.UserPrincipalName) && string.IsNullOrWhiteSpace(request.UserId))
                return BadRequest("Provide userPrincipalName or userId.");

            try
            {
                var actor = User.Identity?.Name ?? "unknown";
                var (newPassword, userId) = await _resetService.ResetAsync(request, ct);

                await _audit.AppendAsync(new
                {
                    tsUtc = DateTime.UtcNow,
                    action = "graph_password_reset",
                    by = actor,
                    target = request.UserPrincipalName ?? request.UserId,
                    targetId = userId,
                    reason = request.Reason
                }, ct);

                return Ok(new ResetResponse
                {
                    UserId = userId,
                    TempPassword = newPassword,
                    ForceChangeAtNextSignIn = true,
                    Message = "Password reset and set to require change at next sign-in."
                });
            }
            catch (Exception ex)
            {
                await _audit.AppendAsync(new
                {
                    tsUtc = DateTime.UtcNow,
                    action = "graph_password_reset_failed",
                    by = User.Identity?.Name ?? "unknown",
                    target = request.UserPrincipalName ?? request.UserId,
                    error = ex.Message
                }, ct);

                return StatusCode(500, new { error = ex.Message });
            }
        }
    }
}
