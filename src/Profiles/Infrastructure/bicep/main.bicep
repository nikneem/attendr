targetScope = 'subscription'

@description('The environment name (e.g., dev, staging, prod)')
param environmentName string

@description('The location for all resources')
param location string = 'northeurope'

@description('The base name for all resources')
param baseName string = 'attendr'

@description('Tags to apply to all resources')
param tags object = {}

@description('Landing zone resource names')
param landingzone object = {
  resourceGroupName: ''
  containerAppsEnvironmentName: ''
  appConfigurationName: ''
  keyVaultName: ''
  applicationInsightsName: ''
}

@description('Container registry server')
param containerRegistryServer string

@description('Container registry username')
@secure()
param containerRegistryUsername string

@description('Container registry password')
@secure()
param containerRegistryPassword string

@description('The container image to deploy')
param containerImage string

@description('CORS allowed origins')
param corsOrigins array = []

var resourceGroupName = 'rg-${baseName}-${environmentName}'

// Deploy Resource Group for Profiles service
resource resourceGroup 'Microsoft.Resources/resourceGroups@2024-03-01' = {
  name: resourceGroupName
  location: location
  tags: tags
}

module appResources 'resources.bicep' = {
  scope: resourceGroup
  params: {
    defaultResourceName: '${baseName}-${environmentName}'
    location: location
    tags: tags
    landingzone: landingzone
    containerImage: containerImage
    containerRegistryServer: containerRegistryServer
    containerRegistryUsername: containerRegistryUsername
    containerRegistryPassword: containerRegistryPassword
    corsOrigins: corsOrigins

    tableNames: [
      'profiles'
      'profiletopics'
    ]
  }
}
