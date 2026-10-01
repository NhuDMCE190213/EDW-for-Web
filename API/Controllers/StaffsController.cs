using BLL.DTOs.Staff;
using BLL.Services.Interfaces;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EDW.Api.Controllers
{
    [ApiController]
    [Route("api/staffs")]
    [Authorize]
    public class StaffsController : ControllerBase
    {
        private readonly IStaffService _staffService;
        private readonly IValidator<StaffCreateDto> _createValidator;
        private readonly IValidator<StaffUpdateDto> _updateValidator;

        public StaffsController(
            IStaffService staffService,
            IValidator<StaffCreateDto> createValidator,
            IValidator<StaffUpdateDto> updateValidator)
        {
            _staffService = staffService;
            _createValidator = createValidator;
            _updateValidator = updateValidator;
        }

        [HttpGet]
        public async Task<ActionResult<StaffDashBoardDto>> GetStaffs([FromQuery] StaffDashBoardRequestDto request)
        {
            var result = await _staffService.GetStaffsListAsync(request);
            return Ok(result);
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<StaffDto>> GetStaffById(int id)
        {
            var staff = await _staffService.GetStaffByIdAsync(id);
            if (staff == null)
            {
                return NotFound();
            }
            return Ok(staff);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] StaffCreateDto dto)
        {
            var validationResult = await _createValidator.ValidateAsync(dto);
            if (!validationResult.IsValid)
            {
                return BadRequest(validationResult.Errors);
            }

            var created = await _staffService.CreateAsync(dto);
            return CreatedAtAction(nameof(GetStaffById), new { id = created.StaffId }, created);
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] StaffUpdateDto dto)
        {
            var validationResult = await _updateValidator.ValidateAsync(dto);
            if (!validationResult.IsValid)
            {
                return BadRequest(validationResult.Errors);
            }

            await _staffService.UpdateStaffAsync(id, dto);
            return Ok(new { message = "Staff updated successfully" });
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            await _staffService.DeleteStaffAsync(id);
            return Ok(new { message = "Staff deleted successfully" });
        }
    }
}
