
using Microsoft.Graph;
using BromcomReset.Api.Models;
using System.Security.Cryptography;

namespace BromcomReset.Api.Services
{
    public class GraphPasswordResetService : IPasswordResetService
    {
        private readonly GraphServiceClient _graph;

        public GraphPasswordResetService(GraphServiceClient graph)
        {
            _graph = graph;
        }

        public async Task<(string newPassword, string userId)> ResetAsync(ResetRequest request, CancellationToken ct)
        {
            // Determine target user id
            string userId;
            if (!string.IsNullOrWhiteSpace(request.UserId))
            {
                userId = request.UserId!;
            }
            else
            {
                // resolve by UPN
                var upn = request.UserPrincipalName!;
                var user = await _graph.Users[upn].Request().GetAsync(ct);
                userId = user.Id!;
            }

            // Generate password if not provided
            var newPassword = request.NewPassword ?? GeneratePassword();

            var userUpdate = new User
            {
                PasswordProfile = new PasswordProfile
                {
                    ForceChangePasswordNextSignIn = request.ForceChangeAtNextSignIn,
                    Password = newPassword
                }
            };

            await _graph.Users[userId].Request().UpdateAsync(userUpdate, ct);

            return (newPassword, userId);
        }

        private static string GeneratePassword()
        {
            // 16 chars: at least 1 upper, 1 lower, 1 digit, 1 special
            const string lowers = "abcdefghijkmnopqrstuvwxyz";
            const string uppers = "ABCDEFGHJKLMNPQRSTUVWXYZ";
            const string digits = "23456789";
            const string specials = "!@#$%^&*()-_=+";
            string all = lowers + uppers + digits + specials;

            var rnd = RandomNumberGenerator.Create();
            char Pick(string chars)
            {
                var b = new byte[4];
                rnd.GetBytes(b);
                var i = BitConverter.ToUInt32(b, 0) % (uint)chars.Length;
                return chars[(int)i];
            }

            var pwd = new List<char>
            {
                Pick(lowers), Pick(uppers), Pick(digits), Pick(specials)
            };
            while (pwd.Count < 16) pwd.Add(Pick(all));
            // Shuffle
            for (int i = pwd.Count - 1; i > 0; i--)
            {
                var b = new byte[4];
                rnd.GetBytes(b);
                int j = (int)(BitConverter.ToUInt32(b, 0) % (uint)(i + 1));
                (pwd[i], pwd[j]) = (pwd[j], pwd[i]);
            }
            return new string(pwd.ToArray());
        }
    }
}
