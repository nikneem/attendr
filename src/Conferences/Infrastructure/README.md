# Conferences Service Infrastructure

This directory contains the Infrastructure as Code (IaC) for deploying the Conferences service to Azure Container Apps.

## Structure

- `bicep/` - Bicep templates for Azure resources
  - `main.bicep` - Main deployment template
  - `modules/` - Reusable Bicep modules
    - `container-app.bicep` - Container App configuration
    - `role-assignments.bicep` - RBAC role assignments
    - `get-app-configuration.bicep` - App Configuration reference
    - `get-app-insights.bicep` - Application Insights reference
- `parameters/` - Environment-specific parameter files
  - `main.dev.bicepparam` - Development environment parameters
  - `main.prod.bicepparam` - Production environment parameters

## Deployment

### Prerequisites

- Azure CLI
- Bicep CLI
- Appropriate Azure subscription access

### Deploy to Development

```bash
az deployment sub create \
  --location northeurope \
  --template-file ./bicep/main.bicep \
  --parameters ./parameters/main.dev.bicepparam
```

### Deploy to Production

```bash
az deployment sub create \
  --location northeurope \
  --template-file ./bicep/main.bicep \
  --parameters ./parameters/main.prod.bicepparam
```

## Resources Deployed

- Resource Group for Conferences service
- Container App with:
  - System-assigned managed identity
  - Dapr integration (app ID: `conferences-api`)
  - Health probes (liveness, readiness, startup)
  - Auto-scaling based on HTTP requests
- RBAC role assignments:
  - App Configuration Data Reader
  - Key Vault Secrets User

## Configuration

The container app is configured to:
- Run on port 8080
- Use App Configuration for centralized configuration
- Use Application Insights for monitoring and logging
- Scale between 1-10 replicas based on load
- Use health check endpoints for container orchestration
