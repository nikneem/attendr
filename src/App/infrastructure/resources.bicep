param location string
param defaultResourceName string
param hostnames array

resource staticWebApp 'Microsoft.Web/staticSites@2024-04-01' = {
  name: '${defaultResourceName}-swa'
  location: location
  sku: {
    name: 'Standard'
  }
  properties: {
    repositoryUrl: 'https://github.com/nikneem/attendr'
    branch: 'main'
    stagingEnvironmentPolicy: 'Disabled'
    allowConfigFileUpdates: true
    provider: 'GitHub'
    enterpriseGradeCdnStatus: 'Disabled'
  }
  resource customDomain 'customDomains' = [
    for hostname in hostnames: {
      name: hostname
    }
  ]
}

output staticWebAppName string = staticWebApp.name
