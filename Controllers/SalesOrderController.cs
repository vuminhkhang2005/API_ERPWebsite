using Microsoft.AspNetCore.Mvc;
using WebERP.Data;
using WebERP.Models;
using WebERP.DTOs;

namespace WebERP.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SalesOrderController : ControllerBase
    {
        private readonly SalesOrderRepository repo;

        public SalesOrderController(SalesOrderRepository repo)
        {
            this.repo = repo;
        }

        [HttpGet]
        public IActionResult GetAll([FromQuery] int page = 1, [FromQuery] int size = 10, [FromQuery] string search = "")
        {
            var result = repo.GetSalesOrdersPaged(page, size, search);
            return Ok(result);
        }

        [HttpGet("customers-dropdown")]
        public IActionResult GetCustomersDropdown()
        {
            var customers = repo.GetAllCustomersForDropdown();
            return Ok(customers);
        }

        [HttpPost]
        public IActionResult Create([FromBody] SalesOrderCreateDto dto)
        {
            try
            {
                int newOrderId = repo.AddSalesOrder(dto.CustomerId, dto.Status);
                return Ok(new { success = true, message = "Order created successfully", orderId = newOrderId });
            }
            catch (System.Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        [HttpPut("{id}")]
        public IActionResult Edit(int id, [FromBody] SalesOrderUpdateDto dto)
        {
            try
            {
                repo.UpdateSalesOrderStatus(id, dto.Status);
                repo.UpdateSalesOrderCustomer(id, dto.CustomerId);
                return Ok(new { success = true, message = "Order updated successfully" });
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
                repo.RemoveSalesOrder(id);
                return Ok(new { success = true, message = "Order deleted successfully" });
            }
            catch (System.Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        [HttpPost("full-order")]
        public IActionResult SaveFullOrder([FromBody] SaveFullOrderDto dto)
        {
            try
            {
                var jsonDetails = new System.Text.Json.Nodes.JsonArray();
                if (dto.Details != null)
                {
                    foreach (var item in dto.Details)
                    {
                        jsonDetails.Add(new System.Text.Json.Nodes.JsonObject
                        {
                            ["productId"] = item.ProductId,
                            ["quantity"] = item.Quantity
                        });
                    }
                }

                int finalOrderId = repo.SaveFullOrder(dto.OrderId, dto.CustomerId, dto.Status, jsonDetails);
                
                return Ok(new { success = true, orderId = finalOrderId });
            }
            catch (System.Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }
    }
}
