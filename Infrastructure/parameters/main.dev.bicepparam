using '../bicep/main.bicep'

param environmentName = 'dev'
param location = 'northeurope'
param baseName = 'attendr-alz'
param tags = {
  Environment: 'Development'
  Application: 'Attendr'
  ManagedBy: 'Bicep'
}
