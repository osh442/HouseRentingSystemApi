using HouseRentingSystemApi.Data;
using HouseRentingSystemApi.Data.Entities;
using HouseRentingSystemApi.Models;
using HouseRentingSystemApi.Models.Enums;
using HouseRentingSystemApi.Services.Contracts;
using HouseRentingSystemApi.Services.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace HouseRentingSystemApi.Services
{
    public class HouseService : IHouseService
    {
        private readonly AppDbContext context;

        public HouseService(AppDbContext context)
        {
            this.context = context;
        }

        public async Task<IEnumerable<HouseDetailModel>> GetAllAsync()
        {
            return await context.Houses
                .AsNoTracking()
                .Select(DetailProjection)
                .ToListAsync();
        }

        public async Task<HouseDetailModel?> GetByIdAsync(int id)
        {
            return await context.Houses
                .AsNoTracking()
                .Where(h => h.Id == id)
                .Select(DetailProjection)
                .FirstOrDefaultAsync();
        }

        public async Task<HouseDetailModel> CreateAsync(HouseDetailModel model, string agentId)
        {
            var category = await GetOrCreateCategoryAsync(model.Category.ToString());

            var newHouse = new House
            {
                Title = model.Title,
                Address = model.Address,
                Description = model.Description,
                ImageUrl = model.ImageUrl,
                PricePerMonth = model.PricePerMonth,
                UserId = agentId,
                CategoryId = category.Id
            };

            context.Houses.Add(newHouse);
            await context.SaveChangesAsync();

            return new HouseDetailModel
            {
                Id = newHouse.Id,
                Title = newHouse.Title,
                Address = newHouse.Address,
                ImageUrl = newHouse.ImageUrl,
                Description = newHouse.Description,
                PricePerMonth = newHouse.PricePerMonth,
                Category = model.Category,
                IsRented = false
            };
        }

        public async Task<HouseDetailModel?> EditAsync(int id, HouseDetailModel model)
        {
            var house = await context.Houses.FirstOrDefaultAsync(h => h.Id == id);

            if (house == null)
                return null;

            var category = await GetOrCreateCategoryAsync(model.Category.ToString());

            house.Title = model.Title;
            house.Address = model.Address;
            house.ImageUrl = model.ImageUrl;
            house.Description = model.Description;
            house.PricePerMonth = model.PricePerMonth;
            house.CategoryId = category.Id;

            await context.SaveChangesAsync();

            return ToDetailModel(house);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var house = await context.Houses.FirstOrDefaultAsync(h => h.Id == id);

            if (house == null)
                return false;

            context.Houses.Remove(house);
            await context.SaveChangesAsync();

            return true;
        }

        public async Task<RentResult> RentAsync(int id, string clientId)
        {
            var house = await context.Houses.FirstOrDefaultAsync(h => h.Id == id);

            if (house == null)
                return RentResult.NotFound;

            if (house.RenterId != null)
                return RentResult.AlreadyRented;

            house.RenterId = clientId;
            await context.SaveChangesAsync();

            return RentResult.Success;
        }

        public async Task<ReleaseResult> ReleaseAsync(int id, string clientId)
        {
            var house = await context.Houses.FirstOrDefaultAsync(h => h.Id == id);

            if (house == null)
                return ReleaseResult.NotFound;

            if (house.RenterId == null)
                return ReleaseResult.NotRented;

            if (house.RenterId != clientId)
                return ReleaseResult.Forbidden;

            house.RenterId = null;
            await context.SaveChangesAsync();

            return ReleaseResult.Success;
        }

        public async Task<IEnumerable<HouseDetailModel>> GetRentedByUserAsync(string clientId)
        {
            return await context.Houses
                .AsNoTracking()
                .Where(h => h.RenterId == clientId)
                .Select(DetailProjection)
                .ToListAsync();
        }

        public async Task<IEnumerable<HouseDetailModel>> GetListingsByAgentAsync(string agentId)
        {
            return await context.Houses
                .AsNoTracking()
                .Where(h => h.UserId == agentId)
                .Select(DetailProjection)
                .ToListAsync();
        }

        private async Task<Category> GetOrCreateCategoryAsync(string name)
        {
            var category = await context.Categories
                .FirstOrDefaultAsync(c => c.Name == name);

            if (category == null)
            {
                category = new Category { Name = name };
                context.Categories.Add(category);
                await context.SaveChangesAsync();
            }

            return category;
        }

        // Преводимо от EF Core до SQL - използва се в IQueryable заявки.
        private static readonly Expression<Func<House, HouseDetailModel>> DetailProjection =
            h => new HouseDetailModel
            {
                Id = h.Id,
                Title = h.Title,
                Address = h.Address,
                ImageUrl = h.ImageUrl,
                Description = h.Description,
                PricePerMonth = h.PricePerMonth,
                Category = (CategoryViewEnum)h.CategoryId,
                IsRented = h.RenterId != null
            };

        // За вече материализирани (in-memory) обекти.
        private static HouseDetailModel ToDetailModel(House h) => new HouseDetailModel
        {
            Id = h.Id,
            Title = h.Title,
            Address = h.Address,
            ImageUrl = h.ImageUrl,
            Description = h.Description,
            PricePerMonth = h.PricePerMonth,
            Category = (CategoryViewEnum)h.CategoryId,
            IsRented = h.RenterId != null
        };
    }
}
