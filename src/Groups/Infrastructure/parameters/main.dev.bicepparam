using '../bicep/main.bicep'

param environmentName = 'dev'
param location = 'northeurope'
param baseName = 'attendr-groups'
param tags = {
  Environment: 'Development'
  Application: 'Attendr'
  Service: 'Groups'
  ManagedBy: 'Bicep'
}

// Landing zone resources
param landingzone = {
  resourceGroupName: 'rg-attendr-dev'
  containerAppsEnvironmentName: 'cae-attendr-dev'
  appConfigurationName: 'appconfig-attendr-dev-xxxxxx'
  keyVaultName: 'kv-attendr-dev-xxxxxx'
  applicationInsightsName: 'appi-attendr-dev'
}

// Container image details
param containerImage = 'your-registry.azurecr.io/attendr/attendr-groups-api'
