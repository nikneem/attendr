targetScope = 'resourceGroup'

@description('The name of the Container Apps environment')
param name string

@description('The location for the Container Apps environment')
param location string

@description('Tags to apply to the environment')
param tags object = {}

@description('The resource ID of the Log Analytics workspace')
param logAnalyticsWorkspaceId string

resource logAnalytics 'Microsoft.OperationalInsights/workspaces@2023-09-01' existing = {
  name: last(split(logAnalyticsWorkspaceId, '/'))
}

resource containerAppsEnvironment 'Microsoft.App/managedEnvironments@2025-10-02-preview' = {
  name: name
  location: location
  tags: tags
  properties: {
    appLogsConfiguration: {
      destination: 'log-analytics'
      logAnalyticsConfiguration: {
        customerId: logAnalytics.properties.customerId
        sharedKey: logAnalytics.listKeys().primarySharedKey
      }
    }
    zoneRedundant: false
  }
}

output id string = containerAppsEnvironment.id
output name string = containerAppsEnvironment.name
output defaultDomain string = containerAppsEnvironment.properties.defaultDomain
