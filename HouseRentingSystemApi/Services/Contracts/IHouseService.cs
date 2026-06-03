using HouseRentingSystemApi.Models;
using HouseRentingSystemApi.Services.Models;

namespace HouseRentingSystemApi.Services.Contracts
{
    public interface IHouseService
    {
        Task<IEnumerable<HouseDetailModel>> GetAllAsync();

        Task<HouseDetailModel?> GetByIdAsync(int id);

        Task<HouseDetailModel> CreateAsync(HouseDetailModel model, string agentId);

        Task<HouseDetailModel?> EditAsync(int id, HouseDetailModel model);

        Task<bool> DeleteAsync(int id);

        Task<RentResult> RentAsync(int id, string clientId);

        Task<ReleaseResult> ReleaseAsync(int id, string clientId);

        Task<IEnumerable<HouseDetailModel>> GetRentedByUserAsync(string clientId);

        Task<IEnumerable<HouseDetailModel>> GetListingsByAgentAsync(string agentId);
    }
}
