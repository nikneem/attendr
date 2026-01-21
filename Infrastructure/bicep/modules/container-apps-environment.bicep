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
  resource httpRouteConfig 'httpRouteConfigs@2025-10-02-preview' = {
    name: 'gateway'
    properties: {
      rules: [
        {
          description: 'Route /profiles to Profiles API'
          routes: [
            {
              match: {
                pathSeparatedPrefix: '/profiles'
              }
              action: {
                prefixRewrite: '/api/profiles'
              }
            }
          ]
          targets: [
            {
              containerApp: 'ca-attendr-profiles-prod'
            }
          ]
        }
        {
          description: 'Route /conferences to Conferences API'
          routes: [
            {
              match: {
                pathSeparatedPrefix: '/conferences'
              }
              action: {
                prefixRewrite: '/api/conferences'
              }
            }
          ]
          targets: [
            {
              containerApp: 'ca-attendr-conf-prod'
            }
          ]
        }
        {
          description: 'Route /topics to Topics API'
          routes: [
            {
              match: {
                pathSeparatedPrefix: '/topics'
              }
              action: {
                prefixRewrite: '/api/topics'
              }
            }
          ]
          targets: [
            {
              containerApp: 'ca-attendr-conf-prod'
            }
          ]
        }
        {
          description: 'Route /groups to Groups API'
          routes: [
            {
              match: {
                pathSeparatedPrefix: '/groups'
              }
              action: {
                prefixRewrite: '/api/groups'
              }
            }
          ]
          targets: [
            {
              containerApp: 'ca-attendr-groups-prod'
            }
          ]
        }
        {
          description: 'Route /presence to Presence API'
          routes: [
            {
              match: {
                pathSeparatedPrefix: '/presence'
              }
              action: {
                prefixRewrite: '/api/presence'
              }
            }
          ]
          targets: [
            {
              containerApp: 'ca-attendr-pres-prod'
            }
          ]
        }
        {
          description: 'Route /notifications to Notifications API'
          routes: [
            {
              match: {
                pathSeparatedPrefix: '/notifications'
              }
              action: {
                prefixRewrite: '/api/notifications'
              }
            }
          ]
          targets: [
            {
              containerApp: 'ca-attendr-notif-prod'
            }
          ]
        }
      ]
    }
  }
}

output id string = containerAppsEnvironment.id
output name string = containerAppsEnvironment.name
output defaultDomain string = containerAppsEnvironment.properties.defaultDomain
