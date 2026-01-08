targetScope = 'resourceGroup'

@description('The name of the user assigned identity')
param name string

@description('The location for the user assigned identity')
param location string

@description('Tags to apply to the user assigned identity')
param tags object = {}

@description('Container registry information')
param containerRegistry object

@description('The name of the App Configuration')
param appConfigurationName string

@description('The name of the Key Vault')
param keyVaultName string

// Create user assigned identity
resource userAssignedIdentity 'Microsoft.ManagedIdentity/userAssignedIdentities@2023-01-31' = {
  name: name
  location: location
  tags: tags
}

// Reference to container registry in its resource group
resource containerRegistryResource 'Microsoft.ContainerRegistry/registries@2023-07-01' existing = {
  name: containerRegistry.name
  scope: resourceGroup(containerRegistry.subscriptionId, containerRegistry.resourceGroupName)
}

// Reference to App Configuration
resource appConfiguration 'Microsoft.AppConfiguration/configurationStores@2024-05-01' existing = {
  name: appConfigurationName
}

// Reference to Key Vault
resource keyVault 'Microsoft.KeyVault/vaults@2023-07-01' existing = {
  name: keyVaultName
}

// Role definitions
var acrPullRoleId = '7f951dda-4ed3-4680-a7ca-43fe172d538d' // AcrPull
var appConfigurationDataReaderRoleId = '516239f1-63e1-4d78-a4de-a74fb236a071' // App Configuration Data Reader
var keyVaultSecretsUserRoleId = '4633458b-17de-408a-b874-0445c86b69e6' // Key Vault Secrets User

// Assign AcrPull role to the user assigned identity
module acrPullRoleAssignment './role-assignment.bicep' = {
  name: '${name}-acr-pull-role'
  scope: resourceGroup(containerRegistry.subscriptionId, containerRegistry.resourceGroupName)
  params: {
    principalId: userAssignedIdentity.properties.principalId
    roleDefinitionId: acrPullRoleId
    principalType: 'ServicePrincipal'
    scopeId: containerRegistryResource.id
  }
}

// Assign App Configuration Data Reader role
module appConfigRoleAssignment './role-assignment.bicep' = {
  name: '${name}-appconfig-reader-role'
  params: {
    principalId: userAssignedIdentity.properties.principalId
    roleDefinitionId: appConfigurationDataReaderRoleId
    principalType: 'ServicePrincipal'
    scopeId: appConfiguration.id
  }
}

// Assign Key Vault Secrets User role
module keyVaultRoleAssignment './role-assignment.bicep' = {
  name: '${name}-keyvault-secrets-role'
  params: {
    principalId: userAssignedIdentity.properties.principalId
    roleDefinitionId: keyVaultSecretsUserRoleId
    principalType: 'ServicePrincipal'
    scopeId: keyVault.id
  }
}

output id string = userAssignedIdentity.id
output principalId string = userAssignedIdentity.properties.principalId
output clientId string = userAssignedIdentity.properties.clientId
