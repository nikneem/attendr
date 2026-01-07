targetScope = 'resourceGroup'

@description('The principal ID of the managed identity')
param principalId string

@description('The name of the App Configuration')
param appConfigurationName string

@description('The name of the Key Vault')
param keyVaultName string

// App Configuration Data Reader role
var appConfigurationDataReaderRoleId = '516239f1-63e1-4d78-a4de-a74fb236a071'

// Key Vault Secrets User role
var keyVaultSecretsUserRoleId = '4633458b-17de-408a-b874-0445c86b69e6'

resource appConfiguration 'Microsoft.AppConfiguration/configurationStores@2024-05-01' existing = {
  name: appConfigurationName
}

resource keyVault 'Microsoft.KeyVault/vaults@2023-07-01' existing = {
  name: keyVaultName
}

// Assign App Configuration Data Reader role
resource appConfigRoleAssignment 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(appConfiguration.id, principalId, appConfigurationDataReaderRoleId)
  scope: appConfiguration
  properties: {
    roleDefinitionId: subscriptionResourceId(
      'Microsoft.Authorization/roleDefinitions',
      appConfigurationDataReaderRoleId
    )
    principalId: principalId
    principalType: 'ServicePrincipal'
  }
}

// Assign Key Vault Secrets User role
resource keyVaultRoleAssignment 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(keyVault.id, principalId, keyVaultSecretsUserRoleId)
  scope: keyVault
  properties: {
    roleDefinitionId: subscriptionResourceId('Microsoft.Authorization/roleDefinitions', keyVaultSecretsUserRoleId)
    principalId: principalId
    principalType: 'ServicePrincipal'
  }
}

output appConfigRoleAssignmentId string = appConfigRoleAssignment.id
output keyVaultRoleAssignmentId string = keyVaultRoleAssignment.id
