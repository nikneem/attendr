targetScope = 'resourceGroup'

@description('The name of the App Configuration')
param appConfigurationName string

resource appConfiguration 'Microsoft.AppConfiguration/configurationStores@2024-05-01' existing = {
  name: appConfigurationName
}

output endpoint string = appConfiguration.properties.endpoint
