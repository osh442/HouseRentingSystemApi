using HouseRentingSystemApi.Models.Authorization;

namespace HouseRentingSystemApi.Services.Contracts
{
    public interface IAuthService
    {
        Task<AuthResult> LoginAsync(LoginModel model);

        Task<AuthResult> RegisterAsync(Register model);
    }
}
