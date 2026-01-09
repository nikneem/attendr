targetScope = 'resourceGroup'

@description('The name of the Key Vault')
param keyVaultName string

@secure()
@description('Service Bus connection string')
param serviceBusConnectionString string

@secure()
@description('Redis Cache connection string')
param redisCacheConnectionString string

@secure()
@description('Storage Account connection string')
param storageAccountConnectionString string

resource keyVault 'Microsoft.KeyVault/vaults@2025-05-01' existing = {
  name: keyVaultName
}

resource serviceBusSecret 'Microsoft.KeyVault/vaults/secrets@2025-05-01' = {
  parent: keyVault
  name: 'ServiceBusConnectionString'
  properties: {
    value: serviceBusConnectionString
    contentType: 'text/plain'
  }
}

resource redisCacheSecret 'Microsoft.KeyVault/vaults/secrets@2025-05-01' = {
  parent: keyVault
  name: 'RedisCacheConnectionString'
  properties: {
    value: redisCacheConnectionString
    contentType: 'text/plain'
  }
}

resource storageAccountSecret 'Microsoft.KeyVault/vaults/secrets@2025-05-01' = {
  parent: keyVault
  name: 'StorageAccountConnectionString'
  properties: {
    value: storageAccountConnectionString
    contentType: 'text/plain'
  }
}

// Outputs - secrets URIs for Key Vault references
output serviceBusSecretUri string = serviceBusSecret.properties.secretUri
output redisCacheSecretUri string = redisCacheSecret.properties.secretUri
output storageAccountSecretUri string = storageAccountSecret.properties.secretUri

parent: keyVault
name: 'PostgresConferencesConnectionString'
properties: {
value: postgresConferencesConnectionString
contentType: 'text/plain'
}
}

resource postgresPresenceSecret 'Microsoft.KeyVault/vaults/secrets@2025-05-01' = {
  parent: keyVault
  name: 'PostgresPresenceConnectionString'
  properties: {
    value: postgresPresenceConnectionString
    contentType: 'text/plain'
  }
}

output serviceBusSecretUri string = serviceBusSecret.properties.secretUri
output serviceBusSecretUriWithVersion string = serviceBusSecret.properties.secretUriWithVersion
output redisCacheSecretUri string = redisCacheSecret.properties.secretUri
output redisCacheSecretUriWithVersion string = redisCacheSecret.properties.secretUriWithVersion
output storageAccountSecretUri string = storageAccountSecret.properties.secretUri
output storageAccountSecretUriWithVersion string = storageAccountSecret.properties.secretUriWithVersion
output postgresGroupsSecretUri string = postgresGroupsSecret.properties.secretUri
output postgresConferencesSecretUri string = postgresConferencesSecret.properties.secretUri
output postgresPresenceSecretUri string = postgresPresenceSecret.properties.secretUri
