# Attendr Documentation

Welcome to the Attendr project documentation. This folder contains all technical and architectural documentation for the Attendr conference management platform.

## Documentation Structure

### 📁 Technical Documentation (`/technical`)

Technical implementation guides and patterns:

- **[Stateful Domain Model](technical/stateful-domain-model.md)** - Base class for domain models with state tracking
- **[Integration Events](technical/integration-events.md)** - Event publishing capabilities using Dapr pub/sub

### 📁 Architecture Documentation (`/architecture`)

High-level architecture decisions and system design:

- Architecture Decision Records (ADRs)
- System architecture diagrams
- Service boundaries and interactions
- Design patterns and principles

### 📁 Development Guides (`/development`)

Developer-focused guides and workflows:

- **[Angular Development Guidelines](development/angular-guidelines.md)** - ⚠️ **REQUIRED READING** - Zoneless Angular patterns and best practices
- Setup and installation instructions
- Development workflows
- Testing strategies
- Deployment guides
- Contributing guidelines

### 📁 API Documentation (`/api`)

API specifications and integration guides:

- REST API endpoints
- OpenAPI/Swagger specifications
- Authentication and authorization
- API versioning strategies

### 📁 Component Documentation (`/components`)

Documentation for individual services and modules:

- **[Angular Frontend](components/angular-frontend.md)** - Angular 21 web application
- **[Conferences Integration](components/conferences-integration.md)** - Conferences API integration library
- Profiles Service
- Groups Service
- Other microservices

## Quick Links

- [Main README](../README.md) - Project overview and getting started
- [Copilot Instructions](../.copilot-instructions.md) - AI assistant guidelines

## Contributing to Documentation

When adding new documentation:

1. Place files in the appropriate category folder
2. Use descriptive filenames with kebab-case (e.g., `my-feature-guide.md`)
3. Update this README with links to new documents
4. Follow markdown best practices
5. Include code examples where applicable

## Documentation Standards

- **Format**: Markdown (.md)
- **Structure**: Use clear headings (H1 for title, H2 for sections)
- **Code Blocks**: Use fenced code blocks with language identifiers
- **Links**: Use relative links for internal documentation
- **Images**: Store in `images/` subfolder within the relevant category
