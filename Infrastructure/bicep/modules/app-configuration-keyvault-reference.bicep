targetScope = 'resourceGroup'

@description('The name of the App Configuration store')
param appConfigurationName string

@description('The key name for the configuration')
param keyName string

@description('The name of the Key Vault')
param keyVaultName string

@description('The name of the secret in Key Vault')
param secretName string

resource appConfig 'Microsoft.AppConfiguration/configurationStores@2025-06-01-preview' existing = {
  name: appConfigurationName
}

resource keyVault 'Microsoft.KeyVault/vaults@2025-05-01' existing = {
  name: keyVaultName
}

resource secret 'Microsoft.KeyVault/vaults/secrets@2025-05-01' existing = {
  parent: keyVault
  name: secretName
}

resource keyValue 'Microsoft.AppConfiguration/configurationStores/keyValues@2025-06-01-preview' = {
  parent: appConfig
  name: keyName
  properties: {
    contentType: 'application/vnd.microsoft.appconfig.keyvaultref+json;charset=utf-8'
    value: '{"uri":"${secret.properties.secretUri}"}'
  }
}

output keyName string = keyValue.name
