# ERP Website - System README

This ERP system is split across two repositories:

- `API_ERP`: ASP.NET Core Web API for authentication, authorization, sales order workflows, and inventory operations.
- `WebERP`: ASP.NET Core MVC/Razor frontend that calls the API using an httpOnly JWT cookie.

Default local URLs:

- Frontend: `https://localhost:7215`
- Backend API: `https://localhost:7284`

## Main Features

- JWT authentication stored in an httpOnly cookie.
- Role-based access for `Admin`, `Employee`, and `Customer`.
- Customer management.
- Sales order and order detail management.
- Customer order placement flow:
  - A customer can only place orders for the customer profile linked to the logged-in user.
  - A customer cannot select another customer account.
  - A customer cannot edit existing orders.
  - New customer orders are always submitted with `Pending` status.
  - The API rejects order quantities that exceed available stock.
- Inventory management:
  - Products.
  - Warehouses.
  - Stock-in and stock-out movements.
  - Product stock is synchronized through `StockMovements` and database triggers.

## Technology Stack

- .NET 8
- ASP.NET Core Web API
- SQL Server
- ADO.NET, stored procedures, and raw SQL
- JWT Bearer Authentication
- Credentialed CORS for the frontend
- Rate limiting
- CSRF origin/referer protection for state-changing requests

## Backend Structure

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

Default Development connection string:

```json
"DefaultConnection": "Server=.;Database=New_ERP;Trusted_Connection=True;TrustServerCertificate=True;"
```

Main business tables:

- `Users`
- `Customers`
- `Categories`
- `Products`
- `SalesOrders`
- `OrderDetails`
- `Warehouses`
- `StockMovements`

`DbInitializer` creates the `Users` table if it does not exist, seeds demo users, and updates the sales order paging stored procedure so customers can only see their own orders.

## Demo Accounts

| Role | Username | Password |
| --- | --- | --- |
| Admin | `admin` | `admin123` |
| Employee | `employee` | `employee123` |
| Customer | `customer1` | `customer123` |

The demo customer account is linked to `CustomerId = 1` when that customer exists in the database.

## API Endpoints

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

## Run the Backend

```bash
dotnet restore
dotnet build
dotnet run --launch-profile https
```

Swagger is available at:

```text
https://localhost:7284/swagger
```

## Run the Full System Locally

1. Start the API:

```bash
cd API_ERP
dotnet run --launch-profile https
```

2. Start the frontend:

```bash
cd WebERP
dotnet run --launch-profile https
```

3. Open the frontend:

```text
https://localhost:7215
```

## Inventory Rules

Do not update inventory by directly editing `Products.StockQty` from the frontend. Stock must be changed through `StockMovements`:

- `IN`: receives stock and increases inventory.
- `OUT`: issues stock and decreases inventory.

Stock-out movements generated from `Completed` orders have an `OrderID` and cannot be manually deleted, which preserves the audit trail.
