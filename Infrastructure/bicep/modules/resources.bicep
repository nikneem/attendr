targetScope = 'resourceGroup'

@description('The environment name (e.g., dev, staging, prod)')
param environmentName string

@description('The location for all resources')
param location string = resourceGroup().location

@description('The base name for all resources')
param baseName string

@description('Tags to apply to all resources')
param tags object = {}

@description('Container registry information')
param containerRegistry object

var uniqueSuffix = uniqueString(resourceGroup().id)
var keyVaultName = 'kv-${baseName}-${environmentName}'
var appConfigName = 'appconfig-${baseName}-${environmentName}-${take(uniqueSuffix, 6)}'
var logAnalyticsName = 'log-${baseName}-${environmentName}'
var appInsightsName = 'appi-${baseName}-${environmentName}'
var containerAppsEnvName = 'cae-${baseName}-${environmentName}'
var serviceBusNamespaceName = 'sb-${baseName}-${environmentName}-${take(uniqueSuffix, 6)}'
var redisCacheName = 'redis-${baseName}-${environmentName}-${take(uniqueSuffix, 6)}'
var userAssignedIdentityName = 'id-${baseName}-${environmentName}'
var storageAccountName = 'st${baseName}${environmentName}${take(uniqueSuffix, 6)}'

// Integration event topics from the IntegrationEvents library
var serviceBusTopics = [
  'conference.created'
  'conference.updated'
  'profile.created'
  'profile.updated'
  'profile.followed.conference'
  'profiles.followed.conference'
  'presentation.updated'
  'presentation.schedule-changed'
  'profile.checked-in'
  'profile.conference-attendance-changed'
]

// Key Vault
module keyVault './keyvault.bicep' = {
  params: {
    name: keyVaultName
    location: location
    tags: tags
  }
}

// Create central user assigned identity with ACR pull permissions
module userIdentity './user-assigned-identity.bicep' = {
  params: {
    name: userAssignedIdentityName
    location: location
    tags: tags
    containerRegistry: containerRegistry
    appConfigurationName: appConfigName
    keyVaultName: keyVaultName
  }
  dependsOn: [
    keyVault
  ]
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

// Storage Account for Profiles service (Table Storage)
module storageAccount './storage-account.bicep' = {
  params: {
    name: storageAccountName
    location: location
    tags: tags
    skuName: 'Standard_LRS'
  }
}

// Service Bus with topics
module serviceBus './servicebus.bicep' = {
  params: {
    namespaceName: serviceBusNamespaceName
    location: location
    tags: tags
    skuName: 'Standard'
    topics: serviceBusTopics
  }
}

// Redis Cache (cheapest tier)
module redisCache './redis.bicep' = {
  params: {
    name: redisCacheName
    location: location
    tags: tags
    skuName: 'Basic'
    skuFamily: 'C'
    skuCapacity: 0
  }
}

// Reference to deployed Service Bus for connection string
resource serviceBusNamespace 'Microsoft.ServiceBus/namespaces@2025-05-01-preview' existing = {
  name: serviceBusNamespaceName
  dependsOn: [
    serviceBus
  ]
}

// Reference to deployed Redis Cache for connection string
resource redisCacheResource 'Microsoft.Cache/redis@2024-11-01' existing = {
  name: redisCacheName
  dependsOn: [
    redisCache
  ]
}

// Store Service Bus connection string in Key Vault and App Configuration
module serviceBusSecret './keyvault-secret-with-appconfig.bicep' = {
  params: {
    keyVaultName: keyVaultName
    appConfigurationName: appConfigName
    secretName: 'ServiceBusConnectionString'
    secretValue: listKeys(
      '${serviceBusNamespace.id}/AuthorizationRules/RootManageSharedAccessKey',
      '2025-05-01-preview'
    ).primaryConnectionString
    appConfigKey: 'ConnectionStrings:ServiceBus'
  }
  dependsOn: [
    keyVault
    appConfiguration
  ]
}

// Store Redis Cache connection string in Key Vault and App Configuration
module redisCacheSecret './keyvault-secret-with-appconfig.bicep' = {
  params: {
    keyVaultName: keyVaultName
    appConfigurationName: appConfigName
    secretName: 'RedisCacheConnectionString'
    secretValue: '${redisCacheResource.properties.hostName}:${redisCacheResource.properties.sslPort},password=${redisCacheResource.listKeys().primaryKey},ssl=True,abortConnect=False'
    appConfigKey: 'ConnectionStrings:RedisCache'
  }
  dependsOn: [
    keyVault
    appConfiguration
  ]
}

// Store Storage Account connection string in Key Vault and App Configuration
module storageAccountSecret './keyvault-secret-with-appconfig.bicep' = {
  params: {
    keyVaultName: keyVaultName
    appConfigurationName: appConfigName
    secretName: 'StorageAccountConnectionString'
    secretValue: storageAccount.outputs.connectionString
    appConfigKey: 'ConnectionStrings:StorageAccount'
  }
  dependsOn: [
    keyVault
    appConfiguration
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

// DAPR Components for Container Apps
module daprComponents './dapr-components.bicep' = {
  params: {
    containerAppsEnvironmentName: containerAppsEnvName
    serviceBusConnectionString: listKeys(
      '${serviceBusNamespace.id}/AuthorizationRules/RootManageSharedAccessKey',
      '2025-05-01-preview'
    ).primaryConnectionString
    redisCacheHostName: redisCacheResource.properties.hostName
    redisCachePort: redisCacheResource.properties.sslPort
    redisCachePrimaryKey: redisCacheResource.listKeys().primaryKey
  }
  dependsOn: [
    containerAppsEnvironment
  ]
}

// App Configuration
module appConfiguration './app-configuration.bicep' = {
  params: {
    name: appConfigName
    location: location
    tags: tags
  }
  dependsOn: [
    keyVault
  ]
}

output keyVaultName string = keyVaultName
output appConfigName string = appConfigName
output containerAppsEnvironmentName string = containerAppsEnvName
output logAnalyticsWorkspaceId string = logAnalytics.outputs.id
output appInsightsConnectionString string = appInsights.outputs.connectionString
output serviceBusNamespace string = serviceBusNamespaceName
output redisCacheName string = redisCacheName
output storageAccountName string = storageAccount.outputs.name
output daprPubSubComponentName string = daprComponents.outputs.pubSubComponentName
output daprStateStoreComponentName string = daprComponents.outputs.stateStoreComponentName
output userAssignedIdentityId string = userIdentity.outputs.id
output userAssignedIdentityPrincipalId string = userIdentity.outputs.principalId
output userAssignedIdentityClientId string = userIdentity.outputs.clientId
output containerAppsDefaultDomainName string = containerAppsEnvironment.outputs.defaultDomain
