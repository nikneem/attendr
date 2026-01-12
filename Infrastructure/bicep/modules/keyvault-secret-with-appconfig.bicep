targetScope = 'resourceGroup'

@description('The name of the Key Vault')
param keyVaultName string

@description('The name of the App Configuration')
param appConfigurationName string

@description('The name of the secret in Key Vault')
param secretName string

@description('The secret value to store')
@secure()
param secretValue string

@description('The key name in App Configuration')
param appConfigKey string

@description('Optional content type for the secret')
param contentType string = 'text/plain'

@description('Optional label for the App Configuration key')
param label string = ''

resource keyVault 'Microsoft.KeyVault/vaults@2023-07-01' existing = {
  name: keyVaultName
}

resource appConfiguration 'Microsoft.AppConfiguration/configurationStores@2024-05-01' existing = {
  name: appConfigurationName
}

// Store secret in Key Vault
resource secret 'Microsoft.KeyVault/vaults/secrets@2023-07-01' = {
  parent: keyVault
  name: secretName
  properties: {
    value: secretValue
    contentType: contentType
  }
}

// Create App Configuration key with Key Vault reference
resource appConfigKeyVaultReference 'Microsoft.AppConfiguration/configurationStores/keyValues@2024-05-01' = {
  parent: appConfiguration
  name: label == '' ? appConfigKey : '${appConfigKey}$${label}'
  properties: {
    contentType: 'application/vnd.microsoft.appconfig.keyvaultref+json;charset=utf-8'
    value: '{"uri":"${secret.properties.secretUri}"}'
  }
}
