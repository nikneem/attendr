using '../bicep/main.bicep'

param environmentName = 'prod'
param location = 'westeurope'
param baseName = 'attendr'
param tags = {
  Environment: 'Production'
  Application: 'Attendr'
  ManagedBy: 'Bicep'
}
