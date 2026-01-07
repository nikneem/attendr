targetScope = 'resourceGroup'

@description('The name of the Cosmos DB account')
param accountName string

@description('The name of the MongoDB database')
param databaseName string

@description('The location for the Cosmos DB account')
param location string

@description('Tags to apply to the Cosmos DB account')
param tags object = {}

@description('Enable free tier')
param enableFreeTier bool = true

resource cosmosAccount 'Microsoft.DocumentDB/databaseAccounts@2025-11-01-preview' = {
  name: accountName
  location: location
  tags: tags
  kind: 'MongoDB'
  properties: {
    databaseAccountOfferType: 'Standard'
    enableFreeTier: enableFreeTier
    apiProperties: {
      serverVersion: '4.2'
    }
    consistencyPolicy: {
      defaultConsistencyLevel: 'Session'
    }
    locations: [
      {
        locationName: location
        failoverPriority: 0
        isZoneRedundant: false
      }
    ]
    capabilities: [
      {
        name: 'EnableMongo'
      }
    ]
    backupPolicy: {
      type: 'Periodic'
      periodicModeProperties: {
        backupIntervalInMinutes: 240
        backupRetentionIntervalInHours: 8
        backupStorageRedundancy: 'Local'
      }
    }
    publicNetworkAccess: 'Enabled'
    networkAclBypass: 'AzureServices'
  }
  resource mongoDatabase 'mongodbDatabases@2025-11-01-preview' = {
    name: databaseName
    properties: {
      resource: {
        id: databaseName
      }
    }
  }
}

var connectionStringValue = 'mongodb://${cosmosAccount.name}:${cosmosAccount.listKeys().primaryMasterKey}@${cosmosAccount.name}.mongo.cosmos.azure.com:10255/?ssl=true&replicaSet=globaldb&retrywrites=false&maxIdleTimeMS=120000'

output id string = cosmosAccount.id
output endpoint string = cosmosAccount.properties.documentEndpoint
output connectionString string = connectionStringValue
