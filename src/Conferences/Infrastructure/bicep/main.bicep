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

var resourceGroupName = 'rg-${baseName}-${environmentName}'

// Deploy Resource Group for Conferences service
resource resourceGroup 'Microsoft.Resources/resourceGroups@2024-03-01' = {
  name: resourceGroupName
  location: location
  tags: tags
}

// Reference to landing zone resource group
resource landingZoneResourceGroup 'Microsoft.Resources/resourceGroups@2024-03-01' existing = {
  name: landingzone.resourceGroupName
}

// Get App Configuration endpoint
module appConfigurationEndpoint './modules/get-app-configuration.bicep' = {
  scope: landingZoneResourceGroup
  params: {
    appConfigurationName: landingzone.appConfigurationName
  }
}

// Get Application Insights connection string
module appInsightsConnectionString './modules/get-app-insights.bicep' = {
  scope: landingZoneResourceGroup
  params: {
    applicationInsightsName: landingzone.applicationInsightsName
  }
}

// Deploy the Conferences container app
module conferencesApp './modules/container-app.bicep' = {
  scope: resourceGroup
  params: {
    name: 'ca-${baseName}-${environmentName}'
    location: location
    tags: tags
    landingZoneResourceGroupName: landingzone.resourceGroupName
    containerAppsEnvironmentName: landingzone.containerAppsEnvironmentName
    containerImage: containerImage
    appConfigurationEndpoint: appConfigurationEndpoint.outputs.endpoint
    applicationInsightsConnectionString: appInsightsConnectionString.outputs.connectionString
    containerRegistryServer: containerRegistryServer
    containerRegistryUsername: containerRegistryUsername
    containerRegistryPassword: containerRegistryPassword
  }
}

// Assign permissions to the container app's system-assigned managed identity
module roleAssignments './modules/role-assignments.bicep' = {
  scope: landingZoneResourceGroup
  params: {
    principalId: conferencesApp.outputs.managedIdentityPrincipalId
    appConfigurationName: landingzone.appConfigurationName
    keyVaultName: landingzone.keyVaultName
  }
}

// Configure service integration endpoints in App Configuration
module serviceIntegration '../../../../Infrastructure/bicep/modules/app-configuration-service-integration.bicep' = {
  scope: landingZoneResourceGroup
  params: {
    appConfigurationName: landingzone.appConfigurationName
    environmentName: environmentName
    baseName: baseName
    serviceName: 'Conferences'
  }
}
output resourceGroupName string = resourceGroup.name
output containerAppName string = conferencesApp.outputs.name
output containerAppFqdn string = conferencesApp.outputs.fqdn
output managedIdentityPrincipalId string = conferencesApp.outputs.managedIdentityPrincipalId
