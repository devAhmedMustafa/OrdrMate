# Gahez - Restaurant Management System

Gahez is a comprehensive restaurant management system built with ASP.NET Core 8.0, designed to streamline restaurant operations including order management, table reservations, kitchen operations, and payment processing.

## 🚀 Quick Start with Docker

### Prerequisites

- [Docker](https://www.docker.com/get-started) (version 20.10 or higher)
- [Docker Compose](https://docs.docker.com/compose/install/) (version 2.0 or higher)

### Running the Application

1. **Clone the repository:**

   ```bash
   git clone <repository-url>
   cd OrdrMate
   # Swith to dev branch
   git checkout dev
   ```
2. **Start the application with Docker Compose:**

   ```bash
   docker-compose up -d
   ```

   This command will:

   - Build the OrdrMate API from the Dockerfile
   - Start a PostgreSQL database container
   - Set up networking between containers
   - Expose the API on port 5126
3. **Verify the application is running:**

   - API: http://localhost:5126
   - API Documentation (Swagger): http://localhost:5126/swagger
   - Database: localhost:5432 (accessible with credentials below)

### Docker Services

The `docker-compose.yml` defines the following services:

#### Backend Service (`backend`)

- **Image**: Built from the local Dockerfile
- **Port**: 5126 (mapped to container port 5126)
- **Environment**: Development mode with database connection
- **Volumes**:
  - `./OrdrMate` → `/app/OrdrMate` (live code reloading)
  - `./logs` → `/app/logs` (application logs)

#### Database Service (`db`)

- **Image**: PostgreSQL 15
- **Port**: 5432 (accessible externally)
- **Database**: `appdb`
- **Credentials**:
  - Username: `postgres`
  - Password: `postgres`
- **Data Persistence**: Uses Docker volume `postgres_data`

### Docker Commands

```bash
# Start all services in background
docker-compose up -d

# Start and view logs
docker-compose up

# Stop all services
docker-compose down

# Stop and remove volumes (⚠️ This will delete database data)
docker-compose down -v

# View service logs
docker-compose logs backend
docker-compose logs db

# Rebuild and restart services
docker-compose up --build

# Access backend container shell
docker-compose exec backend bash

# Access database container
docker-compose exec db psql -U postgres -d appdb
```

### Development Workflow

#### Hot Reloading

The backend service is configured with volume mounting for live code reloading:

- Changes to files in `./OrdrMate` are immediately reflected in the container
- No need to rebuild the Docker image for code changes
- The application automatically restarts when files change

#### Database Management

```bash
# Run database migrations
docker-compose exec backend dotnet ef database update

# View database tables
docker-compose exec db psql -U postgres -d appdb -c "\dt"

# Access database shell
docker-compose exec db psql -U postgres -d appdb
```

#### Viewing Logs

```bash
# Backend application logs
docker-compose logs -f backend

# Database logs
docker-compose logs -f db

# All services logs
docker-compose logs -f
```

## 🏗️ Architecture Overview

### Technology Stack

- **Backend**: ASP.NET Core 8.0 Web API
- **Database**: PostgreSQL 15
- **ORM**: Entity Framework Core
- **Authentication**: JWT with Google OAuth
- **Real-time**: WebSocket/SignalR
- **Payment**: Paymob integration
- **Storage**: AWS S3 for file uploads
- **Notifications**: Firebase Cloud Messaging

### Key Features

- **Order Management**: Complete order lifecycle with real-time updates
- **Table Reservations**: Queue management with waiting time estimates
- **Kitchen Operations**: Multi-station workflow with parallel processing
- **Payment Processing**: Secure payment gateway integration
- **User Management**: Role-based authentication (Customer, Manager, Owner, Admin)
- **Real-time Updates**: WebSocket integration for live notifications

## 🔧 Configuration

### Environment Variables

The application uses the following environment variables (configured in `docker-compose.yml`):

```yaml
environment:
  - ASPNETCORE_ENVIRONMENT=Development
  - ConnectionStrings__DefaultConnection=Host=db;Port=5432;Database=appdb;Username=postgres;Password=postgres
```

### Additional Configuration Files

- `appsettings.json` - Production configuration
- `appsettings.Development.json` - Development overrides
- `OrdrMate.http` - HTTP client test requests

## 📊 Database Schema

The application uses Entity Framework Core with code-first migrations. Key entities include:

- **Users**: Customer and staff authentication
- **Restaurants**: Restaurant information and settings
- **Branches**: Multiple locations per restaurant
- **Items**: Menu items with categories and pricing
- **Orders**: Order processing and status tracking
- **Tables**: Table management and reservations
- **Payments**: Payment processing and history
- **Kitchens**: Kitchen stations and workflow management

### Initial Setup

The database will be automatically created when the application starts. To run migrations manually:

```bash
# Apply pending migrations
docker-compose exec backend dotnet ef database update

# Create a new migration (after model changes)
docker-compose exec backend dotnet ef migrations add "MigrationName"
```

## 🔐 API Authentication

The API uses JWT authentication with the following endpoints:

- `POST /api/customer/google-login` - Google OAuth login

### Authorization Levels

- **Public**: Registration, login, payment webhooks
- **Customer**: Place orders, view order history
- **Branch Manager**: Manage branch operations, kitchen workflow
- **Restaurant Owner**: Full restaurant management
- **System Admin**: System-wide administration

## 🧪 Testing the API

### Using Swagger UI

Visit http://localhost:5126/swagger to explore and test API endpoints interactively.

### Sample API Calls

```bash
# Register a new user
curl -X POST http://localhost:5126/api/customer/register \
  -H "Content-Type: application/json" \
  -d '{"email":"user@example.com","password":"Password123","firstName":"John","lastName":"Doe"}'

# Login
curl -X POST http://localhost:5126/api/customer/login \
  -H "Content-Type: application/json" \
  -d '{"email":"user@example.com","password":"Password123"}'

# Get restaurants (requires authentication)
curl -X GET http://localhost:5126/api/restaurant \
  -H "Authorization: Bearer YOUR_JWT_TOKEN"
```

## 📝 Logs and Debugging

### Application Logs

Logs are written to the `./logs` directory and are accessible both from the host and container:

```bash
# View logs from host
tail -f ./logs/app.log

# View logs from container
docker-compose exec backend tail -f /app/logs/app.log
```

### Container Logs

```bash
# Backend application logs
docker-compose logs -f backend

# Database logs for troubleshooting
docker-compose logs -f db
```

## 📚 Documentation

- **Technical Documentation**: See `/docs` folder for detailed system architecture
- **API Documentation**: Available at http://localhost:5126/swagger when running
- **Mermaid Diagrams**: Complete system diagrams in `/docs` folder

## 🤝 Contributing

1. Fork the repository
2. Create a feature branch: `git checkout -b feature/amazing-feature`
3. Make your changes
4. Test with Docker: `docker-compose up --build`
5. Commit your changes: `git commit -m 'Add amazing feature'`
6. Push to the branch: `git push origin feature/amazing-feature`
7. Open a Pull Request

---

**OrdrMate** - Streamlining restaurant operations with modern technology 🍽️
