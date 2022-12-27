# Project Overview
An application to assist healthcare users in forming new patient records with both accuracy and efficiency.

## Healthcare Use Cases
We built this project over two different phases, each representing two different possible operations of health care data.

### Medical Intake
Patient presents medical insurance card at the doctor's office. An image of the insurance card is uploaded to the system and sent to the OCR service to extract patient biographic information as well as insurance coverage information.

The information extracted by the OCR is presented on a form for a intake specialist to verify, correct, and then submit to the EMR system using the FHIR API exposed by the Azure Health Data Service.

### Discharge Summary Analysis
For a given patient, we obtain a discharge summary and extract relevant medical information. Using a custom web application, we submit the contents of the discharge summary to the Text Analytics for Healthcare service.

Upon successful processing, we extract the entities identified as well as the FHIR bundle. The extracted entities are then displayed on a web page on a custom web
application  to conduct quick spot/quality check. Once the data is validated, the extracted FHIR bundle is submitted to the Azure Health Data Services for saving into the EMR using the Azure Health Data Service FHIR API.

# Power Platform Version

## Prerequisites

### Azure API for FHIR
The healthcare industry is rapidly transforming health data to the emerging standard of FHIR® (Fast Healthcare Interoperability Resources). FHIR enables a robust, extensible data model with standardized semantics and data exchange that enables all systems using FHIR to work together. FHIR also enables the rapid exchange of data in applications. Backed by a managed PaaS [Azure API for FHIR](https://docs.microsoft.com/en-us/azure/healthcare-apis/overview) offering, FHIR also provides a scalable and secure environment for the management and storage of Protected Health Information (PHI) data in the native FHIR format.

- Deploy Azure API for FHIR with First Party Auth by [deploying via the Azure Portal](https://docs.microsoft.com/en-us/azure/healthcare-apis/fhir-paas-portal-quickstart). 
- To access the Azure API for FHIR, apply RBAC by adding users to `FHIR Data Reader` or `FHIR Data Contributor` role using Access Control (IAM). The users with access will authenticate through the Connector which assumes their role.
- Load sample data into Azure API for FHIR. 
    - Sample project to load data into FHIR can be found [here](https://github.com/microsoft/FHIRPower/tree/main/FHIR-Seed-Data)
    - Detailed instructions to load synthetic data can be found in the [OpenHack-FHIR GitHub](https://github.com/microsoft/OpenHack-FHIR/tree/main/Challenge01-AzureAPIforFHIR#task-2-generate--load-synthetic-data)
- See the following link for more details on [Using Azure API for FHIR](https://github.com/microsoft/OpenHack-FHIR)

### Certified Connector
- Get access to [Power Platform](https://docs.microsoft.com/en-us/power-platform/) environment to create Power Apps.
- Custom Connectors [FHIRBase](https://docs.microsoft.com/en-us/connectors/fhirbase/) and [FHIRClinical](
https://docs.microsoft.com/en-us/connectors/fhirclinical/) need to be added to your PowerPlatform Environment by an Environment Administrator.
- Details on [FHIRBase Actions](https://docs.microsoft.com/en-us/connectors/fhirbase/#actions) and [FHIRClinical Actions](https://docs.microsoft.com/en-us/connectors/fhirclinical/#actions) for API calls.
- Mapping [FHIR Base and Clinical Resources](https://www.hl7.org/fhir/resourcelist.html) to FHIRBase and FHIRClinical connectors.
- More details on [Power Apps](https://docs.microsoft.com/en-us/powerapps/)

### Custom Connector
- Instructions to create your custom connector, can be found [here][(./CUSTOM_CONNECTOR.md) ](https://github.com/microsoft/FHIRPower/blob/main/CUSTOM_CONNECTOR.md)
- Sample custom connector can be found [here][(./SampleFHIRCustomConnector)](https://github.com/microsoft/FHIRPower/tree/main/SampleFHIRCustomConnector)

# Pro dev

## Solution Architecture Overview
We built a custom web application using Blazor to handle the application flow. The application manages the two use-cases mentioned above, the medical intake as well as the discharge summary analysis.


The application will be deployed as a static website for performance purposes. We want to improve the performance, while also lowering the deployment cost.
Since we are using a static website for production deployment, we’ll need to use the SWA CLI for local development. When you deploy a static website on Azure, Azure provides the backend support required to run the application. This includes wiring up support of Authentication/Authorization as well as access to Azure Functions. In order to support local development, this infrastructure is not available, which is what the SWA CLI provides.

![Architecture](./Fast-Pass-Architecture-resized.png)

## Prerequisites
Most recent version of Azure Functions CLI, Static Web Apps CLI, and at least DOTNET version 7

## Local Development Setup Guide
1. Clone Project from Git
2. Expand FastPass.API and add a document called local.setting.json
3. Initialize the following properties in the new file:
```
    {
    "IsEncrypted": false,
    "Host": {
    "CORS": "*"  },
     "Values": {
    "AzureWebJobsStorage": "UseDevelopmentStorage=true",
    "FUNCTIONS_WORKER_RUNTIME": "dotnet-isolated",
    "IsDevelopment": "true",
    "APIConfig:TextAnalyticsBase": "",
    "APIConfig:TextAnalyticsKey": "",
    "APIConfig:FhirScope": "",
    "APIConfig:FhirServerUri": "",
    "APIConfig:Authority": "",
    "APIConfig:TenantId": "",
    "APIConfig:ClientId": "",
    "APIConfig:ClientSecret": ""  }
    }
```
4. The <ins>Base</ins> and <ins>Key</ins> can be found in the resource group containing your Azure Language service
5. The <ins>Uri</ins> can be found in the resource group containing your Azure API for FHIR service and the <ins>Scope</ins> will be the default so it should look like the following: 
    `"your uri"/.default`
6. The <ins>Tenant Id</ins> can be found in the Tenant Properties of your AAD subscription
7. The <ins>ClienntId</ins> and <ins>ClientSecret</ins> can be found in your Azure Static Web Page resource
8. Right click on the solution file and select "startup project" and select the "multiple files option". You will select "start" for both the API and UI files
9. After building and running you should run the following command in a separate instance:
    `swa start http://localhost:5043 --api-location http://localhost:7214`
10. With the debugger open on your local host go to the local host port indicated by the emulator. For example:
    `Azure Static Web Apps emulator started at http://localhost:4280. Press CTRL+C to exit.`
11. Log in to the version of the app residing at the port indicated by the emulator using one of the given options
12. Begin submitting text. For example: 
    ```
    Patient:
    Patient H Sample

    Provider's Pt ID:
    6910828
    
    Sex:
    Female
    ```

## Provisioning and Deploying to Azure
The fast-pass pro dev version is implemented in a Blazor static web app and corresponding C# managed API. The sections below will help you get your Azure resources provisioned and the code deployed.

### Provisioning Azure Resources
A [bicep file](./deploy/main.bicep) is available for creating the Azure resources. Bicep is a domain specific language built on top of the Azure Resource Manager (ARM) APIs. To read more, see [What is Bicep?](https://learn.microsoft.com/en-us/azure/azure-resource-manager/bicep/overview?tabs=bicep).

### Bicep Template Description
The template will provision three resources:
1. A Cognitive Services - Text Analytics service
    - <b>Note:</b> The acceptance of a 'Responsible AI' terms is required at least once in your subscription before the creation of a cognitive services resources can be automated. At the present time, there is no way to do this in an automated way. You can either pre-create the one that the script will manage (fast-pass-lang), or create and delete another. Upon creation of that resource in the portal, you will be asked to accept the Responsible AI terms. See [here](https://learn.microsoft.com/en-us/azure/cognitive-services/cognitive-services-apis-create-account-cli?tabs=windows#prerequisites) for more information.
2. Application Insights
3. A Static Web App 
    - This will also contain the managed API
    - The Application settings will be updated to reference App Insights, Cognitive services and your other passed in parameters

See the [bicep file](./deploy/main.bicep) for any additional parameters you'd like to override. Including linking it to a GitHub repo.

You have two options to provision fast-pass resources; have the Bicep script take care of the App Registration or do the App Registration outside of Bicep and pass in the results. [Bicep doesn't yet handle App Registrations natively](https://learn.microsoft.com/en-us/answers/questions/1058054/support-for-creating-aad-app-registration-using-bi.html), but it can be done by invoking a PowerShell script from Bicep. This it is more involved, so it may be more straight forward to handle that outside the script. Both approaches are documented below.

### App Registration Outside of Bicep Template
This is the default approach (manageAppRegistration=false). Be sure to login, then run the script:
Azure CLI...
```
az login
az deployment group create \
    --resource-group [[resource-group-name]] \
    --template-file main.bicep \
    --parameters \
        fhirServer='[[FHIRServerURL]]' \
        appRegistrationAuthority='[[AuthorityOfAppRegistration]]' \
        appRegistrationTenantId='[[TenantIdOfAppRegistration]]' \
        appRegistrationClientId='[[ClientIdOfAppRegistration]]' \
        appRegistrationClientSecret='[[ClientSecretOfAppRegistration]]'
```

PowerShell (Tested in v7.3)...
```
Connect-AzAccount
New-AzResourceGroupDeployment 
    -ResourceGroupName [[resource-group-name]] 
    -TemplateFile main.bicep 
    -fhirServer [[FHIRServerURL]] 
    -appRegistrationAuthority [[AuthorityOfAppRegistration]] 
    -appRegistrationTenantId [[TenantIdOfAppRegistration]] 
    -appRegistrationClientId [[ClientIdOfAppRegistration]] 
    -appRegistrationClientSecret [[ClientSecretOfAppRegistration]]
```

### App Registration via the Bicep Template
This approach (manageAppRegistration=true). Requires a managed identity to be able to create the App Registration. Follow the process [here](https://reginbald.medium.com/creating-app-registration-with-arm-bicep-b1d48a287abb) - don't skip the step where you are assigning the Application Administrator role to the identity.

Azure CLI...
```
az login
az deployment group create \
    --resource-group [[resource-group-name]] \
    --template-file main.bicep \
    --parameters \
        fhirServer='[[FHIRServerURL]]' \
        manageAppRegistration=true \
        appRegistrationUser='[[NameOfManagedIdentityWithAppAdminRights]]' 
```

PowerShell (Tested in v7.3)...
```
Connect-AzAccount
New-AzResourceGroupDeployment 
    -ResourceGroupName [[resource-group-name]] 
    -TemplateFile main.bicep 
    -fhirServer [[FHIRServerURL]] 
    -manageAppRegistration 
    -appRegistrationUser [[NameOfManagedIdentityWithAppAdminRights]] 
```

### Deploying Code From GitHub
There's already GitHub action in the repo: [here](./.github/workflows/azure-static-web-apps-brave-mushroom-056103510.yml). The only adjustment needed is to add the Deployment Token from the Static Web App and save it as a GitHub secret.
- In the Azure Portal, go to the Static Web App resource that was created
    - Click <ins>Manage deployment token</ins> > Copy to clipboard
- Go to your GitHub repository where you've forked / cloned this repo
    - Settings > Secrets > Actions > New repository secret
    - Name the secret - make sure it is the same as what's referenced two places in the [GitHub action](./.github/workflows/azure-static-web-apps-brave-mushroom-056103510.yml)
    - Paste the value that you copied above 
    - Click Add secret
- Test the deployment by merging a change to the main branch.

## Contributing
This project welcomes contributions and suggestions.  Most contributions require you to agree to a
Contributor License Agreement (CLA) declaring that you have the right to, and actually do, grant us
the rights to use your contribution. For details, visit https://cla.opensource.microsoft.com.

When you submit a pull request, a CLA bot will automatically determine whether you need to provide
a CLA and decorate the PR appropriately (e.g., status check, comment). Simply follow the instructions
provided by the bot. You will only need to do this once across all repos using our CLA.

This project has adopted the [Microsoft Open Source Code of Conduct](https://opensource.microsoft.com/codeofconduct/).
For more information see the [Code of Conduct FAQ](https://opensource.microsoft.com/codeofconduct/faq/) or
contact [opencode@microsoft.com](mailto:opencode@microsoft.com) with any additional questions or comments.

## Trademarks
This project may contain trademarks or logos for projects, products, or services. Authorized use of Microsoft 
trademarks or logos is subject to and must follow 
[Microsoft's Trademark & Brand Guidelines](https://www.microsoft.com/en-us/legal/intellectualproperty/trademarks/usage/general).
Use of Microsoft trademarks or logos in modified versions of this project must not cause confusion or imply Microsoft sponsorship.
Any use of third-party trademarks or logos are subject to those third-party's policies.
