using ProjectManagementApi.Models;

namespace ProjectManagementApi.Services
{
    public interface ITokenService
    {
        string CreateToken(ApplicationUser user);

        DateTime GetExpirationDate();
    }
}