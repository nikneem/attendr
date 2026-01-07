using '../bicep/main.bicep'

param environmentName = 'prod'
param location = 'northeurope'
param baseName = 'attendr-landingzone'
param tags = {
  Environment: 'Production'
  Application: 'Attendr'
  ManagedBy: 'Bicep'
}
