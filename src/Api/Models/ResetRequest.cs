
namespace BromcomReset.Api.Models
{
    public class ResetRequest
    {
        public string? UserPrincipalName { get; set; }
        public string? UserId { get; set; }
        public string? NewPassword { get; set; }
        public bool ForceChangeAtNextSignIn { get; set; } = true;
        public string? Reason { get; set; }
    }
}
