targetScope = 'resourceGroup'

@description('The name of the Key Vault')
param keyVaultName string

@secure()
@description('MongoDB connection string')
param mongoDbConnectionString string

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

output mongoDbSecretUri string = mongoDbSecret.properties.secretUri
output mongoDbSecretUriWithVersion string = mongoDbSecret.properties.secretUriWithVersion
