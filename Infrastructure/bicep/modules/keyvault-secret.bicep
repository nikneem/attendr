targetScope = 'resourceGroup'

@description('The name of the Key Vault')
param keyVaultName string

@description('The name of the secret')
param secretName string

@secure()
@description('The value of the secret')
param secretValue string

resource keyVault 'Microsoft.KeyVault/vaults@2025-05-01' existing = {
  name: keyVaultName
}

resource secret 'Microsoft.KeyVault/vaults/secrets@2025-05-01' = {
  parent: keyVault
  name: secretName
  properties: {
    value: secretValue
    contentType: 'text/plain'
  }
}

output secretUri string = secret.properties.secretUri
output secretName string = secret.name
