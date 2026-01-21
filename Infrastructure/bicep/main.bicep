targetScope = 'subscription'

@description('The environment name (e.g., dev, staging, prod)')
param environmentName string

@description('The location for all resources')
param location string = deployment().location

@description('The base name for all resources')
param baseName string = 'attendr'

@description('Tags to apply to all resources')
param tags object = {}

@description('Base url of the frontend application')
param frontendUrl string

@description('Container registry information')
param containerRegistry object = {
  subscriptionId: ''
  resourceGroupName: ''
  name: ''
}

@description('Azure OpenAI API Key')
@secure()
param azureOpenAIApiKey string = ''

@description('Azure OpenAI Deployment Name')
param azureOpenAIDeploymentName string = ''

@description('Azure OpenAI Endpoint')
param azureOpenAIEndpoint string = ''

var resourceGroupName = 'rg-${baseName}-${environmentName}'

// Deploy Resource Group
resource resourceGroup 'Microsoft.Resources/resourceGroups@2024-03-01' = {
  name: resourceGroupName
  location: location
  tags: tags
}

// Deploy resources into the resource group
module resourceDeployment './modules/resources.bicep' = {
  scope: resourceGroup
  params: {
    environmentName: environmentName
    location: location
    baseName: baseName
    tags: tags
    containerRegistry: containerRegistry
    frontendUrl: frontendUrl
    azureOpenAIApiKey: azureOpenAIApiKey
    azureOpenAIDeploymentName: azureOpenAIDeploymentName
    azureOpenAIEndpoint: azureOpenAIEndpoint
  }
}

output resourceGroupName string = resourceGroup.name
output keyVaultName string = resourceDeployment.outputs.keyVaultName
output appConfigName string = resourceDeployment.outputs.appConfigName
output containerAppsEnvironmentName string = resourceDeployment.outputs.containerAppsEnvironmentName
output serviceBusNamespace string = resourceDeployment.outputs.serviceBusNamespace
output redisCacheName string = resourceDeployment.outputs.redisCacheName
output daprPubSubComponentName string = resourceDeployment.outputs.daprPubSubComponentName
output daprStateStoreComponentName string = resourceDeployment.outputs.daprStateStoreComponentName
output userAssignedIdentityId string = resourceDeployment.outputs.userAssignedIdentityId
output userAssignedIdentityPrincipalId string = resourceDeployment.outputs.userAssignedIdentityPrincipalId
output userAssignedIdentityClientId string = resourceDeployment.outputs.userAssignedIdentityClientId
output containerAppsDefaultDomainName string = resourceDeployment.outputs.containerAppsDefaultDomainName
