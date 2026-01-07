targetScope = 'resourceGroup'

@description('The environment name (e.g., dev, staging, prod)')
param environmentName string

@description('The location for all resources')
param location string

@description('The base name for all resources')
param baseName string

@description('Tags to apply to all resources')
param tags object = {}

var uniqueSuffix = uniqueString(resourceGroup().id)
var keyVaultName = 'kv-${baseName}-${environmentName}-${take(uniqueSuffix, 6)}'
var appConfigName = 'appconfig-${baseName}-${environmentName}-${take(uniqueSuffix, 6)}'
var logAnalyticsName = 'log-${baseName}-${environmentName}'
var appInsightsName = 'appi-${baseName}-${environmentName}'
var containerAppsEnvName = 'cae-${baseName}-${environmentName}'
var cosmosAccountName = 'cosmos-${baseName}-${environmentName}-${take(uniqueSuffix, 6)}'
var cosmosDatabaseName = 'attendr'

// Key Vault
module keyVault './keyvault.bicep' = {
  params: {
    name: keyVaultName
    location: location
    tags: tags
  }
}

// Log Analytics Workspace for Azure Monitor
module logAnalytics './log-analytics.bicep' = {
  params: {
    name: logAnalyticsName
    location: location
    tags: tags
  }
}

// Application Insights
module appInsights './app-insights.bicep' = {
  params: {
    name: appInsightsName
    location: location
    tags: tags
    workspaceId: logAnalytics.outputs.id
  }
}

// Cosmos DB (MongoDB API) - Free tier
module cosmosDb './cosmosdb.bicep' = {
  params: {
    accountName: cosmosAccountName
    databaseName: cosmosDatabaseName
    location: location
    tags: tags
  }
}

// Store MongoDB connection string in Key Vault
module mongoDbSecrets './keyvault-secrets.bicep' = {
  params: {
    keyVaultName: keyVaultName
    mongoDbConnectionString: cosmosDb.outputs.connectionString
  }
  dependsOn: [
    keyVault
  ]
}

// Container Apps Environment
module containerAppsEnvironment './container-apps-environment.bicep' = {
  params: {
    name: containerAppsEnvName
    location: location
    tags: tags
    logAnalyticsWorkspaceId: logAnalytics.outputs.id
  }
}

// App Configuration with Key Vault references
module appConfiguration './app-configuration.bicep' = {
  params: {
    name: appConfigName
    location: location
    tags: tags
    keyVaultName: keyVaultName
    keyVaultMongoDbSecretUri: mongoDbSecrets.outputs.mongoDbSecretUri
  }
  dependsOn: [
    keyVault
  ]
}

output keyVaultName string = keyVaultName
output appConfigName string = appConfigName
output containerAppsEnvironmentName string = containerAppsEnvName
output mongoDbConnectionString string = cosmosDb.outputs.connectionString
output logAnalyticsWorkspaceId string = logAnalytics.outputs.id
output appInsightsConnectionString string = appInsights.outputs.connectionString
