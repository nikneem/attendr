targetScope = 'resourceGroup'

@description('The name of the Log Analytics workspace')
param name string

@description('The location for the Log Analytics workspace')
param location string

@description('Tags to apply to the workspace')
param tags object = {}

@description('SKU name')
@allowed([
  'PerGB2018'
  'Free'
  'Standalone'
  'PerNode'
  'Standard'
  'Premium'
])
param skuName string = 'PerGB2018'

@description('Retention in days')
@minValue(30)
@maxValue(730)
param retentionInDays int = 30

resource logAnalytics 'Microsoft.OperationalInsights/workspaces@2025-07-01' = {
  name: name
  location: location
  tags: tags
  properties: {
    sku: {
      name: skuName
    }
    retentionInDays: retentionInDays
    publicNetworkAccessForIngestion: 'Enabled'
    publicNetworkAccessForQuery: 'Enabled'
  }
}

output id string = logAnalytics.id
output customerId string = logAnalytics.properties.customerId
