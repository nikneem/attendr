targetScope = 'resourceGroup'

@description('The name of the App Configuration store')
param name string

@description('The location for the App Configuration store')
param location string

@description('Tags to apply to the App Configuration store')
param tags object = {}

resource appConfig 'Microsoft.AppConfiguration/configurationStores@2024-05-01' = {
  name: name
  location: location
  tags: tags
  identity: {
    type: 'SystemAssigned'
  }
  sku: {
    name: 'standard'
  }
  properties: {
    enablePurgeProtection: false
    publicNetworkAccess: 'Enabled'
  }
}

output id string = appConfig.id
output name string = appConfig.name
output endpoint string = appConfig.properties.endpoint
