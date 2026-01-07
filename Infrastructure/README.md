# Azure Infrastructure

This directory contains the Infrastructure as Code (IaC) for the Attendr application using Azure Bicep.

## Structure

```
Infrastructure/
├── bicep/
│   ├── main.bicep                          # Subscription-level deployment
│   └── modules/
│       ├── resources.bicep                 # Main resources orchestration
│       ├── keyvault.bicep                  # Azure Key Vault
│       ├── keyvault-secrets.bicep          # Key Vault secrets
│       ├── log-analytics.bicep             # Log Analytics workspace
│       ├── app-insights.bicep              # Application Insights
│       ├── cosmosdb.bicep                  # Cosmos DB (MongoDB API)
│       ├── servicebus.bicep                # Azure Service Bus with topics
│       ├── redis.bicep                     # Azure Redis Cache
│       ├── container-apps-environment.bicep # Container Apps environment
│       ├── dapr-components.bicep           # Dapr components (pubsub & statestore)
│       └── app-configuration.bicep         # App Configuration
├── parameters/
│   ├── main.prod.bicepparam                # Production parameters
│   └── main.dev.bicepparam                 # Development parameters
└── compiled/                                # Generated during build (gitignored)
```

## Resources Deployed

1. **Resource Group** - Container for all resources
2. **Azure Key Vault** - Secure secrets storage (Standard tier)
3. **Azure Cosmos DB** - MongoDB API (Free tier eligible)
4. **Azure Service Bus** - Message broker with topics for integration events (Standard tier)
5. **Azure Redis Cache** - In-memory cache for state storage (Basic tier, C0)
6. **Azure Monitor**:
   - Log Analytics Workspace (PerGB2018 pricing)
   - Application Insights
7. **Azure Container Apps Environment** - For hosting containerized applications
   - **Dapr PubSub Component** - Uses Service Bus for pub/sub messaging
   - **Dapr State Store Component** - Uses Redis Cache for state management
8. **Azure App Configuration** - Centralized configuration (Free tier)

### Key Features

- **MongoDB, Service Bus, and Redis secrets** are stored in Key Vault
- **App Configuration** uses Key Vault references for secure secret access
- **Dapr components** configured automatically in Container Apps environment:
  - `pubsub` - Azure Service Bus Topics integration
  - `statestore` - Redis Cache integration
- **Service Bus Topics** automatically created from integration events:
  - `conference.created`
  - `conference.updated`
  - `profile.created`
  - `profile.updated`
  - `profile.followed.conference`
  - `profiles.followed.conference`
  - `presentation.updated`
  - `presentation.schedule-changed`
  - `profile.checked-in`
  - `profile.conference-attendance-changed`
- **Free/cheap tier** selections where available:
  - Cosmos DB: Free tier enabled (first 1000 RU/s and 25 GB free)
  - App Configuration: Free tier
  - Key Vault: Standard tier (most cost-effective)
  - Log Analytics: PerGB2018 with 30-day retention
  - Service Bus: Standard tier (supports topics)
  - Redis Cache: Basic tier, C0 (250 MB, cheapest option)

## Deployment

### Prerequisites

1. Azure subscription with appropriate permissions
2. Azure CLI installed
3. GitHub repository secrets configured:
   - `AZURE_PROD_SUBSCRIPTION`
   - `AZURE_PROD_TENANDID`
   - `AZURE_PROD_CLIENTID`

### GitHub Actions

The infrastructure is deployed via GitHub Actions workflow: `.github/workflows/deploy-infrastructure.yml`

**Triggers:**
- Push to `main` branch (when Infrastructure files change)
- Manual workflow dispatch (with environment selection)

**Workflow:**
1. **Build Job**: Transpiles Bicep files to ARM JSON templates
2. **Deploy Job**: Deploys to Azure subscription using federated identity

### Manual Deployment

```bash
# Login to Azure
az login

# BserviceBusNamespace` - Name of the Service Bus namespace
- `redisCacheName` - Name of the Redis Cache
- `daprPubSubComponentName` - Name of the Dapr PubSub component (pubsub)
- `daprStateStoreComponentName` - Name of the Dapr State Store component (statestor
cd Infrastructure
az bicep build --file bicep/main.bicep --outfile compiled/main.json
az bicep build-params --file parameters/main.prod.bicepparam --outfile compiled/main.prod.parameters.json

# Deploy to subscription
az deployment sub create \
  --name "attendr-infra-$(date +%Y%m%d-%H%M%S)" \
  --location westeurope \
  --template-file compiled/main.json \
  --parameters compiled/main.prod.parameters.json
```

## Outputs

The deployment provides the following outputs:
- `resourceGroupName` - Name of the created resource group
- `keyVaultName` - Name of the Key Vault
- `appConfigName` - Name of the App Configuration store
- `containerAppsEnvironmentName` - Name of the Container Apps environment
- `mongoDbConnectionString` - Connection string for MongoDB (sensitive)

## Security

- Key Vau, Service Bus, and Redis connection strings stored securely in Key Vault
- App Configuration references Key Vault for secrets (not stored directly)
- Dapr components use secrets securely stored in Container Apps environment
- App Configuration has managed identity access to Key Vault
- MongoDB connection strings stored securely in Key Vault
- App Configuration references Key Vault for secrets (not stored directly)

## Cost Optimization

ReService Bus Standard tier (cheapest tier that supports topics)
- Redis Cache Basic C0 (250 MB, ~$16/month)
- sources are configured for minimal cost:
- Cosmos DB Free tier (first account per subscription)
- App Configuration Free tier
- Log Analytics with minimal retention (30 days)
- No zone redundancy
- Periodic backup for Cosmos DB (cheaper than continuous)

## Next Steps

After infrastructure deployment:
1. Deploy container applications to Container Apps environment
2. Configure application settings in App Configuration
3. Set up CI/CD pipelines for application deployments
