using '../bicep/main.bicep'

param environmentName = 'prod'
param location = 'northeurope'
param baseName = 'attendr-pres'
param tags = {
  Environment: 'Production'
  Application: 'Attendr'
  Service: 'Presence'
  ManagedBy: 'Bicep'
}

// Landing zone resources
param landingzone = {
  resourceGroupName: 'rg-attendr-alz-prod'
  containerAppsEnvironmentName: 'cae-attendr-alz-prod'
  appConfigurationName: 'appconfig-attendr-alz-prod-mlp2ft'
  keyVaultName: 'kv-attendr-alz-prod'
  applicationInsightsName: 'appi-attendr-alz-prod'
}

// Container image details
param containerImage = 'your-registry.azurecr.io/attendr/attendr-presence-api'
