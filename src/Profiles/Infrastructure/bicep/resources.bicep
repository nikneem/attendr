param defaultResourceName string
param location string = resourceGroup().location

@description('Tags to apply to the container app')
param tags object = {}

@description('Landing zone resource names')
param landingzone object = {
  resourceGroupName: ''
  containerAppsEnvironmentName: ''
  appConfigurationName: ''
  keyVaultName: ''
  applicationInsightsName: ''
}

@description('The container image to deploy')
param containerImage string

@description('Container registry server')
param containerRegistryServer string

@description('Container registry username')
@secure()
param containerRegistryUsername string

@description('Container registry password')
@secure()
param containerRegistryPassword string

@description('CORS allowed origins')
param corsOrigins array = []

param tableNames array = [
  'profiles'
]

// Get App Configuration endpoint
module appConfigurationEndpoint './modules/get-app-configuration.bicep' = {
  scope: resourceGroup(landingzone.resourceGroupName)
  params: {
    appConfigurationName: landingzone.appConfigurationName
  }
}

// Get Application Insights connection string
module appInsightsConnectionString './modules/get-app-insights.bicep' = {
  scope: resourceGroup(landingzone.resourceGroupName)
  params: {
    applicationInsightsName: landingzone.applicationInsightsName
  }
}

resource storageAccount 'Microsoft.Storage/storageAccounts@2025-06-01' = {
  name: uniqueString(defaultResourceName)
  location: location
  sku: {
    name: 'Standard_LRS'
  }
  kind: 'StorageV2'
  resource tableService 'tableServices@2025-06-01' = {
    name: 'default'
    resource tables 'tables@2025-06-01' = [
      for tableName in tableNames: {
        name: tableName
      }
    ]
  }
}

// Reference to Container Apps environment in landing zone
resource containerAppsEnvironment 'Microsoft.App/managedEnvironments@2024-03-01' existing = {
  scope: resourceGroup(landingzone.resourceGroupName)
  name: landingzone.containerAppsEnvironmentName
}

resource containerApp 'Microsoft.App/containerApps@2024-03-01' = {
  name: 'ca-${defaultResourceName}'
  location: location
  tags: tags
  identity: {
    type: 'SystemAssigned'
  }
  properties: {
    managedEnvironmentId: containerAppsEnvironment.id
    configuration: {
      registries: [
        {
          server: containerRegistryServer
          username: containerRegistryUsername
          passwordSecretRef: 'registry-password'
        }
      ]
      ingress: {
        external: true
        targetPort: 8080
        transport: 'http'
        allowInsecure: false
        corsPolicy: {
          allowedOrigins: corsOrigins
          allowedMethods: [
            'GET'
            'POST'
            'PUT'
            'DELETE'
            'OPTIONS'
          ]
          allowedHeaders: [
            '*'
          ]
          allowCredentials: true
        }
      }
      dapr: {
        enabled: true
        appId: 'profiles-api'
        appProtocol: 'http'
        appPort: 8080
        enableApiLogging: true
      }
      secrets: [
        {
          name: 'appinsights-connection-string'
          value: appInsightsConnectionString.outputs.connectionString
        }
        {
          name: 'registry-password'
          value: containerRegistryPassword
        }
      ]
    }
    template: {
      containers: [
        {
          name: 'profiles-api'
          image: '${containerRegistryServer}/${containerImage}'
          resources: {
            cpu: json('0.25')
            memory: '0.5Gi'
          }
          env: [
            {
              name: 'ASPNETCORE_ENVIRONMENT'
              value: 'Production'
            }
            {
              name: 'ASPNETCORE_HTTP_PORTS'
              value: '8080'
            }
            {
              name: 'AppConfiguration__Endpoint'
              value: appConfigurationEndpoint.outputs.endpoint
            }
            {
              name: 'APPLICATIONINSIGHTS_CONNECTION_STRING'
              secretRef: 'appinsights-connection-string'
            }
          ]
          probes: [
            {
              type: 'Liveness'
              httpGet: {
                path: '/health/live'
                port: 8080
                scheme: 'HTTP'
              }
              initialDelaySeconds: 10
              periodSeconds: 10
              failureThreshold: 3
            }
            {
              type: 'Readiness'
              httpGet: {
                path: '/health/ready'
                port: 8080
                scheme: 'HTTP'
              }
              initialDelaySeconds: 5
              periodSeconds: 5
              failureThreshold: 3
            }
            {
              type: 'Startup'
              httpGet: {
                path: '/health/startup'
                port: 8080
                scheme: 'HTTP'
              }
              initialDelaySeconds: 0
              periodSeconds: 2
              failureThreshold: 30
            }
          ]
        }
      ]
      scale: {
        minReplicas: 1
        maxReplicas: 10
        rules: [
          {
            name: 'http-scaling'
            http: {
              metadata: {
                concurrentRequests: '100'
              }
            }
          }
        ]
      }
    }
  }
}

module roleAssignments './modules/role-assignments.bicep' = {
  name: 'roleAssignments'
  params: {
    principalId: containerApp.identity.principalId
    landingZoneResourceGroupName: landingzone.resourceGroupName
  }
}

module appConfigurationValues '../../../../Infrastructure/bicep/modules/azure-app-configuration-value.bicep' = [
  for tableName in tableNames: {
    name: 'appConfigurationValues${tableName}'
    scope: resourceGroup(landingzone.resourceGroupName)
    params: {
      appConfigurationName: landingzone.appConfigurationName
      name: 'Aspire:Azure:Data:Tables:${tableName}:ServiceUri'
      value: 'https://${storageAccount.name}.table.${environment().suffixes.storage}'
    }
  }
]
