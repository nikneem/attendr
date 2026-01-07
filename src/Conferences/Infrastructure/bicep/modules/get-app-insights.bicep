targetScope = 'resourceGroup'

@description('The name of Application Insights')
param applicationInsightsName string

resource applicationInsights 'Microsoft.Insights/components@2020-02-02' existing = {
  name: applicationInsightsName
}

output connectionString string = applicationInsights.properties.ConnectionString
