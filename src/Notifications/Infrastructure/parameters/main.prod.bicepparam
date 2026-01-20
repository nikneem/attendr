using '../bicep/main.bicep'

param environmentName = 'prod'
param location = 'northeurope'
param baseName = 'attendr-notif'
param tags = {
  Environment: 'Production'
  Application: 'Attendr'
  Service: 'Notifications'
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
param containerImage = 'your-registry.azurecr.io/attendr/attendr-notifications-api'

// Container registry credentials
param containerRegistryServer = ''
param containerRegistryUsername = ''
param containerRegistryPassword = ''

// CORS allowed origins
param corsOrigins = [
  'https://attendr.live'
  'https://www.attendr.live'
]

// VAPID keys (provide via GitHub secrets in workflow)
param vapidPublicKey = ''

param vapidPrivateKey = ''
