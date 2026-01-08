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

@description('Container registry information')
param containerRegistry object = {
  subscriptionId: ''
  resourceGroupName: ''
  name: ''
}

@description('The container image to deploy')
param containerImage string

var resourceGroupName = 'rg-${baseName}-${environmentName}'

// Deploy Resource Group for Profiles service
resource resourceGroup 'Microsoft.Resources/resourceGroups@2024-03-01' = {
  name: resourceGroupName
  location: location
  tags: tags
}

// Reference to landing zone resource group
resource landingZoneResourceGroup 'Microsoft.Resources/resourceGroups@2024-03-01' existing = {
  name: landingzone.resourceGroupName
}

// Create user assigned identity with all required permissions
module userIdentity './modules/user-assigned-identity.bicep' = {
  scope: resourceGroup
  params: {
    name: 'id-${baseName}-${environmentName}'
    location: location
    tags: tags
    containerRegistry: containerRegistry
    appConfigurationName: landingzone.appConfigurationName
    keyVaultName: landingzone.keyVaultName
    landingZoneResourceGroupName: landingzone.resourceGroupName
  }
}

// Deploy the Profiles container app
module profilesApp './modules/container-app.bicep' = {
  scope: resourceGroup
  params: {
    name: 'ca-${baseName}-profiles-${environmentName}'
    location: location
    tags: tags
    landingZoneResourceGroupName: landingzone.resourceGroupName
    containerAppsEnvironmentName: landingzone.containerAppsEnvironmentName
    containerImage: containerImage
    appConfigurationEndpoint: appConfigurationEndpoint.outputs.endpoint
    applicationInsightsConnectionString: appInsightsConnectionString.outputs.connectionString
    userAssignedIdentityId: userIdentity.outputs.id
  }
  dependsOn: [
    userIdentity
  ]
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

// Configure service integration endpoints in App Configuration
module serviceIntegration '../../../../Infrastructure/bicep/modules/app-configuration-service-integration.bicep' = {
  scope: landingZoneResourceGroup
  params: {
    appConfigurationName: landingzone.appConfigurationName
    environmentName: environmentName
    baseName: baseName
    serviceName: 'Profiles'
  }
}
output resourceGroupName string = resourceGroup.name
output containerAppName string = profilesApp.outputs.name
output containerAppFqdn string = profilesApp.outputs.fqdn
output managedIdentityPrincipalId string = profilesApp.outputs.managedIdentityPrincipalId
