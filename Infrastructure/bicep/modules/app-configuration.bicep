targetScope = 'resourceGroup'

@description('The name of the App Configuration store')
param name string

@description('The location for the App Configuration store')
param location string

@description('Tags to apply to the App Configuration store')
param tags object = {}

@description('The name of the Key Vault')
param keyVaultName string

@description('The URI of the MongoDB secret in Key Vault')
param keyVaultMongoDbSecretUri string

resource appConfig 'Microsoft.AppConfiguration/configurationStores@2024-05-01' = {
  name: name
  location: location
  tags: tags
  sku: {
    name: 'free'
  }
  properties: {
    enablePurgeProtection: false
    publicNetworkAccess: 'Enabled'
  }
}

resource keyVault 'Microsoft.KeyVault/vaults@2023-07-01' existing = {
  name: keyVaultName
}

// Grant App Configuration access to Key Vault
resource appConfigKeyVaultAccess 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(keyVault.id, appConfig.id, 'Key Vault Secrets User')
  scope: keyVault
  properties: {
    roleDefinitionId: subscriptionResourceId(
      'Microsoft.Authorization/roleDefinitions',
      '4633458b-17de-408a-b874-0445c86b69e6'
    ) // Key Vault Secrets User
    principalId: appConfig.identity.principalId
    principalType: 'ServicePrincipal'
  }
}

// Add Key Vault reference for MongoDB connection string
resource mongoDbConnectionStringKeyValue 'Microsoft.AppConfiguration/configurationStores/keyValues@2024-05-01' = {
  parent: appConfig
  name: 'ConnectionStrings:MongoDb'
  properties: {
    contentType: 'application/vnd.microsoft.appconfig.keyvaultref+json;charset=utf-8'
    value: '{"uri":"${keyVaultMongoDbSecretUri}"}'
  }
  dependsOn: [
    appConfigKeyVaultAccess
  ]
}

output id string = appConfig.id
output name string = appConfig.name
output endpoint string = appConfig.properties.endpoint
