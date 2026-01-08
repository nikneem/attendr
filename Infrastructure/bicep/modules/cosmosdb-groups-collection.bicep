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

resource groupsCollection 'Microsoft.DocumentDB/databaseAccounts/mongodbDatabases/collections@2025-11-01-preview' = {
  name: 'groups'
  parent: cosmosAccount::mongoDatabase
  tags: tags
  properties: {
    resource: {
      id: 'groups'
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
              'name'
            ]
          }
          options: {
            unique: false
          }
        }
        {
          key: {
            keys: [
              'settings.isSearchable'
            ]
          }
          options: {
            unique: false
          }
        }
        {
          key: {
            keys: [
              'settings.isSearchable'
              'name'
            ]
          }
          options: {
            unique: false
          }
        }
        {
          key: {
            keys: [
              'members.id'
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

output collectionName string = groupsCollection.name
output collectionId string = groupsCollection.id
