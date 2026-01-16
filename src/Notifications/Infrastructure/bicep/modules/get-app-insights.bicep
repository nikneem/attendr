targetScope = 'resourceGroup'

@description('The name of the Application Insights instance')
param applicationInsightsName string

resource appInsights 'Microsoft.Insights/components@2020-02-02' existing = {
  name: applicationInsightsName
}

output connectionString string = appInsights.properties.ConnectionString
