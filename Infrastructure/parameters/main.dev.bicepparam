using '../bicep/main.bicep'

param environmentName = 'dev'
param location = 'northeurope'
param baseName = 'attendr-alz'
param frontendUrl = 'https://attendr.live'
param tags = {
  Environment: 'Development'
  Application: 'Attendr'
  ManagedBy: 'Bicep'
}
param containerRegistry = {
  subscriptionId: 'c2a162ec-4baf-44f5-a66e-0fb3b8618424'
  resourceGroupName: 'mvp-int-env'
  name: 'nvv54gsk4pteu'
}
param azureOpenAIApiKey = ''
param azureOpenAIDeploymentName = ''
param azureOpenAIEndpoint = ''
