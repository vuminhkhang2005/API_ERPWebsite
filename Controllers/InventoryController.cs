using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebERP.Data;
using WebERP.DTOs;

namespace WebERP.Controllers
{
    [Authorize(Roles = "Admin,Employee")]
    [ApiController]
    [Route("api/[controller]")]
    public class InventoryController : ControllerBase
    {
        private readonly InventoryRepository repo;

        public InventoryController(InventoryRepository repo)
        {
            this.repo = repo;
        }

        [HttpGet("products")]
        public IActionResult GetProducts([FromQuery] int page = 1, [FromQuery] int size = 10, [FromQuery] string search = "")
        {
            return Ok(repo.GetProductsPaged(page, size, search));
        }

        [HttpPost("products")]
        public IActionResult CreateProduct([FromBody] ProductCreateDto dto)
        {
            try
            {
                int productId = repo.AddProduct(dto.ProductName, dto.CategoryId, dto.UnitPrice);
                return Ok(new { success = true, productId, message = "Product created successfully." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        [HttpPut("products/{id}")]
        public IActionResult UpdateProduct(int id, [FromBody] ProductUpdateDto dto)
        {
            try
            {
                repo.UpdateProduct(id, dto.ProductName, dto.CategoryId, dto.UnitPrice);
                return Ok(new { success = true, message = "Product updated successfully." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        [HttpDelete("products/{id}")]
        public IActionResult DeleteProduct(int id)
        {
            try
            {
                repo.DeleteProduct(id);
                return Ok(new { success = true, message = "Product deleted successfully." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        [HttpGet("warehouses")]
        public IActionResult GetWarehouses()
        {
            return Ok(repo.GetWarehouses());
        }

        [HttpPost("warehouses")]
        public IActionResult CreateWarehouse([FromBody] WarehouseCreateDto dto)
        {
            try
            {
                int warehouseId = repo.AddWarehouse(dto.WarehouseName, dto.Location);
                return Ok(new { success = true, warehouseId, message = "Warehouse created successfully." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        [HttpPut("warehouses/{id}")]
        public IActionResult UpdateWarehouse(int id, [FromBody] WarehouseUpdateDto dto)
        {
            try
            {
                repo.UpdateWarehouse(id, dto.WarehouseName, dto.Location);
                return Ok(new { success = true, message = "Warehouse updated successfully." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        [HttpDelete("warehouses/{id}")]
        public IActionResult DeleteWarehouse(int id)
        {
            try
            {
                repo.DeleteWarehouse(id);
                return Ok(new { success = true, message = "Warehouse deleted successfully." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        [HttpGet("categories")]
        public IActionResult GetCategories()
        {
            return Ok(repo.GetCategories());
        }

        [HttpGet("movements")]
        public IActionResult GetMovements([FromQuery] int page = 1, [FromQuery] int size = 10, [FromQuery] string search = "")
        {
            return Ok(repo.GetMovementsPaged(page, size, search));
        }

        [HttpPost("movements")]
        public IActionResult CreateMovement([FromBody] StockMovementCreateDto dto)
        {
            try
            {
                int movementId = repo.AddStockMovement(dto.ProductId, dto.WarehouseId, dto.MovementType, dto.Quantity);
                return Ok(new { success = true, movementId, message = "Stock movement created successfully." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        [HttpDelete("movements/{id}")]
        public IActionResult DeleteMovement(int id)
        {
            try
            {
                repo.DeleteStockMovement(id);
                return Ok(new { success = true, message = "Stock movement deleted successfully." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }
    }
}
