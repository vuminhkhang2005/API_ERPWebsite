using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebERP.Data;
using WebERP.Models;
using WebERP.DTOs;
using System.Security.Claims;

namespace WebERP.Controllers
{
    [Authorize]
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
            int? customerId = null;
            if (User.IsInRole("Customer"))
            {
                var claim = User.FindFirst("CustomerId")?.Value;
                if (int.TryParse(claim, out int parsed))
                {
                    customerId = parsed;
                }
                else
                {
                    return Ok(new { last_page = 1, data = Array.Empty<SalesOrder>() });
                }
            }

            var result = repo.GetSalesOrdersPaged(page, size, search, customerId);
            return Ok(result);
        }

        [Authorize(Roles = "Admin,Employee")]
        [HttpGet("customers-dropdown")]
        public IActionResult GetCustomersDropdown()
        {
            var customers = repo.GetAllCustomersForDropdown();
            return Ok(customers);
        }

        [HttpPost]
        public IActionResult Create([FromBody] SalesOrderCreateDto dto)
        {
            if (User.IsInRole("Customer"))
            {
                var claim = User.FindFirst("CustomerId")?.Value;
                if (!int.TryParse(claim, out int customerId) || dto.CustomerId != customerId)
                {
                    return Forbid();
                }

                dto.Status = "Pending";
            }

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

        [Authorize(Roles = "Admin,Employee")]
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

        [Authorize(Roles = "Admin")]
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
            if (User.IsInRole("Customer"))
            {
                var claim = User.FindFirst("CustomerId")?.Value;
                if (!int.TryParse(claim, out int customerId) || dto.CustomerId != customerId)
                {
                    return Forbid();
                }

                // If customer is editing an order (orderId > 0), verify they own the order
                if (dto.OrderId > 0)
                {
                    // Verify that the existing order belongs to the customer
                    // We can check this by fetching the order or using a simple check.
                    // Let's implement a check to verify order customer matches.
                    // Wait, we can quickly check if the order exists and has this customerId.
                    // For simplicity, we can trust the dto.CustomerId because the backend's SaveFullOrder will match the DB order's CustomerId with dto.CustomerId anyway or overwrite it, but to prevent cross-customer editing, let's verify!
                    // Wait, can a customer edit an order? In our RBAC rules, customer CANNOT edit existing orders (PUT/edit is restricted to Admin/Employee). But customer can submit a new full order (orderId = 0).
                    // So if dto.OrderId > 0, we should block it for Customer!
                    return BadRequest(new { success = false, message = "Customers are not allowed to modify existing orders." });
                }

                if (dto.Details == null || dto.Details.Count == 0)
                {
                    return BadRequest(new { success = false, message = "Order must contain at least one product." });
                }

                if (dto.Details.Any(d => d.ProductId <= 0 || d.Quantity <= 0))
                {
                    return BadRequest(new { success = false, message = "Order details contain invalid product or quantity." });
                }

                try
                {
                    repo.EnsureProductsAvailable(dto.Details);
                }
                catch (System.Exception ex)
                {
                    return BadRequest(new { success = false, message = ex.Message });
                }

                dto.Status = "Pending";
            }

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
