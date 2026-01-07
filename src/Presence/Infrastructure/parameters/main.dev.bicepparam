using '../bicep/main.bicep'

param environmentName = 'dev'
param location = 'northeurope'
param baseName = 'attendr-presence'
param tags = {
  Environment: 'Development'
  Application: 'Attendr'
  Service: 'Presence'
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
param containerImage = 'your-registry.azurecr.io/attendr/attendr-presence-api'
