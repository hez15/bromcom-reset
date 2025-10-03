
namespace BromcomReset.Api.Models
{
    public class ResetResponse
    {
        public string? UserId { get; set; }
        public string? TempPassword { get; set; }
        public bool ForceChangeAtNextSignIn { get; set; }
        public string? Message { get; set; }
    }
}
