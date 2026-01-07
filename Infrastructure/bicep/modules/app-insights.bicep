targetScope = 'resourceGroup'

@description('The name of the Application Insights resource')
param name string

@description('The location for Application Insights')
param location string

@description('Tags to apply to Application Insights')
param tags object = {}

@description('The resource ID of the Log Analytics workspace')
param workspaceId string

@description('Application type')
@allowed([
  'web'
  'other'
])
param applicationType string = 'web'

resource appInsights 'Microsoft.Insights/components@2020-02-02' = {
  name: name
  location: location
  tags: tags
  kind: 'web'
  properties: {
    Application_Type: applicationType
    WorkspaceResourceId: workspaceId
    publicNetworkAccessForIngestion: 'Enabled'
    publicNetworkAccessForQuery: 'Enabled'
  }
}

output id string = appInsights.id
output instrumentationKey string = appInsights.properties.InstrumentationKey
output connectionString string = appInsights.properties.ConnectionString
