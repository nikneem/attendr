targetScope = 'resourceGroup'

@description('The principal ID of the managed identity')
param principalId string

param landingZoneResourceGroupName string

resource storageTableDataContributorRoleDefinition 'Microsoft.Authorization/roleDefinitions@2022-05-01-preview' existing = {
  name: '0a9a7e1f-b9d0-4cc4-a60d-0319b160aaa3'
}
resource appConfigurationReaderRoleDefinition 'Microsoft.Authorization/roleDefinitions@2022-05-01-preview' existing = {
  name: '516239f1-63e1-4d78-a4de-a74fb236a071'
}
resource keyVaultSecrestUserRoldeDefinition 'Microsoft.Authorization/roleDefinitions@2022-05-01-preview' existing = {
  name: '4633458b-17de-408a-b874-0445c86b69e6'
}

// Storage Account Table Data Contributor role assignment
module storageAccountTableDataContibutorRoleAssignment '../../../../../Infrastructure/bicep/modules/role-assignment.bicep' = {
  name: 'storageAccountTableDataContibutorRoleAssignment'
  params: {
    principalId: principalId
    roleDefinitionId: storageTableDataContributorRoleDefinition.name
    scopeId: resourceGroup().name
  }
}

module appConfigurationDataReaderRoleAssignment '../../../../../Infrastructure/bicep/modules/role-assignment.bicep' = {
  name: 'appConfigurationDataReaderRoleAssignment'
  scope: resourceGroup(landingZoneResourceGroupName)
  params: {
    principalId: principalId
    roleDefinitionId: appConfigurationReaderRoleDefinition.name
    scopeId: landingZoneResourceGroupName
  }
}

module keyVaultSecretsUserRoleAssignment '../../../../../Infrastructure/bicep/modules/role-assignment.bicep' = {
  name: 'keyVaultSecretsUserRoleAssignment'
  scope: resourceGroup(landingZoneResourceGroupName)
  params: {
    principalId: principalId
    roleDefinitionId: keyVaultSecrestUserRoldeDefinition.name
    scopeId: landingZoneResourceGroupName
  }
}
