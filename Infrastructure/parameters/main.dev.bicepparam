using '../bicep/main.bicep'

param environmentName = 'dev'
param location = 'westeurope'
param baseName = 'attendr'
param tags = {
  Environment: 'Development'
  Application: 'Attendr'
  ManagedBy: 'Bicep'
}
