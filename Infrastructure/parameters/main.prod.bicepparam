using '../bicep/main.bicep'

param environmentName = 'prod'
param location = 'northeurope'
param baseName = 'attendr-alz'
param tags = {
  Environment: 'Production'
  Application: 'Attendr'
  ManagedBy: 'Bicep'
}
param containerRegistry = {
  subscriptionId: subscription().subscriptionId
  resourceGroupName: 'rg-acr-shared'
  name: 'acrattendr'
}
