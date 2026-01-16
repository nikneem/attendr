using '../bicep/main.bicep'

param environmentName = 'dev'
param location = 'northeurope'
param baseName = 'attendr-notif'
param tags = {
  Environment: 'Development'
  Application: 'Attendr'
  Service: 'Notifications'
  ManagedBy: 'Bicep'
}

// Landing zone resources
param landingzone = {
  resourceGroupName: 'rg-attendr-alz-dev'
  containerAppsEnvironmentName: 'cae-attendr-alz-dev'
  appConfigurationName: 'appconfig-attendr-alz-dev'
  keyVaultName: 'kv-attendr-alz-dev'
  applicationInsightsName: 'appi-attendr-alz-dev'
}

// Container image details
param containerImage = 'your-registry.azurecr.io/attendr/attendr-notifications-api:dev'

// Container registry credentials
param containerRegistryServer = ''
param containerRegistryUsername = ''
param containerRegistryPassword = ''

// CORS allowed origins
param corsOrigins = [
  'http://localhost:4200'
  'https://localhost:4200'
  'https://attendr-dev.azurewebsites.net'
]

// VAPID keys (provide via GitHub secrets in workflow)
param vapidPublicKey = ''

param vapidPrivateKey = ''
