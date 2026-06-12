# ERP Website - System README

Hệ thống ERP gồm 2 repository:

- `API_ERP`: ASP.NET Core Web API, xử lý xác thực, phân quyền, nghiệp vụ bán hàng và kho.
- `WebERP`: ASP.NET Core MVC/Razor frontend, gọi API qua cookie JWT httpOnly.

Frontend mặc định chạy tại `https://localhost:7215`.
Backend mặc định chạy tại `https://localhost:7284`.

## Chức năng chính

- Authentication bằng JWT lưu trong httpOnly cookie.
- Phân quyền `Admin`, `Employee`, `Customer`.
- Quản lý khách hàng.
- Quản lý đơn bán hàng và chi tiết đơn hàng.
- Luồng Customer đặt hàng:
  - Customer chỉ đặt cho tài khoản khách hàng được gán với user đăng nhập.
  - Customer không chọn được khách hàng khác.
  - Customer không sửa đơn hàng đã tạo.
  - Đơn mới từ Customer luôn ở trạng thái `Pending`.
  - API chặn số lượng đặt vượt tồn kho.
- Quản lý kho:
  - Sản phẩm.
  - Kho/bãi.
  - Phiếu nhập/xuất kho.
  - Tồn kho được đồng bộ qua bảng `StockMovements` và trigger database.

## Công nghệ

- .NET 8
- ASP.NET Core Web API
- SQL Server
- ADO.NET / Stored Procedure / raw SQL
- JWT Bearer Authentication
- CORS credentials cho frontend
- Rate limiting
- CSRF origin/referer protection cho request thay đổi dữ liệu

## Cấu trúc backend

```text
Controllers/
  AuthController.cs
  CustomerController.cs
  SalesOrderController.cs
  OrderDetailController.cs
  InventoryController.cs
Data/
  DbHelper.cs
  DbInitializer.cs
  CustomerRepository.cs
  SalesOrderRepository.cs
  OrderDetailRepository.cs
  InventoryRepository.cs
DTOs/
Models/
Helpers/
Middleware/
```

## Database

Database mặc định trong môi trường Development:

```json
"DefaultConnection": "Server=.;Database=New_ERP;Trusted_Connection=True;TrustServerCertificate=True;"
```

Các bảng nghiệp vụ chính:

- `Users`
- `Customers`
- `Categories`
- `Products`
- `SalesOrders`
- `OrderDetails`
- `Warehouses`
- `StockMovements`

`DbInitializer` sẽ tạo bảng `Users` nếu chưa có, seed tài khoản demo, và cập nhật stored procedure phân trang đơn hàng để hỗ trợ Customer chỉ xem đơn của mình.

## Tài khoản demo

| Role | Username | Password |
| --- | --- | --- |
| Admin | `admin` | `admin123` |
| Employee | `employee` | `employee123` |
| Customer | `customer1` | `customer123` |

Customer demo được gán với `CustomerId = 1` nếu khách hàng này tồn tại trong database.

## API chính

### Auth

- `POST /api/Auth/login`
- `POST /api/Auth/logout`
- `GET /api/Auth/me`

### Customers

- `GET /api/Customer`
- `GET /api/Customer/{id}`
- `POST /api/Customer`
- `PUT /api/Customer/{id}`
- `DELETE /api/Customer/{id}`

### Sales Orders

- `GET /api/SalesOrder`
- `GET /api/SalesOrder/customers-dropdown`
- `POST /api/SalesOrder`
- `POST /api/SalesOrder/full-order`
- `PUT /api/SalesOrder/{id}`
- `DELETE /api/SalesOrder/{id}`

### Order Details

- `GET /api/OrderDetail?orderId={id}`
- `GET /api/OrderDetail/products-dropdown`
- `POST /api/OrderDetail`
- `PUT /api/OrderDetail/{id}`
- `DELETE /api/OrderDetail/{id}`

### Inventory

- `GET /api/Inventory/products`
- `POST /api/Inventory/products`
- `PUT /api/Inventory/products/{id}`
- `DELETE /api/Inventory/products/{id}`
- `GET /api/Inventory/categories`
- `GET /api/Inventory/warehouses`
- `POST /api/Inventory/warehouses`
- `PUT /api/Inventory/warehouses/{id}`
- `DELETE /api/Inventory/warehouses/{id}`
- `GET /api/Inventory/movements`
- `POST /api/Inventory/movements`
- `DELETE /api/Inventory/movements/{id}`

## Chạy backend

```bash
dotnet restore
dotnet build
dotnet run --launch-profile https
```

Sau khi chạy, Swagger có tại:

```text
https://localhost:7284/swagger
```

## Chạy toàn hệ thống local

1. Chạy API:

```bash
cd API_ERP
dotnet run --launch-profile https
```

2. Chạy frontend:

```bash
cd WebERP
dotnet run --launch-profile https
```

3. Mở frontend:

```text
https://localhost:7215
```

## Ghi chú nghiệp vụ kho

Không cập nhật tồn kho trực tiếp bằng cách sửa `Products.StockQty` từ frontend. Tồn kho phải đi qua `StockMovements`:

- `IN`: nhập kho, tăng tồn.
- `OUT`: xuất kho, giảm tồn.

Các phiếu xuất tự sinh từ đơn hàng đã `Completed` có `OrderID` và bị khóa xóa thủ công để giữ audit trail.
