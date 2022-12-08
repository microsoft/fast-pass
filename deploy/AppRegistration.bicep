param appRegistrationUser string
param registrationName string
param location string = resourceGroup().location
param currentTime string = utcNow()

resource upsertAppRegistrationScript 'Microsoft.Resources/deploymentScripts@2020-10-01' = {
  name: registrationName
  location: location
  kind: 'AzurePowerShell'
  identity: {
    type: 'UserAssigned'
    userAssignedIdentities: {
      '${resourceId('${resourceGroup().name}', 'Microsoft.ManagedIdentity/userAssignedIdentities', '${appRegistrationUser}')}': {}
    }
  }
  properties: {
    azPowerShellVersion: '5.0'
    arguments: '-resourceName "${registrationName}"'
    scriptContent: '''
      param([string] $resourceName)
      $access = Get-AzAccessToken -ResourceUrl https://graph.microsoft.com 
      $token = $access.Token
      $tenantId = $access.TenantId
      $headers = @{'Content-Type' = 'application/json'; 'Authorization' = 'Bearer ' + $token}

      $template = @{
        displayName = $resourceName
        requiredResourceAccess = @(
          @{
            resourceAppId = "00000003-0000-0000-c000-000000000000"
            resourceAccess = @(
              @{
                id = "e1fe6dd8-ba31-4d61-89e7-88639da4683d"
                type = "Scope"
              }
            )
          }
        )
        signInAudience = "AzureADMyOrg"
      }
      
      $app = (Invoke-RestMethod -Method Get -Headers $headers -Uri "https://graph.microsoft.com/beta/applications?filter=displayName eq '$($resourceName)'").value
      $principal = @{}
      if ($app) {
        $ignore = Invoke-RestMethod -Method Patch -Headers $headers -Uri "https://graph.microsoft.com/beta/applications/$($app.id)" -Body ($template | ConvertTo-Json -Depth 10)
        $principal = (Invoke-RestMethod -Method Get -Headers $headers -Uri "https://graph.microsoft.com/beta/servicePrincipals?filter=appId eq '$($app.appId)'").value
      } else {
        $app = (Invoke-RestMethod -Method Post -Headers $headers -Uri "https://graph.microsoft.com/beta/applications" -Body ($template | ConvertTo-Json -Depth 10))
        $principal = Invoke-RestMethod -Method POST -Headers $headers -Uri  "https://graph.microsoft.com/beta/servicePrincipals" -Body (@{ "appId" = $app.appId } | ConvertTo-Json)
      }
      
      $app = (Invoke-RestMethod -Method Get -Headers $headers -Uri "https://graph.microsoft.com/beta/applications/$($app.id)")
      
      foreach ($password in $app.passwordCredentials) {
        Write-Host "Deleting secret with id: $($password.keyId)"
        $body = @{
          "keyId" = $password.keyId
        }
        $ignore = Invoke-RestMethod -Method POST -Headers $headers -Uri "https://graph.microsoft.com/beta/applications/$($app.id)/removePassword" -Body ($body | ConvertTo-Json)
      }
      
      $body = @{
        "passwordCredential" = @{
          "displayName"= "Client Secret"
        }
      }
      $secret = (Invoke-RestMethod -Method POST -Headers $headers -Uri  "https://graph.microsoft.com/beta/applications/$($app.id)/addPassword" -Body ($body | ConvertTo-Json)).secretText
      
      $DeploymentScriptOutputs = @{}
      $DeploymentScriptOutputs['objectId'] = $app.id
      $DeploymentScriptOutputs['clientId'] = $app.appId
      $DeploymentScriptOutputs['clientSecret'] = $secret
      $DeploymentScriptOutputs['principalId'] = $principal.id
      $DeploymentScriptOutputs['tenantId'] = $tenantId

    '''
    cleanupPreference: 'OnSuccess'
    retentionInterval: 'P1D'
    forceUpdateTag: currentTime // ensures script will run every time
  }
}

output objectId string = upsertAppRegistrationScript.properties.outputs.objectId
output clientId string = upsertAppRegistrationScript.properties.outputs.clientId
output clientSecret string = upsertAppRegistrationScript.properties.outputs.clientSecret
output principalId string = upsertAppRegistrationScript.properties.outputs.principalId
output tenantId string = upsertAppRegistrationScript.properties.outputs.tenantId
