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

// Deploy the Profiles container app
module profilesApp './modules/container-app.bicep' = {
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
  }
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

// Assign permissions to the container app managed identity
module permissions './modules/role-assignments.bicep' = {
  scope: landingZoneResourceGroup
  params: {
    principalId: profilesApp.outputs.managedIdentityPrincipalId
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
    serviceName: 'Profiles'
  }
}
output resourceGroupName string = resourceGroup.name
output containerAppName string = profilesApp.outputs.name
output containerAppFqdn string = profilesApp.outputs.fqdn
output managedIdentityPrincipalId string = profilesApp.outputs.managedIdentityPrincipalId
