targetScope = 'subscription'

@description('The environment name (e.g., dev, staging, prod)')
param environmentName string

@description('The location for all resources')
param location string = 'westeurope'

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
output mongoDbConnectionString string = resourceDeployment.outputs.mongoDbConnectionString
