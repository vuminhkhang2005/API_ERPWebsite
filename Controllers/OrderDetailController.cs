using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebERP.Data;
using WebERP.Models;
using WebERP.DTOs;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.DependencyInjection;

namespace WebERP.Controllers
{
    [Authorize]
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
            if (User.IsInRole("Customer"))
            {
                var claim = User.FindFirst("CustomerId")?.Value;
                if (!int.TryParse(claim, out int customerId))
                {
                    return Forbid();
                }

                // Verify the customer owns this order
                bool ownsOrder = false;
                var dbHelper = HttpContext.RequestServices.GetRequiredService<DbHelper>();
                using (var conn = dbHelper.GetConnection())
                {
                    conn.Open();
                    var cmd = new SqlCommand("SELECT COUNT(*) FROM SalesOrders WHERE OrderId = @orderId AND CustomerId = @customerId", conn);
                    cmd.Parameters.AddWithValue("@orderId", orderId);
                    cmd.Parameters.AddWithValue("@customerId", customerId);
                    ownsOrder = ((int)cmd.ExecuteScalar()) > 0;
                }

                if (!ownsOrder)
                {
                    return Forbid();
                }
            }

            var result = repo.GetOrderDetails(orderId);
            return Ok(result);
        }
        
        [HttpGet("products-dropdown")]
        public IActionResult GetProductsDropdown()
        {
            var result = repo.GetAllProductsForDropdown();
            return Ok(result);
        }

        [Authorize(Roles = "Admin,Employee")]
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

        [Authorize(Roles = "Admin,Employee")]
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

        [Authorize(Roles = "Admin,Employee")]
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
