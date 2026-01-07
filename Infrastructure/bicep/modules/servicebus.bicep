targetScope = 'resourceGroup'

@description('The name of the Service Bus namespace')
param namespaceName string

@description('The location for the Service Bus')
param location string

@description('Tags to apply to the Service Bus')
param tags object = {}

@description('SKU name')
@allowed([
  'Basic'
  'Standard'
  'Premium'
])
param skuName string = 'Standard'

@description('List of topic names to create')
param topics array = []

resource serviceBusNamespace 'Microsoft.ServiceBus/namespaces@2022-10-01-preview' = {
  name: namespaceName
  location: location
  tags: tags
  sku: {
    name: skuName
    tier: skuName
  }
  properties: {
    minimumTlsVersion: '1.2'
    publicNetworkAccess: 'Enabled'
    disableLocalAuth: false
  }
}

resource serviceBusTopics 'Microsoft.ServiceBus/namespaces/topics@2022-10-01-preview' = [
  for topic in topics: {
    parent: serviceBusNamespace
    name: topic
    properties: {
      enableBatchedOperations: true
      enablePartitioning: false
      maxSizeInMegabytes: 1024
      requiresDuplicateDetection: false
      supportOrdering: true
    }
  }
]

// Create default subscription for each topic
resource serviceBusSubscriptions 'Microsoft.ServiceBus/namespaces/topics/subscriptions@2022-10-01-preview' = [
  for topic in topics: {
    parent: serviceBusTopics[indexOf(topics, topic)]
    name: 'default'
    properties: {
      enableBatchedOperations: true
      maxDeliveryCount: 10
      requiresSession: false
      deadLetteringOnMessageExpiration: true
      lockDuration: 'PT5M'
    }
  }
]

output id string = serviceBusNamespace.id
output name string = serviceBusNamespace.name
output primaryConnectionString string = listKeys(
  '${serviceBusNamespace.id}/AuthorizationRules/RootManageSharedAccessKey',
  serviceBusNamespace.apiVersion
).primaryConnectionString
