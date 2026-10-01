using BLL.DTOs.Promotion;
using BLL.DTOs.Promotion.PromotionList;
using BLL.Services.Interfaces;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EDW.Api.Controllers
{
    [ApiController]
    [Route("api/promotions")]
    [Authorize]
    public class PromotionsController : ControllerBase
    {
        private readonly IPromotionService _promotionService;
        private readonly IValidator<PromotionCreateDto> _createValidator;
        private readonly IValidator<PromotionUpdateDto> _updateValidator;

        public PromotionsController(
            IPromotionService promotionService,
            IValidator<PromotionCreateDto> createValidator,
            IValidator<PromotionUpdateDto> updateValidator)
        {
            _promotionService = promotionService;
            _createValidator = createValidator;
            _updateValidator = updateValidator;
        }

        [HttpGet]
        public async Task<ActionResult<PromotionListResponeDto>> GetPromotions([FromQuery] PromotionListRequest request)
        {
            var result = await _promotionService.GetPromotionsListAsync(request);
            return Ok(result);
        }

        [HttpGet("active")]
        [AllowAnonymous]
        public async Task<ActionResult<List<PromotionDto>>> GetActiveAndUpcomingPromotions()
        {
            var result = await _promotionService.GetActiveAndUpcomingPromotionsAsync();
            return Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] PromotionCreateDto dto)
        {
            var validationResult = await _createValidator.ValidateAsync(dto);
            if (!validationResult.IsValid)
            {
                return BadRequest(validationResult.Errors);
            }

            await _promotionService.CreatePromotionAsync(dto);
            return Ok(new { message = "Promotion created successfully" });
        }

        [HttpPut("{id:guid}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] PromotionUpdateDto dto)
        {
            var validationResult = await _updateValidator.ValidateAsync(dto);
            if (!validationResult.IsValid)
            {
                return BadRequest(validationResult.Errors);
            }

            await _promotionService.UpdatePromotionAsync(id, dto);
            return Ok(new { message = "Promotion updated successfully" });
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            await _promotionService.DeletePromotionAsync(id);
            return Ok(new { message = "Promotion deleted successfully" });
        }
    }
}
