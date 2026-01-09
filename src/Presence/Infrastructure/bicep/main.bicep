targetScope = 'subscription'

@description('The environment name (e.g., dev, staging, prod)')
param environmentName string

@description('The location for all resources')
param location string = 'northeurope'

@description('The base name for all resources')
param baseName string = 'attendr'

@description('Tags to apply to all resources')
param tags object = {}

@description('Landing zone resource names')
param landingzone object = {
  resourceGroupName: ''
  containerAppsEnvironmentName: ''
  appConfigurationName: ''
  keyVaultName: ''
  applicationInsightsName: ''
}

@description('Container registry server')
param containerRegistryServer string

@description('Container registry username')
@secure()
param containerRegistryUsername string

@description('Container registry password')
@secure()
param containerRegistryPassword string

@description('The container image to deploy')
param containerImage string

@description('CORS allowed origins')
param corsOrigins array = []

var resourceGroupName = 'rg-${baseName}-${environmentName}'
var postgresServerName = 'psql-pres-${baseName}-${environmentName}-${uniqueString(subscription().subscriptionId, resourceGroupName)}'
var postgresAdminLogin = 'attendradmin'
var postgresAdminPassword = '${uniqueString(subscription().subscriptionId, resourceGroupName)}P@ssw0rd!'
var postgresDatabaseName = 'attendr-presence'

// Deploy Resource Group for Presence service
resource resourceGroup 'Microsoft.Resources/resourceGroups@2024-03-01' = {
  name: resourceGroupName
  location: location
  tags: tags
}

// Reference to landing zone resource group
resource landingZoneResourceGroup 'Microsoft.Resources/resourceGroups@2024-03-01' existing = {
  name: landingzone.resourceGroupName
}

// Deploy PostgreSQL server with database for Presence service
module postgresServer './modules/postgresql.bicep' = {
  scope: resourceGroup
  params: {
    serverName: postgresServerName
    location: location
    tags: tags
    administratorLogin: postgresAdminLogin
    administratorPassword: postgresAdminPassword
    postgresVersion: '16'
    skuName: 'Standard_B1ms'
    skuTier: 'Burstable'
    storageSizeGB: 32
    databaseName: postgresDatabaseName
  }
}

// Get App Configuration endpoint
module appConfigurationEndpoint './modules/get-app-configuration.bicep' = {
  scope: landingZoneResourceGroup
  params: {
    appConfigurationName: landingzone.appConfigurationName
  }
}

// Get Application Insights connection string
module appInsightsConnectionString './modules/get-app-insights.bicep' = {
  scope: landingZoneResourceGroup
  params: {
    applicationInsightsName: landingzone.applicationInsightsName
  }
}

// Deploy the Presence container app
module presenceApp './modules/container-app.bicep' = {
  scope: resourceGroup
  params: {
    name: 'ca-${baseName}-${environmentName}'
    location: location
    tags: tags
    landingZoneResourceGroupName: landingzone.resourceGroupName
    containerAppsEnvironmentName: landingzone.containerAppsEnvironmentName
    containerImage: containerImage
    appConfigurationEndpoint: appConfigurationEndpoint.outputs.endpoint
    applicationInsightsConnectionString: appInsightsConnectionString.outputs.connectionString
    containerRegistryServer: containerRegistryServer
    containerRegistryUsername: containerRegistryUsername
    containerRegistryPassword: containerRegistryPassword
    corsOrigins: corsOrigins
  }
  dependsOn: [
    postgresServer
  ]
}

// Assign permissions to the container app's system-assigned managed identity
module roleAssignments './modules/role-assignments.bicep' = {
  scope: landingZoneResourceGroup
  params: {
    principalId: presenceApp.outputs.managedIdentityPrincipalId
    appConfigurationName: landingzone.appConfigurationName
    keyVaultName: landingzone.keyVaultName
  }
}

// Store PostgreSQL connection string in landing zone Key Vault
module postgresSecret '../../../../Infrastructure/bicep/modules/keyvault-secret.bicep' = {
  scope: landingZoneResourceGroup
  params: {
    keyVaultName: landingzone.keyVaultName
    secretName: 'PostgresPresenceConnectionString'
    secretValue: postgresServer.outputs.connectionString
  }
}

// Add PostgreSQL connection string to App Configuration as Key Vault reference
module postgresAppConfig '../../../../Infrastructure/bicep/modules/app-configuration-keyvault-reference.bicep' = {
  scope: landingZoneResourceGroup
  params: {
    appConfigurationName: landingzone.appConfigurationName
    keyName: 'ConnectionStrings:attendr-presence'
    keyVaultName: landingzone.keyVaultName
    secretName: 'PostgresPresenceConnectionString'
  }
  dependsOn: [
    postgresSecret
  ]
}

// Configure service integration endpoints in App Configuration
module serviceIntegration '../../../../Infrastructure/bicep/modules/app-configuration-service-integration.bicep' = {
  scope: landingZoneResourceGroup
  params: {
    appConfigurationName: landingzone.appConfigurationName
    environmentName: environmentName
    baseName: baseName
    serviceName: 'Presence'
  }
}
output resourceGroupName string = resourceGroup.name
output containerAppName string = presenceApp.outputs.name
output containerAppFqdn string = presenceApp.outputs.fqdn
output managedIdentityPrincipalId string = presenceApp.outputs.managedIdentityPrincipalId
output postgresServerName string = postgresServer.outputs.serverName
output postgresDatabaseName string = postgresServer.outputs.databaseName
