targetScope = 'resourceGroup'

@description('The name of the App Configuration store')
param appConfigurationName string

@description('The environment name (e.g., dev, staging, prod)')
param environmentName string

@description('The base name for all resources')
param baseName string = 'attendr'

@description('The service name (e.g., profiles, groups, conferences, presence)')
param serviceName string

resource appConfig 'Microsoft.AppConfiguration/configurationStores@2025-06-01-preview' existing = {
  name: appConfigurationName
}

// Add service integration endpoint for container-to-container communication
resource serviceIntegrationKeyValue 'Microsoft.AppConfiguration/configurationStores/keyValues@2025-06-01-preview' = {
  parent: appConfig
  name: 'Attendr:Integration:${serviceName}'
  properties: {
    value: 'http://ca-${baseName}-${environmentName}'
    contentType: 'text/plain'
  }
}

output serviceEndpoint string = serviceIntegrationKeyValue.properties.value
