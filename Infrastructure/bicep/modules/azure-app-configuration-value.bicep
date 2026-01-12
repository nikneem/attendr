@description('The name of the App Configuration store')
param appConfigurationName string

@description('Name of the configuration value')
param name string

@description('The value to store in App Configuration')
param value string

@description('Tag to use, this tag is optional')
param tag string = ''

resource appConfig 'Microsoft.AppConfiguration/configurationStores@2025-06-01-preview' existing = {
  name: appConfigurationName
}

resource configKeyValue 'Microsoft.AppConfiguration/configurationStores/keyValues@2025-06-01-preview' = {
  parent: appConfig
  name: name
  properties: {
    value: value
    contentType: 'text/plain'
    tags: empty(tag)
      ? {}
      : {
          tag: tag
        }
  }
}
