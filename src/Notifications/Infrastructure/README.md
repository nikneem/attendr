# Notifications Service Infrastructure

This directory contains the Infrastructure as Code (IaC) for deploying the Notifications service to Azure Container Apps.

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

### Resource Group
- **Name**: `rg-attendr-notif-{environmentName}`
- **Purpose**: Contains all Notifications service-specific resources

### Storage Account
- **Type**: Azure Storage Account (Standard_LRS)
- **Tables**: 
  - `notifications` - Notification records
  - `notificationpreferences` - User notification preferences
  - `subscriptions` - Push notification subscriptions
- **Purpose**: Stores notification data using Azure Table Storage

### Container App
- **Name**: `ca-attendr-notif-{environmentName}`
- **Image**: From Azure Container Registry
- **Resources**: 0.25 CPU, 0.5Gi Memory
- **Scaling**: 1-10 replicas based on HTTP load
- **Features**:
  - External ingress with CORS support
  - Dapr enabled (appId: `notifications-api`)
  - Health probes (liveness, readiness, startup)
  - Managed identity for Azure service authentication

## Managed Identity Permissions

The Container App's managed identity is granted:
- **Storage Table Data Contributor** - Read/write access to notification tables
- **App Configuration Data Reader** - Read configuration from App Configuration
- **Key Vault Secrets User** - Access secrets from Key Vault

## Configuration

### Table Storage
Three App Configuration entries are created:
- `Aspire:Azure:Data:Tables:notifications:ServiceUri`
- `Aspire:Azure:Data:Tables:notificationpreferences:ServiceUri`
- `Aspire:Azure:Data:Tables:subscriptions:ServiceUri`

Each points to the storage account's table endpoint.

### Environment Variables
- `ASPNETCORE_ENVIRONMENT`: Production/Development
- `ASPNETCORE_HTTP_PORTS`: 8080
- `AppConfiguration__Endpoint`: Landing zone App Configuration endpoint
- `APPLICATIONINSIGHTS_CONNECTION_STRING`: From landing zone Application Insights

## Prerequisites

1. **Landing Zone Resources** (must already exist):
   - Resource Group
   - Container Apps Environment
   - App Configuration
   - Key Vault
   - Application Insights

2. **Container Image**: Published to Azure Container Registry

3. **Azure CLI**: Bicep CLI installed

## Deployment

### Using Azure CLI

```bash
# Login to Azure
az login

# Deploy to production
az deployment sub create \
  --name "notifications-service-$(date +%Y%m%d-%H%M%S)" \
  --location northeurope \
  --template-file bicep/main.bicep \
  --parameters parameters/main.prod.bicepparam

# Deploy to development
az deployment sub create \
  --name "notifications-service-dev-$(date +%Y%m%d-%H%M%S)" \
  --location northeurope \
  --template-file bicep/main.bicep \
  --parameters parameters/main.dev.bicepparam
```

### Using GitHub Actions

Reference this infrastructure in your CI/CD pipeline:

```yaml
- name: Deploy Notifications Infrastructure
  run: |
    az deployment sub create \
      --name "notifications-${{ github.run_number }}" \
      --location northeurope \
      --template-file src/Notifications/Infrastructure/bicep/main.bicep \
      --parameters src/Notifications/Infrastructure/parameters/main.prod.bicepparam \
      --parameters containerImage=${{ env.CONTAINER_IMAGE }} \
      --parameters containerRegistryServer=${{ secrets.REGISTRY_SERVER }} \
      --parameters containerRegistryUsername=${{ secrets.REGISTRY_USERNAME }} \
      --parameters containerRegistryPassword=${{ secrets.REGISTRY_PASSWORD }}
```

## Outputs

- `containerAppName`: Name of the deployed Container App
- `containerAppUrl`: HTTPS URL of the Container App
- `storageAccountName`: Name of the storage account containing notification tables

## Dependencies

### Landing Zone Resources
- Container Apps Environment for hosting
- App Configuration for centralized configuration
- Key Vault for secrets management
- Application Insights for monitoring

### Shared Infrastructure
- Azure Container Registry (for container images)
- Virtual Network (if using VNet integration)

## Notes

- Storage account name is generated using `uniqueString()` to ensure global uniqueness
- Three tables are automatically created during deployment
- Container App uses Dapr for service-to-service communication
- Health endpoint must be implemented at `/health` path
- CORS is configured for specified origins
- Managed identity eliminates need for connection strings in app configuration
