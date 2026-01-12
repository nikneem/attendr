targetScope = 'resourceGroup'

@description('The name of the Container Apps environment')
param containerAppsEnvironmentName string

@description('The Service Bus connection string')
@secure()
param serviceBusConnectionString string

@description('The Redis cache host name')
param redisCacheHostName string

@description('The Redis cache SSL port')
param redisCachePort int

@description('The Redis cache primary key')
@secure()
param redisCachePrimaryKey string

resource containerAppsEnvironment 'Microsoft.App/managedEnvironments@2025-10-02-preview' existing = {
  name: containerAppsEnvironmentName
}

// Dapr PubSub component using Azure Service Bus
resource daprPubSub 'Microsoft.App/managedEnvironments/daprComponents@2025-10-02-preview' = {
  parent: containerAppsEnvironment
  name: 'attendr-pubsub'
  properties: {
    componentType: 'pubsub.azure.servicebus.topics'
    version: 'v1'
    secrets: [
      {
        name: 'sb-connection-string'
        value: serviceBusConnectionString
      }
    ]
    metadata: [
      {
        name: 'connectionString'
        secretRef: 'sb-connection-string'
      }
    ]
    scopes: []
  }
}

// Dapr State Store component using Redis Cache
resource daprStateStore 'Microsoft.App/managedEnvironments/daprComponents@2025-10-02-preview' = {
  parent: containerAppsEnvironment
  name: 'attendr-statestore'
  properties: {
    componentType: 'state.redis'
    version: 'v1'
    secrets: [
      {
        name: 'redis-password'
        value: redisCachePrimaryKey
      }
    ]
    metadata: [
      {
        name: 'redisHost'
        value: '${redisCacheHostName}:${redisCachePort}'
      }
      {
        name: 'redisPassword'
        secretRef: 'redis-password'
      }
      {
        name: 'enableTLS'
        value: 'true'
      }
    ]
    scopes: []
  }
}

output pubSubComponentName string = daprPubSub.name
output stateStoreComponentName string = daprStateStore.name
