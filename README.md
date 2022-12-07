# Project

> This repo has been populated by an initial template to help get you started. Please
> make sure to update the content to build a great experience for community-building.

## Scenario

**Digitizing Paper forms** Documents are uploaded using PowerApps which triggers a PowerAutomate flow. The flow scans the document using OCR and Text Anlaytics for Health extracts the relevant keyword.

## Overview


### Azure API for FHIR
The healthcare industry is rapidly transforming health data to the emerging standard of FHIR® (Fast Healthcare Interoperability Resources). FHIR enables a robust, extensible data model with standardized semantics and data exchange that enables all systems using FHIR to work together. FHIR also enables the rapid exchange of data in applications. Backed by a managed PaaS [Azure API for FHIR](https://docs.microsoft.com/en-us/azure/healthcare-apis/overview) offering, FHIR also provides a scalable and secure environment for the management and storage of Protected Health Information (PHI) data in the native FHIR format.

### Power Platform Connectors for Azure API for FHIR
A connector is a proxy or a wrapper around an API that allows the underlying service to talk to Microsoft Power Platform. Connectors provide a way for users to leverage a set of pre-built actions and triggers to build applications and workflows.

The two Connectors that work with Azure API for FHIR contain a subset of FHIR Resources and are bi-directional, supporting both reads and writes to the FHIR Service.

**[FHIRBase](https://docs.microsoft.com/en-us/connectors/fhirbase/)** and **[FHIRClinical](
https://docs.microsoft.com/en-us/connectors/fhirclinical/)** are certified custom connectors that allows for building secure healthcare applications to enable interoperability using FHIR.

# Power Platform

##Solution Architecture Overview

## Healthcare Use Cases
This repository provides starter kit PowerApp and PowerAutomate pacakges with the ability to extend the App.

## Prerequisites

### Azure API for FHIR
- Deploy Azure API for FHIR with First Party Auth by [deploying via the Azure Portal](https://docs.microsoft.com/en-us/azure/healthcare-apis/fhir-paas-portal-quickstart). 
- To access the Azure API for FHIR, apply RBAC by adding users to `FHIR Data Reader` or `FHIR Data Contributor` role using Access Control (IAM). The users with access will authenticate through the Connector which assumes their role.
- Load sample data into Azure API for FHIR. 
    - Sample project to load data into FHIR can be found [here](https://github.com/microsoft/FHIRPower/tree/main/FHIR-Seed-Data)
    - Detailed instructions to load synthetic data can be found in the [OpenHack-FHIR Github](https://github.com/microsoft/OpenHack-FHIR/tree/main/Challenge01-AzureAPIforFHIR#task-2-generate--load-synthetic-data)
- See the following link for more details on [Using Azure API for FHIR](https://github.com/microsoft/OpenHack-FHIR)

#### Certified Connector
- Get access to [Power Platform](https://docs.microsoft.com/en-us/power-platform/) environment to create Power Apps.
- Custom Connectors [FHIRBase](https://docs.microsoft.com/en-us/connectors/fhirbase/) and [FHIRClinical](
https://docs.microsoft.com/en-us/connectors/fhirclinical/) need to be added to your PowerPlatform Environment by an Environment Administrator.
- Details on [FHIRBase Actions](https://docs.microsoft.com/en-us/connectors/fhirbase/#actions) and [FHIRClinical Actions](https://docs.microsoft.com/en-us/connectors/fhirclinical/#actions) for API calls.
- Mapping [FHIR Base and Clinical Resources](https://www.hl7.org/fhir/resourcelist.html) to FHIRBase and FHIRClinical connectors.
- More details on [Power Apps](https://docs.microsoft.com/en-us/powerapps/)

#### Custom Connector
- Instructions to create your custom connector, can be found [here][(./CUSTOM_CONNECTOR.md) ](https://github.com/microsoft/FHIRPower/blob/main/CUSTOM_CONNECTOR.md)
- Sample custom connector can be found [here][(./SampleFHIRCustomConnector)](https://github.com/microsoft/FHIRPower/tree/main/SampleFHIRCustomConnector)

# Pro dev

## Healthcare Use Cases

We built this project over two different phases, each representing two different possible operations of health care data.
### Medical Intake
Patient presents medical insurance card at the doctors office. An image of the insurance card is uploaded to the system and sent to the OCR service to extract patient biographic information as well as insurance coverage information.


The information exracted by the OCR is presented on a form for a intake specialist to verify, correct, and then submit to the EMR system using the FHIR API exposed by the Azure Health Data Service.
### Disharge Summary Analysis
For a given patient, we obtain a discharge summary and extract relevant medical information. Using a custom web application, we submit the contents of the dischard summary to the Text Analytics for Healthcare service.


Upon successful processing, we extract the entities identified as well as the FHIR bundle. The extracted entities are then displayed on a web page on a custom web
application  to conduct quick spot/quality check. Once the data is validated, the extracted FHIR bundle is submitted to the Azure Health Data Services for saving into the EMR using the Azure Health Data Service FHIR API.

## Solution Architecture Overview
We built a custom web application using Blazor to handle the application flow. The application manages the two usecases mentioned above, the medical intake as well as the discharge summary analysis.


The application will be deployed as a static website for performance purposes. We want to improve the performance, while also lowering the deployment cost.
Since we are using a static website for production deployment, we’ll need to using the SWA CLI for local development. When you deploy a static website on Azure, Azure provides the backend support required to run the application. This includes wiring up support of Authentication/Authorization as well as access to Azure Functions. In order to support local development, this infrastructure is not available, which is what the SWA CLI provides.

![Architecture](./Fast-Pass-Architecture-resized.png)
## Prerequisites
Most recent version of Azure Functions CLI, Static Web Apps CLI, and atleast DOTNET version 7 downloaded

## Setup Guide
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
4. The <ins>Base</ins> and <ins>Key</ins> can be found in the resource group contianing your Azure Language service
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
