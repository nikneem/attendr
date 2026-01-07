targetScope = 'resourceGroup'

@description('The name of the Key Vault')
param name string

@description('The location for the Key Vault')
param location string

@description('Tags to apply to the Key Vault')
param tags object = {}

@description('Enable RBAC authorization')
param enableRbacAuthorization bool = true

resource keyVault 'Microsoft.KeyVault/vaults@2025-05-01' = {
  name: name
  location: location
  tags: tags
  properties: {
    sku: {
      family: 'A'
      name: 'standard'
    }
    tenantId: subscription().tenantId
    enableRbacAuthorization: enableRbacAuthorization
    enabledForDeployment: false
    enabledForDiskEncryption: false
    enabledForTemplateDeployment: true
    enableSoftDelete: true
    softDeleteRetentionInDays: 7
    publicNetworkAccess: 'Enabled'
    networkAcls: {
      bypass: 'AzureServices'
      defaultAction: 'Allow'
    }
  }
}

output id string = keyVault.id
output name string = keyVault.name
output uri string = keyVault.properties.vaultUri
