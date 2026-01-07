# Profiles Service Infrastructure

This directory contains the Infrastructure as Code (IaC) for deploying the Profiles service to Azure Container Apps.

## Structure

```
Infrastructure/
├── bicep/
│   ├── main.bicep                          # Subscription-level deployment
│   └── modules/
│       ├── container-app.bicep             # Container App configuration
│       ├── role-assignments.bicep          # Managed identity permissions
│       ├── get-app-configuration.bicep     # Get App Config endpoint
│       └── get-app-insights.bicep          # Get App Insights connection string
└── parameters/
    ├── main.prod.bicepparam                # Production parameters
    └── main.dev.bicepparam                 # Development parameters
```

## Resources Deployed

1. **Resource Group** - `rg-attendr-profiles-{environment}`
2. **Container App** - Profiles API running on the landing zone Container Apps environment
   - Managed Identity enabled
   - Dapr enabled (app-id: `profiles-api`)
   - Health probes configured (liveness, readiness, startup)
   - Auto-scaling (1-10 replicas)

## Managed Identity Permissions

The container app's managed identity is automatically granted:

- **App Configuration Data Reader** - Read access to Azure App Configuration
- **Key Vault Secrets User** - Read access to Key Vault secrets

## Configuration

The container app is configured with:

- **Dapr**: Enabled with app-id `profiles-api`
- **Port**: 8080
- **Ingress**: External HTTPS
- **Environment Variables**:
  - `ASPNETCORE_ENVIRONMENT`: Production
  - `AppConfiguration__Endpoint`: From landing zone App Configuration
  - `APPLICATIONINSIGHTS_CONNECTION_STRING`: From landing zone Application Insights

## Prerequisites

Before deploying, update the parameter files with:

1. Landing zone resource group name
2. Container Apps environment name (from landing zone deployment)
3. App Configuration name (from landing zone deployment)
4. Key Vault name (from landing zone deployment)
5. Application Insights name (from landing zone deployment)
6. Container registry and image details

## Deployment

### Using Azure CLI

```bash
# Login to Azure
az login

# Build Bicep to JSON (optional, for validation)
az bicep build --file bicep/main.bicep

# Deploy to production
az deployment sub create \
  --name "profiles-service-$(date +%Y%m%d-%H%M%S)" \
  --location northeurope \
  --template-file bicep/main.bicep \
  --parameters parameters/main.prod.bicepparam

# Deploy to development
az deployment sub create \
  --name "profiles-service-dev-$(date +%Y%m%d-%H%M%S)" \
  --location northeurope \
  --template-file bicep/main.bicep \
  --parameters parameters/main.dev.bicepparam
```

### Using GitHub Actions

Create a workflow similar to the infrastructure deployment workflow, but targeted at this service-specific infrastructure.

## Outputs

The deployment provides:

- `resourceGroupName` - Name of the service resource group
- `containerAppName` - Name of the container app
- `containerAppFqdn` - Fully qualified domain name of the container app
- `managedIdentityPrincipalId` - Principal ID of the managed identity

## Dependencies

This infrastructure depends on the landing zone deployment, which must be deployed first. The landing zone creates:

- Container Apps environment
- Azure App Configuration
- Azure Key Vault
- Application Insights
- Service Bus (for Dapr pubsub)
- Redis Cache (for Dapr state store)

## Notes

- The container app uses a system-assigned managed identity
- Role assignments are created automatically in the landing zone resource group
- The container image must be accessible from the Container Apps environment
- Health checks ensure the service is healthy before receiving traffic
- Dapr components (pubsub and statestore) are available from the landing zone environment
