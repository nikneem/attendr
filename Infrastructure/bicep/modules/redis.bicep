targetScope = 'resourceGroup'

@description('The name of the Redis Cache')
param name string

@description('The location for the Redis Cache')
param location string

@description('Tags to apply to the Redis Cache')
param tags object = {}

@description('SKU name - Basic is cheapest')
@allowed([
  'Basic'
  'Standard'
  'Premium'
])
param skuName string = 'Basic'

@description('SKU family')
@allowed([
  'C'
  'P'
])
param skuFamily string = 'C'

@description('SKU capacity (0-6 for Basic/Standard, 1-5 for Premium)')
@minValue(0)
@maxValue(6)
param skuCapacity int = 0

resource redisCache 'Microsoft.Cache/redis@2024-11-01' = {
  name: name
  location: location
  tags: tags
  properties: {
    sku: {
      name: skuName
      family: skuFamily
      capacity: skuCapacity
    }
    enableNonSslPort: false
    minimumTlsVersion: '1.2'
    publicNetworkAccess: 'Enabled'
    redisConfiguration: {
      'maxmemory-policy': 'allkeys-lru'
    }
  }
}

output id string = redisCache.id
output name string = redisCache.name
output hostName string = redisCache.properties.hostName
output sslPort int = redisCache.properties.sslPort
