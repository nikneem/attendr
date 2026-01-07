targetScope = 'subscription'

@description('The environment name (e.g., dev, staging, prod)')
param environmentName string

@description('The location for all resources')
param location string = deployment().location

@description('The base name for all resources')
param baseName string = 'attendr'

@description('Tags to apply to all resources')
param tags object = {}

var resourceGroupName = 'rg-${baseName}-${environmentName}'

// Deploy Resource Group
resource resourceGroup 'Microsoft.Resources/resourceGroups@2024-03-01' = {
  name: resourceGroupName
  location: location
  tags: tags
}

// Deploy resources into the resource group
module resourceDeployment './modules/resources.bicep' = {
  scope: resourceGroup
  params: {
    environmentName: environmentName
    location: location
    baseName: baseName
    tags: tags
  }
}

output resourceGroupName string = resourceGroup.name
output keyVaultName string = resourceDeployment.outputs.keyVaultName
output appConfigName string = resourceDeployment.outputs.appConfigName
output containerAppsEnvironmentName string = resourceDeployment.outputs.containerAppsEnvironmentName
output serviceBusNamespace string = resourceDeployment.outputs.serviceBusNamespace
output redisCacheName string = resourceDeployment.outputs.redisCacheName
output daprPubSubComponentName string = resourceDeployment.outputs.daprPubSubComponentName
output daprStateStoreComponentName string = resourceDeployment.outputs.daprStateStoreComponentName
