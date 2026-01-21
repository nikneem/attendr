targetScope = 'resourceGroup'

@description('The name of the Key Vault')
param keyVaultName string

@description('The name of the App Configuration')
param appConfigurationName string

@description('Azure OpenAI API Key')
@secure()
param apiKey string

@description('Azure OpenAI Deployment Name')
param deploymentName string

@description('Azure OpenAI Endpoint')
param endpoint string

resource keyVault 'Microsoft.KeyVault/vaults@2023-07-01' existing = {
  name: keyVaultName
}

resource appConfiguration 'Microsoft.AppConfiguration/configurationStores@2024-05-01' existing = {
  name: appConfigurationName
}

// Store API Key in Key Vault
resource apiKeySecret 'Microsoft.KeyVault/vaults/secrets@2023-07-01' = {
  parent: keyVault
  name: 'AzureOpenAIApiKey'
  properties: {
    value: apiKey
    contentType: 'text/plain'
  }
}

// Store Deployment Name in Key Vault
resource deploymentNameSecret 'Microsoft.KeyVault/vaults/secrets@2023-07-01' = {
  parent: keyVault
  name: 'AzureOpenAIDeploymentName'
  properties: {
    value: deploymentName
    contentType: 'text/plain'
  }
}

// Store Endpoint in Key Vault
resource endpointSecret 'Microsoft.KeyVault/vaults/secrets@2023-07-01' = {
  parent: keyVault
  name: 'AzureOpenAIEndpoint'
  properties: {
    value: endpoint
    contentType: 'text/plain'
  }
}

// Create App Configuration key with Key Vault reference for API Key
resource appConfigApiKey 'Microsoft.AppConfiguration/configurationStores/keyValues@2024-05-01' = {
  parent: appConfiguration
  name: 'AzureOpenAI:ApiKey'
  properties: {
    contentType: 'application/vnd.microsoft.appconfig.keyvaultref+json;charset=utf-8'
    value: '{"uri":"${apiKeySecret.properties.secretUri}"}'
  }
}

// Create App Configuration key with Key Vault reference for Deployment Name
resource appConfigDeploymentName 'Microsoft.AppConfiguration/configurationStores/keyValues@2024-05-01' = {
  parent: appConfiguration
  name: 'AzureOpenAI:DeploymentName'
  properties: {
    contentType: 'application/vnd.microsoft.appconfig.keyvaultref+json;charset=utf-8'
    value: '{"uri":"${deploymentNameSecret.properties.secretUri}"}'
  }
}

// Create App Configuration key with Key Vault reference for Endpoint
resource appConfigEndpoint 'Microsoft.AppConfiguration/configurationStores/keyValues@2024-05-01' = {
  parent: appConfiguration
  name: 'AzureOpenAI:Endpoint'
  properties: {
    contentType: 'application/vnd.microsoft.appconfig.keyvaultref+json;charset=utf-8'
    value: '{"uri":"${endpointSecret.properties.secretUri}"}'
  }
}
