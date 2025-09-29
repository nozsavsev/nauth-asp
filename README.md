# NAUTH - Backend

<div align="center">
  <img src="./readme-image/email_banner.svg" alt="NAUTH Banner" />
</div>

A modern, secure, and feature-rich authentication and authorization microservice designed to be the backbone of nozsa.com security.

This is the backend API for the NAUTH service, built with ASP.NET Core and Entity Framework Core.

## Features

- **Core Authentication**: Robust and secure authentication suite, featuring Two-Factor Authentication (2FA), seamless Google Login integration, and passwordless login with temporary codes. User credentials are protected using the strong Argon2id hashing algorithm.
- **Advanced Session Management**: All sessions are validated against the database for enhanced security, with the ability to instantly revoke any session in real-time.
- **Authorization & Permissions**: A flexible and powerful permission system allows for fine-grained access control. Nauth offers centralized permission management and can authenticate other applications on the same domain, storing and managing permissions across all services.
- **Comprehensive User Management**: Administrators have access to a comprehensive set of tools to effectively manage users, including the ability to delete accounts, set passwords, and trigger critical email actions like verification and password resets.
- **Customizable Transactional Emails**: Nauth includes a built-in email template system for all transactional emails. Anyone with the right permissions can customize templates for various actions, ensuring consistent branding and communication.
- **Extensible Service Integration**: Designed for extensibility, Nauth allows authorized users (admins only) to register other applications on the domain to use nauth as an authentication system. These services can then register their own permissions in real-time.

## Getting Started

### Prerequisites

- .NET 9.0 SDK
- SQL Server or compatible database
- Entity Framework Core tools

### Installation

1. Clone the repository
2. Navigate to the `nauth-asp` directory
3. Restore dependencies:

```bash
dotnet restore
```

4. Update the connection string in `appsettings.Development.json`
5. Run database migrations:

```bash
dotnet ef database update
```

6. Run the application:

```bash
dotnet run
```

The API will be available at `https://localhost:5001` (or the port configured in `launchSettings.json`).

## Configuration

### Environment Variables

refer to `appsettings.Development.json.example` for the configuration.

### Database

The application uses Entity Framework Core with PostgreSQL. Database migrations are located in the `Migrations` folder.

## API Endpoints

### Authentication
- `POST /api/auth/login` - User login
- `POST /api/auth/register` - User registration
- `POST /api/auth/logout` - User logout
- `POST /api/auth/refresh` - Refresh JWT token
- `POST /api/auth/2fa/enable` - Enable 2FA
- `POST /api/auth/2fa/verify` - Verify 2FA code

### User Management
- `GET /api/users` - Get all users (admin only)
- `GET /api/users/{id}` - Get user by ID
- `PUT /api/users/{id}` - Update user
- `DELETE /api/users/{id}` - Delete user (admin only)

### Services
- `GET /api/services` - Get all services
- `POST /api/services` - Create new service (admin only)
- `PUT /api/services/{id}` - Update service
- `DELETE /api/services/{id}` - Delete service (admin only)

### Permissions
- `GET /api/permissions` - Get all permissions
- `POST /api/permissions` - Create permission (admin only)
- `PUT /api/permissions/{id}` - Update permission
- `DELETE /api/permissions/{id}` - Delete permission (admin only)

### Building for Production

```bash
dotnet publish -c Release -o ./publish
```

## Docker

The application includes a Dockerfile for containerized deployment:

```bash
docker build -t nauth-backend .
docker run -p 5001:5001 nauth-backend
```

## Contributing

1. Fork the repository
2. Create a feature branch
3. Make your changes
4. Add tests if applicable
5. Submit a pull request

## License

This project is licensed under the MIT License.
