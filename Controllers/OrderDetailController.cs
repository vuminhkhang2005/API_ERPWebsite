using Microsoft.AspNetCore.Mvc;
using WebERP.Data;
using WebERP.Models;
using WebERP.DTOs;

namespace WebERP.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OrderDetailController : ControllerBase
    {
        private readonly OrderDetailRepository repo;

        public OrderDetailController(OrderDetailRepository repo)
        {
            this.repo = repo;
        }

        [HttpGet]
        public IActionResult GetByOrderId([FromQuery] int orderId)
        {
            var result = repo.GetOrderDetails(orderId);
            return Ok(result);
        }
        
        [HttpGet("products-dropdown")]
        public IActionResult GetProductsDropdown()
        {
            var result = repo.GetAllProductsForDropdown();
            return Ok(result);
        }

        [HttpPost]
        public IActionResult Create([FromBody] OrderDetailCreateDto dto)
        {
            try
            {
                repo.AddOrderDetail(dto.OrderId, dto.ProductId, dto.Quantity);
                return Ok(new { success = true, message = "Order detail added" });
            }
            catch (System.Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        [HttpPut("{id}")]
        public IActionResult Edit(int id, [FromBody] OrderDetailUpdateDto dto)
        {
            try
            {
                repo.UpdateOrderDetailQuantity(id, dto.Quantity);
                return Ok(new { success = true, message = "Order detail updated" });
            }
            catch (System.Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            try
            {
                repo.RemoveOrderDetail(id);
                return Ok(new { success = true, message = "Order detail deleted" });
            }
            catch (System.Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }
    }
}
