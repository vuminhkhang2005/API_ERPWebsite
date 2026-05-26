# API ERP Website

This is a RESTful API built with ASP.NET Core for an Enterprise Resource Planning (ERP) system. 

## Features

* **Customer Management:** Endpoints for managing customer data.
* **Sales Orders:** Endpoints for handling sales orders.
* **Order Details:** Endpoints for line items inside sales orders.
* **Data Access:** Utilizes ADO.NET / Stored Procedures for database operations.

## Technology Stack

* C#
* ASP.NET Core Web API
* SQL Server (via ADO.NET)

## Getting Started

1. Clone the repository:
   ```bash
   git clone https://github.com/vuminhkhang2005/API_ERPWebsite.git
   ```
2. Open the project in Visual Studio or your preferred IDE.
3. Restore the NuGet packages.
4. Update the database connection string in `appsettings.json` if necessary.
5. Run the project. The API will be available at `https://localhost:<port>/`.

## License

This project is open-source and available under the MIT License.
