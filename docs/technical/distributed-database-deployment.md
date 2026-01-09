# Infrastructure Deployment - Distributed Database Architecture

## Overview
The infrastructure has been updated to follow a distributed microservices pattern where each service owns and deploys its own database in its own resource group. This provides better isolation, independent scaling, and clearer ownership boundaries.

## Architecture Pattern

### Landing Zone (Centralized Resources)
Located in: `Infrastructure/bicep/`

The landing zone deploys shared infrastructure resources:
- **Azure Key Vault**: Stores all secrets including database connection strings
- **Azure App Configuration**: Central configuration store with Key Vault references
- **Azure Service Bus**: Message broker for integration events
- **Azure Redis Cache**: Distributed cache and state store for DAPR
- **Container Apps Environment**: Shared hosting environment
- **Log Analytics & Application Insights**: Centralized monitoring
- **Storage Account**: Table Storage for Profiles service (only service that uses Table Storage)

### Service-Specific Resources
Each service (Groups, Conferences, Presence) deploys in its own resource group:
- **Resource Group**: `rg-{baseName}-{environmentName}-{serviceName}`
- **PostgreSQL Flexible Server**: Dedicated database server
- **Container App**: The service application
- **Connection String Storage**: Stored in landing zone Key Vault
- **App Configuration Entry**: Reference to Key Vault secret

## Database Deployment

### Groups Service
- **Location**: `src/Groups/Infrastructure/bicep/`
- **PostgreSQL Server**: `psql-groups-{baseName}-{environmentName}-{uniqueSuffix}`
- **Database Name**: `attendr-groups`
- **Connection String**: Stored in Key Vault as `PostgresGroupsConnectionString`
- **App Config Key**: `ConnectionStrings:attendr-groups`

### Conferences Service
- **Location**: `src/Conferences/Infrastructure/bicep/`
- **PostgreSQL Server**: `psql-conf-{baseName}-{environmentName}-{uniqueSuffix}`
- **Database Name**: `attendr-conferences`
- **Connection String**: Stored in Key Vault as `PostgresConferencesConnectionString`
- **App Config Key**: `ConnectionStrings:attendr-conferences`

### Presence Service
- **Location**: `src/Presence/Infrastructure/bicep/`
- **PostgreSQL Server**: `psql-pres-{baseName}-{environmentName}-{uniqueSuffix}`
- **Database Name**: `attendr-presence`
- **Connection String**: Stored in Key Vault as `PostgresPresenceConnectionString`
- **App Config Key**: `ConnectionStrings:attendr-presence`

### Profiles Service
- **Database**: Azure Table Storage (deployed in landing zone)
- **Storage Account**: `st{baseName}{environmentName}{uniqueSuffix}`
- **Table Name**: `profiles`
- **Connection String**: Stored in Key Vault as `StorageAccountConnectionString`
- **App Config Key**: `ConnectionStrings:profiles`

## Deployment Flow

1. **Landing Zone Deployment**
   ```bash
   az deployment sub create \
     --location northeurope \
     --template-file Infrastructure/bicep/main.bicep \
     --parameters Infrastructure/parameters/main.dev.bicepparam
   ```
   - Creates shared resources
   - Deploys Storage Account for Profiles
   - Does NOT deploy PostgreSQL servers

2. **Service Deployment** (for each service)
   ```bash
   az deployment sub create \
     --location northeurope \
     --template-file src/{Service}/Infrastructure/bicep/main.bicep \
     --parameters @parameters.json
   ```
   - Creates service resource group
   - Deploys PostgreSQL server and database
   - Deploys container app
   - Stores connection string in landing zone Key Vault
   - Adds App Configuration entry referencing Key Vault secret

## PostgreSQL Configuration

All PostgreSQL servers use consistent settings:
- **Version**: PostgreSQL 16
- **SKU**: Standard_B1ms (Burstable tier)
- **Storage**: 32 GB with auto-grow enabled
- **Backup**: 7-day retention, no geo-redundancy
- **High Availability**: Disabled (for dev/test)
- **Network**: Public access enabled with firewall rule for Azure services
- **Database Charset**: UTF8
- **Collation**: en_US.utf8

### Security
- Admin credentials generated using `uniqueString()` per service
- Connection strings stored securely in Key Vault
- Container apps access via managed identity and Key Vault references
- No passwords in App Configuration (only Key Vault URIs)

## Helper Modules

### keyvault-secret.bicep
Adds a secret to an existing Key Vault.
```bicep
module secret 'keyvault-secret.bicep' = {
  params: {
    keyVaultName: 'kv-name'
    secretName: 'SecretName'
    secretValue: 'SecretValue'
  }
}
```

### app-configuration-keyvault-reference.bicep
Creates an App Configuration entry that references a Key Vault secret.
```bicep
module appConfigRef 'app-configuration-keyvault-reference.bicep' = {
  params: {
    appConfigurationName: 'appconfig-name'
    keyName: 'ConnectionStrings:database'
    keyVaultName: 'kv-name'
    secretName: 'DatabaseConnectionString'
  }
}
```

## Benefits of This Architecture

1. **Service Isolation**: Each service has its own database server, preventing cross-service interference
2. **Independent Scaling**: Database resources can be scaled per service needs
3. **Clear Ownership**: Service teams own their data storage
4. **Simplified Migrations**: Each service can manage its own schema migrations
5. **Resource Group Boundaries**: Align with microservices principles
6. **Cost Tracking**: Easier to track costs per service
7. **Security Boundaries**: Each PostgreSQL server has its own credentials
8. **Disaster Recovery**: Can backup/restore services independently

## Connection String Management

Services don't store connection strings directly. Instead:
1. Service deployment creates PostgreSQL server
2. Connection string stored in landing zone Key Vault
3. App Configuration gets Key Vault reference URI
4. Container apps use managed identity to access App Configuration
5. App Configuration uses managed identity to resolve Key Vault references
6. Application receives actual connection string at runtime

This ensures:
- No secrets in source control
- No secrets in deployment parameters
- Automatic secret rotation support
- Centralized secret management
- Audit trail in Key Vault

## Future Considerations

- **Production**: Consider enabling High Availability mode for PostgreSQL
- **Scaling**: Upgrade to GeneralPurpose or MemoryOptimized tiers as needed
- **Private Endpoints**: Disable public access and use private endpoints
- **Read Replicas**: Add read replicas for read-heavy workloads
- **Geo-Redundancy**: Enable for production environments
- **Backup Strategy**: Implement additional backup policies beyond default 7 days
