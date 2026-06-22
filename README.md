# House Booking REST API

A comprehensive RESTful API for managing house rentals, bookings, and user authentication built with ASP.NET Core 10.0.

[![.NET Version](https://img.shields.io/badge/.NET-10.0-512BD4)](https://dotnet.microsoft.com/)
[![License](https://img.shields.io/badge/license-MIT-blue.svg)](LICENSE)

## 📋 Table of Contents

- [Features](#-features)
- [Technology Stack](#-technology-stack)
- [Architecture](#-architecture)
- [Getting Started](#-getting-started)
- [Configuration](#-configuration)
- [API Endpoints](#-api-endpoints)
- [Authentication](#-authentication)
- [Database Schema](#-database-schema)
- [Testing](#-testing)
- [Project Structure](#-project-structure)
- [Contributing](#-contributing)

## ✨ Features

- **User Management**
  - User registration and authentication with JWT
  - Role-based access control (Admin, Owner, Renter)
  - Secure password hashing with BCrypt

- **House Management**
  - Create, read, update, and delete house listings
  - Image upload and management via Cloudinary
  - Pagination and filtering support
  - Owner-specific house management

- **Booking System**
  - Create and manage bookings
  - Date range validation
  - Booking overlap prevention
  - Renter and owner booking views
  - Cancel bookings with check-in date validation

- **Additional Features**
  - RESTful API design
  - Comprehensive error handling
  - Logging with Serilog
  - API documentation with Swagger/OpenAPI
  - Unit tested (158 test cases covering repositories, services, and controllers)

## 🛠 Technology Stack

### Core Framework
- **.NET 10.0** - Latest .NET framework
- **ASP.NET Core** - Web API framework
- **Entity Framework Core 10.0** - ORM for database operations

### Database
- **SQL Server** - Primary database

### Authentication & Security
- **JWT Bearer Authentication** - Token-based authentication
- **BCrypt.Net** - Password hashing

### Cloud Services
- **Cloudinary** - Image storage and management

### Testing
- **xUnit** - Testing framework
- **NSubstitute** - Mocking framework
- **AutoMapper** - Object mapping for tests

### Utilities
- **AutoMapper 16.1.1** - Object-to-object mapping
- **Serilog** - Structured logging
- **Swashbuckle** - API documentation
- **dotenv.net** - Environment variable management

## 🏗 Architecture

The project follows a **clean architecture** pattern with clear separation of concerns:

```
HouseBookingRestApi/
├── Controllers/          # API endpoints
├── Services/            # Business logic layer
├── Repositories/        # Data access layer
├── Models/              # Domain entities
├── DTO/                 # Data Transfer Objects
├── Data/                # DbContext and configurations
├── Migrations/          # EF Core migrations
├── Security/            # Authentication utilities
├── Helpers/             # Helper classes and extensions
├── Exceptions/          # Custom exception classes
└── Configuration/       # App configuration

HouseBookingRestApi.Tests/
├── Controllers/         # Controller unit tests
├── Services/           # Service unit tests
└── Repositories/       # Repository unit tests
```

### Design Patterns Used
- **Repository Pattern** - Abstracts data access
- **Unit of Work Pattern** - Manages transactions
- **Dependency Injection** - Loose coupling
- **DTO Pattern** - Separates domain models from API contracts

## 🚀 Getting Started

### Prerequisites

- [.NET 10.0 SDK](https://dotnet.microsoft.com/download/dotnet/10.0) or later
- [SQL Server](https://www.microsoft.com/sql-server/sql-server-downloads) (LocalDB, Express, or full version)
- [Visual Studio 2026](https://visualstudio.microsoft.com/) or [VS Code](https://code.visualstudio.com/)
- [Cloudinary Account](https://cloudinary.com/) (for image storage)

### Installation

1. **Clone the repository**
   ```bash
   git clone https://github.com/orestisLyk/HouseBookingRestAPI.git
   cd HouseBookingRestAPI
   ```

2. **Set up environment variables**

   Create a `.env` file in the project root:
   ```env
   CLOUDINARY_URL=cloudinary://your_api_key:your_api_secret@your_cloud_name
   ```

3. **Configure the database connection**

   Update `appsettings.json` or create `appsettings.Development.json`:
   ```json
   {
	 "ConnectionStrings": {
	   "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=HouseBookingDb;Trusted_Connection=true;TrustServerCertificate=true"
	 },
	 "Jwt": {
	   "Key": "YourSuperSecretKeyHere_MustBeAtLeast32CharactersLong!",
	   "Issuer": "HouseBookingAPI",
	   "Audience": "HouseBookingAPIUsers",
	   "ExpirationMinutes": 60
	 }
   }
   ```

4. **Apply database migrations**
   ```bash
   cd HouseBookingRestApi
   dotnet ef database update
   ```

5. **Run the application**
   ```bash
   dotnet run
   ```

6. **Access the API**
   - API: `https://localhost:5001` or `http://localhost:5000`
   - Swagger UI: `https://localhost:5001/swagger`

## ⚙ Configuration

### JWT Configuration

Configure JWT settings in `appsettings.json`:

```json
{
  "Jwt": {
	"Key": "Your-Secret-Key-Min-32-Chars",
	"Issuer": "HouseBookingAPI",
	"Audience": "HouseBookingAPIUsers",
	"ExpirationMinutes": 60
  }
}
```

### Cloudinary Configuration

Set up Cloudinary for image storage:

1. Sign up at [Cloudinary](https://cloudinary.com/)
2. Get your API credentials from the dashboard
3. Add to `.env` file:
   ```
   CLOUDINARY_URL=cloudinary://api_key:api_secret@cloud_name
   ```

### Database Configuration

The application uses SQL Server. Update the connection string for your environment:

- **LocalDB** (Development): `Server=(localdb)\\mssqllocaldb;Database=HouseBookingDb;Trusted_Connection=true`
- **SQL Server Express**: `Server=localhost\\SQLEXPRESS;Database=HouseBookingDb;Trusted_Connection=true`
- **Azure SQL**: Use connection string from Azure portal

## 📚 API Endpoints

### Authentication (`/api/v1/auth`)

| Method | Endpoint | Description | Auth Required |
|--------|----------|-------------|---------------|
| POST | `/auth/register` | Register a new user | No |
| POST | `/auth/login` | Login and receive JWT token | No |

### Users (`/api/v1/users`)

| Method | Endpoint | Description | Auth Required |
|--------|----------|-------------|---------------|
| GET | `/users/{username}` | Get user profile | Yes (Self or Admin) |
| GET | `/users?page=1&size=10` | Get paginated users | Yes (Admin) |
| DELETE | `/users/{id}` | Delete user | Yes (Admin) |

### Houses (`/api/v1/houses`)

| Method | Endpoint | Description | Auth Required |
|--------|----------|-------------|---------------|
| GET | `/houses/{id}` | Get house by ID | No |
| GET | `/houses?page=1&size=10` | Get paginated houses | No |
| GET | `/houses/by-owner/{ownerId}` | Get houses by owner | No |
| POST | `/houses` | Create new house | Yes (Owner) |
| DELETE | `/houses/{id}` | Delete house | Yes (Owner/Admin) |

### House Images (`/api/v1/house-images`)

| Method | Endpoint | Description | Auth Required |
|--------|----------|-------------|---------------|
| POST | `/house-images` | Upload house image | Yes (Owner) |
| GET | `/house-images/by-house/{id}` | Get images by house | No |
| GET | `/house-images/{id}` | Get image by ID | No |
| DELETE | `/house-images/{id}` | Delete image | Yes (Owner) |

### Bookings (`/api/v1/bookings`)

| Method | Endpoint | Description | Auth Required |
|--------|----------|-------------|---------------|
| GET | `/bookings/{id}` | Get booking by ID | Yes (Renter/Owner/Admin) |
| GET | `/bookings/by-renter/{renterId}` | Get bookings by renter | Yes (Renter/Admin) |
| GET | `/bookings/by-house/{houseId}` | Get bookings by house | Yes (Owner/Admin) |
| POST | `/bookings` | Create booking | Yes (Renter) |
| DELETE | `/bookings/{id}` | Cancel booking | Yes (Renter/Owner/Admin) |

### Roles (`/api/v1/roles`)

| Method | Endpoint | Description | Auth Required |
|--------|----------|-------------|---------------|
| GET | `/roles` | Get all roles | No |

## 🔐 Authentication

The API uses **JWT Bearer Token** authentication.

### Getting a Token

1. **Register a new user:**
   ```bash
   POST /api/v1/auth/register
   {
	 "username": "johndoe",
	 "password": "SecurePass123!",
	 "email": "john@example.com",
	 "firstName": "John",
	 "lastName": "Doe",
	 "roleId": 3
   }
   ```

2. **Login:**
   ```bash
   POST /api/v1/auth/login
   {
	 "username": "johndoe",
	 "password": "SecurePass123!"
   }
   ```

   Response:
   ```json
   {
	 "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."
   }
   ```

3. **Use the token:**
   ```bash
   Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
   ```

### Roles

- **Admin (1)**: Full system access
- **Owner (2)**: Manage houses and view bookings
- **Renter (3)**: Create bookings and view own bookings

## 🗄 Database Schema

### Core Entities

- **Users** - User accounts with authentication
- **Roles** - User roles (Admin, Owner, Renter)
- **Owners** - Extended user info for property owners
- **Renters** - Extended user info for renters
- **Houses** - Property listings
- **HouseImages** - Property images (stored on Cloudinary)
- **Bookings** - Rental bookings
- **Capabilities** - House features/amenities

### Relationships

- User → Role (Many-to-One)
- User → Owner (One-to-One)
- User → Renter (One-to-One)
- Owner → Houses (One-to-Many)
- House → HouseImages (One-to-Many)
- House → Bookings (One-to-Many)
- Renter → Bookings (One-to-Many)
- House ↔ Capabilities (Many-to-Many)

## 🧪 Testing

The project includes **158 comprehensive unit tests** covering:
- Repository layer (42 tests)
- Service layer (56 tests)
- Controller layer (60 tests)

### Running Tests

```bash
# Run all tests
dotnet test

# Run tests with detailed output
dotnet test --logger "console;verbosity=detailed"

# Run tests for a specific project
dotnet test HouseBookingRestApi.Tests/HouseBookingRestApi.Tests.csproj

# Run tests with code coverage
dotnet test /p:CollectCoverage=true
```

### Test Coverage

- ✅ **Repositories**: CRUD operations, pagination, filtering, soft deletes
- ✅ **Services**: Business logic, validation, authorization, error handling
- ✅ **Controllers**: HTTP responses, authentication, role-based access

## 📁 Project Structure

```
HouseBookingRestApi/
│
├── Controllers/
│   ├── AuthController.cs           # Authentication endpoints
│   ├── BookingController.cs        # Booking management
│   ├── HouseController.cs          # House management
│   ├── HouseImageController.cs     # Image management
│   ├── RoleController.cs           # Role management
│   └── UserController.cs           # User management
│
├── Services/
│   ├── IBookingService.cs
│   ├── BookingService.cs
│   ├── IHouseService.cs
│   ├── HouseService.cs
│   ├── IHouseImageService.cs
│   ├── HouseImageService.cs
│   ├── IUserService.cs
│   ├── UserService.cs
│   ├── IRoleService.cs
│   └── RoleService.cs
│
├── Repositories/
│   ├── IUnitOfWork.cs
│   ├── UnitOfWork.cs
│   ├── IBookingRepository.cs
│   ├── BookingRepository.cs
│   ├── IHouseRepository.cs
│   ├── HouseRepository.cs
│   └── ... (other repositories)
│
├── Models/
│   ├── User.cs
│   ├── Role.cs
│   ├── Owner.cs
│   ├── Renter.cs
│   ├── House.cs
│   ├── HouseImage.cs
│   ├── Booking.cs
│   └── Capability.cs
│
├── DTO/
│   ├── UserRegisterDTO.cs
│   ├── UserLoginDTO.cs
│   ├── JwtTokenDTO.cs
│   ├── HouseRegisterDTO.cs
│   ├── HouseReadOnlyDTO.cs
│   ├── BookingRegisterDTO.cs
│   └── ... (other DTOs)
│
├── Data/
│   └── HouseBookingRestApiContext.cs
│
├── Migrations/
│   ├── 20260614103346_InitialCreate.cs
│   └── 20260621093455_ChangePricePerNightToDecimal.cs
│
├── Security/
│   ├── IEncryptionUtil.cs
│   └── EncryptionUtil.cs
│
├── Helpers/
│   └── GlobalExceptionHandler.cs
│
├── Exceptions/
│   ├── EntityNotFoundException.cs
│   ├── EntityForbiddenException.cs
│   ├── InvalidCredentialsException.cs
│   └── ... (other custom exceptions)
│
└── Core/
	└── PaginatedResult.cs

HouseBookingRestApi.Tests/
├── Controllers/
│   ├── AuthControllerTests.cs
│   ├── BookingControllerTests.cs
│   ├── HouseControllerTests.cs
│   ├── HouseImageControllerTests.cs
│   ├── RoleControllerTests.cs
│   └── UserControllerTests.cs
│
├── Services/
│   ├── BookingServiceTests.cs
│   ├── HouseServiceTests.cs
│   ├── HouseImageServiceTests.cs
│   ├── RoleServiceTests.cs
│   └── UserServiceTests.cs
│
└── Repositories/
	├── BookingRepositoryTests.cs
	├── HouseRepositoryTests.cs
	├── HouseImageRepositoryTests.cs
	├── OwnerRepositoryTests.cs
	├── RenterRepositoryTests.cs
	├── RoleRepositoryTests.cs
	└── UserRepositoryTests.cs
```

## 🤝 Contributing

Contributions are welcome! Please follow these steps:

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/AmazingFeature`)
3. Commit your changes (`git commit -m 'Add some AmazingFeature'`)
4. Push to the branch (`git push origin feature/AmazingFeature`)
5. Open a Pull Request

### Coding Standards

- Follow C# coding conventions
- Write unit tests for new features
- Update documentation as needed
- Ensure all tests pass before submitting PR

## 📝 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## 👥 Authors

- **orestisLyk** - *Initial work* - [GitHub](https://github.com/orestisLyk)

## 🙏 Acknowledgments

- ASP.NET Core team for the excellent framework
- Entity Framework Core for powerful ORM capabilities
- Cloudinary for image storage solution
- The open-source community for various packages and tools

## 📧 Contact

For questions or suggestions, please open an issue on GitHub.

---

**Built with ❤️ using .NET 10.0**
