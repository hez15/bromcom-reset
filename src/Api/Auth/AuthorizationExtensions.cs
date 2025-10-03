
using Microsoft.AspNetCore.Authorization;

namespace BromcomReset.Api.Auth
{
    public static class AuthorizationExtensions
    {
        public const string HelpdeskPolicyName = "HelpdeskOnly";

        public static void AddGroupPolicy(AuthorizationOptions options, string requiredGroupObjectId)
        {
            options.AddPolicy(HelpdeskPolicyName, policy =>
            {
                policy.RequireAssertion(ctx =>
                {
                    if (string.IsNullOrWhiteSpace(requiredGroupObjectId))
                    {
                        // If not configured, allow any authenticated user (PoC behavior)
                        return ctx.User.Identity?.IsAuthenticated == true;
                    }

                    // Azure AD includes groups in "groups" claim (GUIDs). Might require "GroupMembershipClaims" config in app reg.
                    var groups = ctx.User.FindAll("groups").Select(c => c.Value).ToHashSet(StringComparer.OrdinalIgnoreCase);
                    return groups.Contains(requiredGroupObjectId);
                });
            });
        }
    }
}
