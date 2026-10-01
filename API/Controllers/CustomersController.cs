using BLL.DTOs.Customer;
using BLL.DTOs.Pagination;
using BLL.Services.Interfaces;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EDW.Api.Controllers
{
    [ApiController]
    [Route("api/customers")]
    [Authorize]
    public class CustomersController : ControllerBase
    {
        private readonly ICustomerService _customerService;
        private readonly IValidator<CustomerCreateDto> _createValidator;
        private readonly IValidator<CustomerUpdateDto> _updateValidator;

        public CustomersController(
            ICustomerService customerService,
            IValidator<CustomerCreateDto> createValidator,
            IValidator<CustomerUpdateDto> updateValidator)
        {
            _customerService = customerService;
            _createValidator = createValidator;
            _updateValidator = updateValidator;
        }

        [HttpGet]
        public async Task<ActionResult<CustomerDashBoardDto>> GetCustomers([FromQuery] CustomerDashBoardRequestDto request)
        {
            var result = await _customerService.GetCustomersAsync(request);
            return Ok(result);
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<CustomerDto>> GetCustomerById(int id)
        {
            var customer = await _customerService.GetCustomerByIdAsync(id);
            if (customer == null)
            {
                return NotFound();
            }
            return Ok(customer);
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Create([FromBody] CustomerCreateDto dto)
        {
            var validationResult = await _createValidator.ValidateAsync(dto);
            if (!validationResult.IsValid)
            {
                return BadRequest(validationResult.Errors);
            }

            var created = await _customerService.CreateAsync(dto);
            return CreatedAtAction(nameof(GetCustomerById), new { id = created.CustomerId }, created);
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] CustomerUpdateDto dto)
        {
            var validationResult = await _updateValidator.ValidateAsync(dto);
            if (!validationResult.IsValid)
            {
                return BadRequest(validationResult.Errors);
            }

            await _customerService.UpdateCustomerAsync(id, dto);
            return Ok(new { message = "Customer updated successfully" });
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            await _customerService.DeleteCustomerAsync(id);
            return Ok(new { message = "Customer deleted successfully" });
        }
    }
}
