<div align="center">

# 🎯 Attendr

### Modern Conference Management Platform

*Empowering conference attendees and organizers to connect, organize, and collaborate*

[![.NET](https://img.shields.io/badge/.NET-10.0-512BD4?style=for-the-badge&logo=dotnet)](https://dotnet.microsoft.com/)
[![Angular](https://img.shields.io/badge/Angular-19-DD0031?style=for-the-badge&logo=angular)](https://angular.io/)
[![PrimeNG](https://img.shields.io/badge/PrimeNG-19-007ACC?style=for-the-badge)](https://primeng.org/)
[![License](https://img.shields.io/badge/License-MIT-green.svg?style=for-the-badge)](LICENSE)

[Features](#-features) • [Tech Stack](#-tech-stack) • [Architecture](#-architecture) • [Getting Started](#-getting-started) • [Contributing](#-contributing)

</div>

---

## 📖 About

Attendr is a comprehensive conference management platform designed to enhance the experience of both conference attendees and organizers. Browse sessions, join groups, manage your profile, and connect with fellow attendees—all in one elegant, modern interface.

## ✨ Features

### 👥 Group Management
- **Browse & Search** - Discover groups with powerful search and pagination
- **Join Groups** - One-click joining with instant membership status
- **Create Groups** - Start your own communities within conferences
- **Member Tracking** - See member counts and your membership status at a glance

### 📅 Conference Organization
- **Session Management** - Browse and organize conference sessions
- **Agenda Planning** - Build your personalized conference schedule
- **Real-time Updates** - Stay informed with live session updates

### 👤 Profile Management
- **User Profiles** - Comprehensive attendee profiles
- **OIDC Authentication** - Secure authentication with OpenID Connect
- **Profile Integration** - Seamless integration across all modules

### 🎨 Modern UI/UX
- **Responsive Design** - Beautiful interface that works on any device
- **Real-time Feedback** - Toast notifications for all user actions
- **Modal Dialogs** - Intuitive interactions with PrimeNG dialogs
- **Loading States** - Clear feedback during data operations

## 🚀 Tech Stack

### Frontend
- **Framework**: Angular 19 (Standalone Components)
- **UI Library**: PrimeNG 19 with Aura Theme
- **State Management**: Angular Signals
- **HTTP Client**: Angular HttpClient with retry interceptors
- **Authentication**: angular-auth-oidc-client
- **Styling**: SCSS with modern CSS features

### Backend
- **.NET 10.0** - Latest .NET framework
- **Clean Architecture** - DDD principles with CQRS pattern
- **Microservices** - Modular service architecture
  - Groups Service
  - Profiles Service
  - Conferences Service
  - Proxy API Gateway
- **Data Persistence**: 
  - MongoDB (Conferences)
  - Azure Table Storage (Profiles, Groups)
  - In-memory repositories for development
- **Observability**: OpenTelemetry integration

## 🏗️ Architecture

### Project Structure

```
attendr/
├── src/
│   ├── App/                          # Angular Frontend
│   │   ├── src/
│   │   │   ├── app/
│   │   │   │   ├── auth/            # Authentication
│   │   │   │   ├── pages/           # Page components
│   │   │   │   ├── shared/          # Shared components, services, stores
│   │   │   │   └── templates/       # Layout templates
│   │   │   └── environments/        # Environment configurations
│   │
│   ├── Conferences/                  # Conferences Microservice
│   │   ├── HexMaster.Attendr.Conferences/
│   │   ├── HexMaster.Attendr.Conferences.Api/
│   │   ├── HexMaster.Attendr.Conferences.Abstractions/
│   │   ├── HexMaster.Attendr.Conferences.Data.MongoDb/
│   │   └── HexMaster.Attendr.Conferences.Tests/
│   │
│   ├── Groups/                       # Groups Microservice
│   │   ├── HexMaster.Attendr.Groups/
│   │   ├── HexMaster.Attendr.Groups.Api/
│   │   ├── HexMaster.Attendr.Groups.Abstractions/
│   │   ├── HexMaster.Attendr.Groups.Data.TableStorage/
│   │   └── HexMaster.Attendr.Groups.Tests/
│   │
│   ├── Profiles/                     # Profiles Microservice
│   │   ├── HexMaster.Attendr.Profiles/
│   │   ├── HexMaster.Attendr.Profiles.Api/
│   │   ├── HexMaster.Attendr.Profiles.Abstractions/
│   │   ├── HexMaster.Attendr.Profiles.Data.TableStorage/
│   │   ├── HexMaster.Attendr.Profiles.Integrations/
│   │   └── HexMaster.Attendr.Profiles.Tests/
│   │
│   ├── Shared/                       # Shared Libraries
│   │   └── HexMaster.Attendr.Core/  # Core domain models, constants
│   │
│   └── HexMaster.Attendr.Proxy.Api/  # API Gateway
```

### Design Patterns

- **Domain-Driven Design (DDD)** - Aggregate roots, value objects, domain events
- **CQRS** - Command Query Responsibility Segregation with handlers
- **Repository Pattern** - Abstracted data access layer
- **Clean Architecture** - Separation of concerns with clear boundaries
- **Reactive Programming** - RxJS observables for data streams

### Key Components

#### Backend
- **Aggregate Roots**: Group, Profile, Conference
- **Query Handlers**: Encapsulated query logic with dependency injection
- **Command Handlers**: Transactional command processing
- **API Endpoints**: Minimal API with endpoint mapping
- **Pagination Constants**: Centralized pagination configuration (default: 25, max: 100)

#### Frontend
- **Services**: HTTP communication layer
- **Stores**: Signal-based state management
- **Components**: Standalone, reusable UI components
- **Interceptors**: Retry logic with exponential backoff and error handling

## 🎯 Getting Started

### Prerequisites

- [.NET 10.0 SDK](https://dotnet.microsoft.com/download)
- [Node.js 20+](https://nodejs.org/)
- [Angular CLI 19+](https://angular.io/cli)

### Installation

1. **Clone the repository**
   ```bash
   git clone https://github.com/nikneem/attendr.git
   cd attendr
   ```

2. **Backend Setup**
   ```bash
   cd src
   dotnet restore
   dotnet build
   ```

3. **Frontend Setup**
   ```bash
   cd src/App
   npm install
   ```

### Running the Application

#### Backend Services
```bash
# Build all services
dotnet build src/Attendr.slnx

# Run specific service (e.g., Groups API)
cd src/Groups/HexMaster.Attendr.Groups.Api
dotnet run

# Run all tests
dotnet test src/Attendr.slnx
```

#### Frontend
```bash
cd src/App

# Development server
npm start
# Navigate to http://localhost:4200

# Build for production
npm run build

# Run tests
npm test
```

### Available Tasks (VS Code)

- **Build All** - Builds both backend and frontend
- **Test All** - Runs all unit tests
- **Build Backend (.NET)** - Builds .NET solution
- **Test Backend (.NET)** - Runs .NET tests
- **Build Frontend (Angular)** - Builds Angular application
- **Test Frontend (Angular)** - Runs Angular tests
- **Serve Frontend (Angular)** - Starts dev server

## 🔧 Configuration

### Environment Variables

Create environment files in `src/App/src/environments/`:

```typescript
// environment.development.ts
export const environment = {
  production: false,
  apiUrl: 'https://localhost:7001',
  authConfig: {
    // OIDC configuration
  }
};
```

### Backend Configuration

Configure `appsettings.json` in each API project:

```json
{
  "ConnectionStrings": {
    "MongoDB": "mongodb://localhost:27017",
    "TableStorage": "UseDevelopmentStorage=true"
  },
  "Attendr": {
    "Cache": {
      "Enabled": true
    }
  }
}
```

## 📊 API Documentation

The API follows RESTful conventions with the following endpoints:

### Groups API
- `GET /api/groups` - List all groups (paginated, searchable)
- `GET /api/groups/my-groups` - Get current user's groups
- `POST /api/groups` - Create a new group

### Profiles API
- `GET /api/profiles/{id}` - Get profile by ID
- `GET /api/profiles/subject/{subjectId}` - Get profile by subject ID
- `POST /api/profiles` - Create a new profile
- `PUT /api/profiles/{id}` - Update profile

## 🔍 Observability

The application includes comprehensive observability features:

- **OpenTelemetry Integration** - Distributed tracing across services
- **Activity Sources** - Dedicated sources for Profiles, Groups, and Conferences
- **Structured Logging** - Consistent logging patterns
- **Error Tracking** - HTTP interceptors with retry logic and error notifications

See [OBSERVABILITY_SETUP.md](OBSERVABILITY_SETUP.md) and [OPENTELEMETRY.md](OPENTELEMETRY.md) for detailed setup instructions.

## 🧪 Testing

### Backend Tests
```bash
# Run all tests
dotnet test src/Attendr.slnx

# Run specific project tests
dotnet test src/Groups/HexMaster.Attendr.Groups.Tests
```

### Frontend Tests
```bash
cd src/App

# Unit tests
npm test

# E2E tests (if configured)
npm run e2e
```

## 🤝 Contributing

Contributions are welcome! Please follow these steps:

1. **Fork the repository**
2. **Create a feature branch** (`git checkout -b feature/amazing-feature`)
3. **Commit your changes** (`git commit -m 'Add some amazing feature'`)
4. **Push to the branch** (`git push origin feature/amazing-feature`)
5. **Open a Pull Request**

### Code Style

- **Backend**: Follow C# coding conventions, use XML documentation
- **Frontend**: Follow Angular style guide, use ESLint
- **Commits**: Use conventional commits format

## 📝 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## 👨‍💻 Author

**Edward van Kuik (Nikneem)**

- GitHub: [@nikneem](https://github.com/nikneem)

## 🙏 Acknowledgments

- Built with [Angular](https://angular.io/) and [.NET](https://dotnet.microsoft.com/)
- UI components by [PrimeNG](https://primeng.org/)
- Icons from [PrimeIcons](https://primeng.org/icons)

---

<div align="center">

**[⬆ back to top](#-attendr)**

Made with ❤️ by the Attendr team

</div>
