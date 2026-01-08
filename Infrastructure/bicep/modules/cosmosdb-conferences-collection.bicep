targetScope = 'resourceGroup'

@description('The name of the Cosmos DB account')
param accountName string

@description('The name of the MongoDB database')
param databaseName string

@description('Tags to apply to the collection')
param tags object = {}

resource cosmosAccount 'Microsoft.DocumentDB/databaseAccounts@2025-11-01-preview' existing = {
  name: accountName

  resource mongoDatabase 'mongodbDatabases@2025-11-01-preview' existing = {
    name: databaseName
  }
}

resource conferencesCollection 'Microsoft.DocumentDB/databaseAccounts/mongodbDatabases/collections@2025-11-01-preview' = {
  name: 'conferences'
  parent: cosmosAccount::mongoDatabase
  tags: tags
  properties: {
    resource: {
      id: 'conferences'
      shardKey: {
        _id: 'Hash'
      }
      indexes: [
        {
          key: {
            keys: [
              '_id'
            ]
          }
        }
        {
          key: {
            keys: [
              'startDate'
              'title'
            ]
          }
          options: {
            unique: false
          }
        }
        {
          key: {
            keys: [
              'endDate'
            ]
          }
          options: {
            unique: false
          }
        }
      ]
    }
    options: {
      throughput: 400
    }
  }
}

output collectionName string = conferencesCollection.name
output collectionId string = conferencesCollection.id
