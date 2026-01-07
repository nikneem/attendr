targetScope = 'resourceGroup'

@description('The name of the Key Vault')
param keyVaultName string

@secure()
@description('MongoDB connection string')
param mongoDbConnectionString string

@secure()
@description('Service Bus connection string')
param serviceBusConnectionString string

@secure()
@description('Redis Cache connection string')
param redisCacheConnectionString string

resource keyVault 'Microsoft.KeyVault/vaults@2023-07-01' existing = {
  name: keyVaultName
}

resource mongoDbSecret 'Microsoft.KeyVault/vaults/secrets@2023-07-01' = {
  parent: keyVault
  name: 'MongoDbConnectionString'
  properties: {
    value: mongoDbConnectionString
    contentType: 'text/plain'
  }
}

resource serviceBusSecret 'Microsoft.KeyVault/vaults/secrets@2023-07-01' = {
  parent: keyVault
  name: 'ServiceBusConnectionString'
  properties: {
    value: serviceBusConnectionString
    contentType: 'text/plain'
  }
}

resource redisCacheSecret 'Microsoft.KeyVault/vaults/secrets@2023-07-01' = {
  parent: keyVault
  name: 'RedisCacheConnectionString'
  properties: {
    value: redisCacheConnectionString
    contentType: 'text/plain'
  }
}

output mongoDbSecretUri string = mongoDbSecret.properties.secretUri
output mongoDbSecretUriWithVersion string = mongoDbSecret.properties.secretUriWithVersion
output serviceBusSecretUri string = serviceBusSecret.properties.secretUri
output serviceBusSecretUriWithVersion string = serviceBusSecret.properties.secretUriWithVersion
output redisCacheSecretUri string = redisCacheSecret.properties.secretUri
output redisCacheSecretUriWithVersion string = redisCacheSecret.properties.secretUriWithVersion
