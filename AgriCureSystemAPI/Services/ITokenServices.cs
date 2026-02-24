using System.Security.Claims;

namespace AgriCureSystemAPI.Services
{
    public interface ITokenServices
    {
        string GenerateAccessToken(IEnumerable<Claim> claims);
        string GenerateRefreshToken();
        ClaimsPrincipal GetPrincipalFromExpiredToken(string token);

    }
}
