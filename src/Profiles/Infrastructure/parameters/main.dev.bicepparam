using '../bicep/main.bicep'

param environmentName = 'dev'
param location = 'northeurope'
param baseName = 'attendr-profiles'
param tags = {
  Environment: 'Development'
  Application: 'Attendr'
  Service: 'Profiles'
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
param containerImage = 'your-registry.azurecr.io/attendr/attendr-profiles-api'

// Container registry credentials
param containerRegistryServer = ''
param containerRegistryUsername = ''
param containerRegistryPassword = ''

// CORS allowed origins
param corsOrigins = [
  'http://localhost:4200'
]
