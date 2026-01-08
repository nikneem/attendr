targetScope = 'subscription'
param location string
@allowed([
  'dev'
  'prd'
])
param environmentName string
param systemName string
param hostnames array

var defaultResourceName = '${systemName}-${environmentName}'
var targetResourceGroupName = 'rg-${defaultResourceName}'

resource targetResourcegroup 'Microsoft.Resources/resourceGroups@2021-04-01' = {
  name: targetResourceGroupName
  location: location
}

module resources './resources.bicep' = {
  name: 'resources'
  scope: targetResourcegroup
  params: {
    location: location
    defaultResourceName: defaultResourceName
    hostnames: hostnames
  }
}

output staticWebAppName string = resources.outputs.staticWebAppName
output resourceGroupName string = targetResourcegroup.name
