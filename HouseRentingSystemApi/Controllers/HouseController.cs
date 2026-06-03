using HouseRentingSystemApi.Models;
using HouseRentingSystemApi.Services.Contracts;
using HouseRentingSystemApi.Services.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace HouseRentingSystemApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class HouseController : ControllerBase
    {
        private readonly IHouseService houseService;

        public HouseController(IHouseService houseService)
        {
            this.houseService = houseService;
        }

        // ─── ПУБЛИЧНИ ENDPOINTS (без авторизация) ───────────────────────────

        /// <summary>Връща всички къщи</summary>
        [HttpGet("All")]
        [Produces(typeof(IEnumerable<HouseDetailModel>))]
        public async Task<IActionResult> GetAll()
        {
            var model = await houseService.GetAllAsync();
            return Ok(model);
        }

        /// <summary>Връща конкретна къща по ID</summary>
        [HttpGet("{id}")]
        [Produces(typeof(HouseDetailModel))]
        public async Task<IActionResult> GetById(int id)
        {
            var house = await houseService.GetByIdAsync(id);

            if (house == null)
                return NotFound();

            return Ok(house);
        }

        // ─── AGENT ENDPOINTS (само агенти) ──────────────────────────────────

        /// <summary>Агент създава нова къща</summary>
        [Authorize(Roles = "Agent")]
        [HttpPost]
        [Produces(typeof(HouseDetailModel))]
        public async Task<IActionResult> Create([FromBody] HouseDetailModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var agentId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (agentId == null)
                return Unauthorized();

            var created = await houseService.CreateAsync(model, agentId);

            return Created($"api/House/{created.Id}", created);
        }

        /// <summary>Агент редактира съществуваща къща</summary>
        [Authorize(Roles = "Agent")]
        [HttpPut("{id}")]
        [Produces(typeof(HouseDetailModel))]
        public async Task<IActionResult> Edit([FromRoute] int id, [FromBody] HouseDetailModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var updated = await houseService.EditAsync(id, model);

            if (updated == null)
                return NotFound("Къщата не е намерена.");

            return Ok(updated);
        }

        /// <summary>Агент трие къща</summary>
        [Authorize(Roles = "Agent")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete([FromRoute] int id)
        {
            var deleted = await houseService.DeleteAsync(id);

            if (!deleted)
                return NotFound("Къщата не е намерена.");

            return Ok(new { message = $"Къща с id {id} беше изтрита успешно." });
        }

        // ─── CLIENT ENDPOINTS (само клиенти) ────────────────────────────────

        /// <summary>Клиент наема свободна къща</summary>
        [Authorize(Roles = "Client")]
        [HttpPost("{id}/rent")]
        public async Task<IActionResult> Rent([FromRoute] int id)
        {
            var clientId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (clientId == null)
                return Unauthorized();

            var result = await houseService.RentAsync(id, clientId);

            return result switch
            {
                RentResult.NotFound => NotFound("Къщата не е намерена."),
                RentResult.AlreadyRented => BadRequest("Тази къща вече е наета."),
                _ => Ok(new { message = "Успешно наехте къщата." })
            };
        }

        /// <summary>Клиент освобождава наета от него къща</summary>
        [Authorize(Roles = "Client")]
        [HttpPost("{id}/release")]
        public async Task<IActionResult> Release([FromRoute] int id)
        {
            var clientId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (clientId == null)
                return Unauthorized();

            var result = await houseService.ReleaseAsync(id, clientId);

            return result switch
            {
                ReleaseResult.NotFound => NotFound("Къщата не е намерена."),
                ReleaseResult.NotRented => BadRequest("Тази къща не е наета."),
                ReleaseResult.Forbidden => Forbid(),
                _ => Ok(new { message = "Успешно освободихте къщата." })
            };
        }

        /// <summary>Клиент вижда наетите от него къщи</summary>
        [Authorize(Roles = "Client")]
        [HttpGet("MyRented")]
        [Produces(typeof(IEnumerable<HouseDetailModel>))]
        public async Task<IActionResult> MyRented()
        {
            var clientId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (clientId == null)
                return Unauthorized();

            var houses = await houseService.GetRentedByUserAsync(clientId);
            return Ok(houses);
        }

        /// <summary>Агент вижда създадените от него къщи</summary>
        [Authorize(Roles = "Agent")]
        [HttpGet("MyListings")]
        [Produces(typeof(IEnumerable<HouseDetailModel>))]
        public async Task<IActionResult> MyListings()
        {
            var agentId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (agentId == null)
                return Unauthorized();

            var houses = await houseService.GetListingsByAgentAsync(agentId);
            return Ok(houses);
        }
    }
}
