using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebERP.Data;
using WebERP.Models;
using WebERP.DTOs;

namespace WebERP.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class CustomerController : ControllerBase
    {
        private readonly CustomerRepository repo;

        public CustomerController(CustomerRepository repo)
        {
            this.repo = repo;
        }

        [Authorize(Roles = "Admin,Employee")]
        [HttpGet]
        public IActionResult GetAll([FromQuery] int page = 1, [FromQuery] int size = 10, [FromQuery] string search = "")
        {
            var result = repo.GetCustomersPaged(page, size, search);
            return Ok(result);
        }

        [Authorize(Roles = "Admin,Employee")]
        [HttpGet("{id}")]
        public IActionResult GetById(int id)
        {
            var customer = repo.FindById(id);
            if (customer == null)
                return NotFound(new { success = false, message = "Customer not found." });
            return Ok(customer);
        }

        [Authorize(Roles = "Admin,Employee")]
        [HttpGet("find")]
        public IActionResult Find([FromQuery] string find)
        {
            try
            {
                var customers = repo.Find(find);
                return Ok(customers);
            }
            catch (System.Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public IActionResult Create([FromBody] CustomerCreateDto dto)
        {
            try
            {
                repo.AddCustomer(dto.customerId, dto.customerName, dto.city, dto.customerType);
                return Ok(new { success = true, message = "Customer added successfully!" });
            }
            catch (System.Exception ex)
            {
                return BadRequest(new { success = false, message = "The Customer ID already exists or invalid data.", error = ex.Message }); 
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpPut("{id}")]
        public IActionResult Edit(int id, [FromBody] CustomerUpdateDto dto)
        {
            try
            {
                repo.UpdateCustomer(id, dto.customerName, dto.city, dto.customerType);
                return Ok(new { success = true, message = "Customer updated successfully!" });
            }
            catch (System.Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }
        
        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            try
            {
                repo.RemoveCustomer(id);
                return Ok(new { success = true, message = "Customer deleted successfully!" });
            }
            catch (System.Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }
    }
}
