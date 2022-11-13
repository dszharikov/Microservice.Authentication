﻿using IdentityModule.Infrastructure;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using OpenIddict.Abstractions;
using System.Security.Claims;

namespace IdentityModule.Web.Definitions.Identity
{
    /// <summary>
    /// User Claims Principal Factory override from Microsoft Identity framework
    /// </summary>
    public class ApplicationUserClaimsPrincipalFactory : UserClaimsPrincipalFactory<ApplicationUser, ApplicationRole>
    {
        /// <inheritdoc />
        public ApplicationUserClaimsPrincipalFactory(
            UserManager<ApplicationUser> userManager,
            RoleManager<ApplicationRole> roleManager,
            IOptions<IdentityOptions> optionsAccessor)
            : base(userManager, roleManager, optionsAccessor) { }

        /// <summary>
        /// Creates a <see cref="T:System.Security.Claims.ClaimsPrincipal" /> from an user asynchronously.
        /// </summary>
        /// <param name="user">The user to create a <see cref="T:System.Security.Claims.ClaimsPrincipal" /> from.</param>
        /// <returns>The <see cref="T:System.Threading.Tasks.Task" /> that represents the asynchronous creation operation, containing the created <see cref="T:System.Security.Claims.ClaimsPrincipal" />.</returns>
        public override async Task<ClaimsPrincipal> CreateAsync(ApplicationUser user)
        {
            var principal = await base.CreateAsync(user);

            // remove email and name from all claims

            principal.Identities.First().RemoveClaims("name");
            principal.Identities.First().RemoveClaims("email");

            if (user.ApplicationUserProfile?.Permissions != null)
            {
                var permissions = user.ApplicationUserProfile.Permissions.ToList();
                if (permissions.Any())
                {
                    permissions.ForEach(x => ((ClaimsIdentity)principal.Identity!).AddClaim(new Claim(x.PolicyName, nameof(x.PolicyName).ToLower())));
                }
            }

            //// For this sample, just include all claims in all token types.
            //// In reality, claims' destinations would probably differ by token type and depending on the scopes requested.
            //// In our case (demo) we're using OpenIddictConstants.Destinations.AccessToken and OpenIddictConstants.Destinations.IdentityToken
            foreach (var principalClaim in principal.Claims)
            {
                principalClaim.SetDestinations(OpenIddictConstants.Destinations.AccessToken, OpenIddictConstants.Destinations.IdentityToken);
            }

            return principal;
        }

    }
}