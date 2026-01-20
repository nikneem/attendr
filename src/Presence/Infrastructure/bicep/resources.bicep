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

@description('PostgreSQL admin login')
param postgresAdminLogin string = 'attendradmin'

@description('PostgreSQL admin password')
@secure()
param postgresAdminPassword string

@description('PostgreSQL database name')
param postgresDatabaseName string = 'attendr-presence'

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

// Deploy PostgreSQL flexible server
resource postgresServer 'Microsoft.DBforPostgreSQL/flexibleServers@2024-08-01' = {
  name: 'psql-${defaultResourceName}'
  location: location
  tags: tags
  sku: {
    name: 'Standard_B1ms'
    tier: 'Burstable'
  }
  properties: {
    version: '16'
    administratorLogin: postgresAdminLogin
    administratorLoginPassword: postgresAdminPassword
    storage: {
      storageSizeGB: 32
    }
    backup: {
      backupRetentionDays: 7
      geoRedundantBackup: 'Disabled'
    }
    highAvailability: {
      mode: 'Disabled'
    }
  }
}

// Create database
resource postgresDatabase 'Microsoft.DBforPostgreSQL/flexibleServers/databases@2024-08-01' = {
  parent: postgresServer
  name: postgresDatabaseName
}

// Allow Azure services to connect
resource postgresFirewallRule 'Microsoft.DBforPostgreSQL/flexibleServers/firewallRules@2024-08-01' = {
  parent: postgresServer
  name: 'AllowAllAzureServicesAndResourcesWithinAzureIps'
  properties: {
    startIpAddress: '0.0.0.0'
    endIpAddress: '0.0.0.0'
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
        external: false
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
        appId: 'presence-api'
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
          name: 'presence-api'
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
                path: '/alive'
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
                path: '/health'
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
                path: '/health'
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
  scope: resourceGroup(landingzone.resourceGroupName)
  params: {
    principalId: containerApp.identity.principalId
    appConfigurationName: landingzone.appConfigurationName
    keyVaultName: landingzone.keyVaultName
  }
}

// Store PostgreSQL connection string in landing zone Key Vault
module postgresSecret '../../../../Infrastructure/bicep/modules/keyvault-secret.bicep' = {
  scope: resourceGroup(landingzone.resourceGroupName)
  params: {
    keyVaultName: landingzone.keyVaultName
    secretName: 'PostgresPresenceConnectionString'
    secretValue: 'Host=${postgresServer.properties.fullyQualifiedDomainName};Database=${postgresDatabaseName};Username=${postgresAdminLogin};Password=${postgresAdminPassword};SSL Mode=Require'
  }
}

// Add PostgreSQL connection string to App Configuration as Key Vault reference
module postgresAppConfig '../../../../Infrastructure/bicep/modules/app-configuration-keyvault-reference.bicep' = {
  scope: resourceGroup(landingzone.resourceGroupName)
  params: {
    appConfigurationName: landingzone.appConfigurationName
    keyName: 'ConnectionStrings:attendr-presence'
    keyVaultName: landingzone.keyVaultName
    secretName: 'PostgresPresenceConnectionString'
  }
  dependsOn: [
    postgresSecret
  ]
}

output containerAppName string = containerApp.name
output containerAppFqdn string = containerApp.properties.configuration.ingress.fqdn
output managedIdentityPrincipalId string = containerApp.identity.principalId
output postgresServerName string = postgresServer.name
output postgresDatabaseName string = postgresDatabaseName
