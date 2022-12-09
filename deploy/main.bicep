/*
  Pre-requisite: Must have created at least one cognitive service resource in the target subscription. At this time, 
    that's the only way to accept the required 'Responsible AI terms'. You can delete that resource or run this to update it.
  
  Parameter switches...
    If manageAppRegistration = true, an application registration will be created (see here: https://reginbald.medium.com/creating-app-registration-with-arm-bicep-b1d48a287abb)
      Prerequisite: An appRegistrationUser managed identity must be created with the Application Administrator role (see here)
        appRegistrationUser is required: This is the name of the managed identity that can manage the app registration
        The following are ignored and retrieved from the app registration: appRegistrationAuthority, appRegistrationTenantId, appRegistrationClientId, appRegistrationClientSecret
    If manageAppRegistration = false, there is an existing app registration
      The following are required: appRegistrationAuthority, appRegistrationTenantId, appRegistrationClientId, appRegistrationClientSecret

    If linkToGitHub = true, the static web app will be linked to a GitHub repository.
      The following are required: linkToGithubRespository, linkToGithubRespositoryBranch
    If linkToGitHub = false, the static web app will not be linked
      The following are ignored

  Required parameter...
    fhirServer: The Url to your FHIR service

  Notes...
    If you try to remove the  cognitive services resource it gets soft deleted and cannot be re-used for 48 hours. 
    To be able to re-run the script immediately, purge the soft-deleted resource by running:
      az resource delete --ids /subscriptions/<subscriptionId>/providers/Microsoft.CognitiveServices/locations/<location>/resourceGroups/<resourceGroup>/deletedAccounts/<cognitiveServiceResourceName>
*/

param manageAppRegistration bool = false      // If true, will get tenantId, clientId, clientSecret from there
param appRegistrationUser string = ''
param appRegistrationAuthority string = ''
param appRegistrationTenantId string = ''
param appRegistrationClientId string = ''
@secure()
param appRegistrationClientSecret string = ''

param linkToGitHub bool = false               // If true, linkToGithubRespository and linkToGithubRespositoryBranch will be used to link to GitHub
param linkToGithubRespository string  = ''
param linkToGithubRespositoryBranch string = 'main'

param fhirServer string

param nameBase string = 'fast-pass'
param location string = resourceGroup().location
param textAnalyticsSku string = 'F0' // F0 or S1

module appRegistration 'AppRegistration.bicep' = if (manageAppRegistration) {
  name: '${nameBase}-ui'
  params: {
    appRegistrationUser: appRegistrationUser
    registrationName: '${nameBase}-ui'
    location: location
  }
}

resource computerVision 'Microsoft.CognitiveServices/accounts@2022-10-01' = {
  name: '${nameBase}-lang'
  location: location
  kind: 'TextAnalytics'
  sku: {
    name: textAnalyticsSku
  }
  identity: {
    type: 'SystemAssigned'
  }
  properties: {
    publicNetworkAccess: 'Enabled'
  }
}

resource swa 'Microsoft.Web/staticSites@2022-03-01' = {
  name: '${nameBase}-ui'
  location: location
  sku:  {
    name: 'Standard'
    tier: 'Standard'
  }

  dependsOn: [ appRegistration ]

  properties: {
    stagingEnvironmentPolicy: 'Enabled'
    allowConfigFileUpdates: true
    enterpriseGradeCdnStatus: 'Disabled'
    provider: linkToGitHub ? 'GitHub' : ''
    repositoryUrl: linkToGitHub ? linkToGithubRespository : ''
    branch: linkToGitHub ? linkToGithubRespositoryBranch : ''
  }
  resource swaAppSettings 'config@2022-03-01' = {
    name: 'appsettings'
    properties: {
      APIConfig__Authority: appRegistrationAuthority
      APIConfig__TenantId: appRegistration.outputs.tenantId ?? appRegistrationTenantId
      APIConfig__ClientId: appRegistration.outputs.clientId ?? appRegistrationClientId
      APIConfig__ClientSecret: appRegistration.outputs.clientSecret ?? appRegistrationClientSecret
      APIConfig__FhirScope: '${fhirServer}.default'
      APIConfig__FhirServerUri: fhirServer
      APIConfig__TextAnalyticsBase: computerVision.properties.endpoint
      APIConfig__TextAnalyticsKey: computerVision.listKeys().key1
      APPLICATIONINSIGHTS_CONNECTION_STRING: ai.properties.ConnectionString
    }
  }
}

resource ai 'Microsoft.Insights/components@2020-02-02' = {
  name: '${nameBase}-ui-ai'
  location: location
  kind: 'other'
  properties: {
    Application_Type: 'web'
  }
}
