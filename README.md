\# NexusTalent Plugin Pipeline



\## Overview

This project demonstrates hands-on development of Microsoft Dataverse plug-ins using C# and the Dataverse SDK. It covers plug-in pipeline stages, data validation, automatic numbering, related-record updates, and tracing.



\## Technologies Used

\- C#

\- Microsoft Dataverse

\- Microsoft Dynamics 365

\- Visual Studio 2022

\- Microsoft.Xrm.Sdk



\## Plug-ins Implemented



\### 1. Candidate Field Validation

\- Validates required Candidate fields, such as Name and Email.

\- Prevents invalid data from being saved.

\- Uses the PreValidation stage.



\### 2. Candidate Auto-Numbering

\- Generates a unique Candidate Code in the format `CAN-000001`.

\- Finds the highest existing valid Candidate Code and generates the next number.

\- Sets the Candidate Code during the PreOperation stage of Create.



\### 3. Interview Rollup

\- Updates the related Requisition's Interview Count when Interview records are created or deleted.

\- Demonstrates related-record updates using the Dataverse SDK.



\## Concepts Practiced

\- `IPlugin` interface and `IPluginExecutionContext`

\- `InputParameters` and Target entity

\- PreValidation, PreOperation, and PostOperation stages

\- Synchronous and asynchronous plug-ins

\- Pre-Images and Post-Images

\- `IOrganizationService` and `ITracingService`

\- Plug-in registration and debugging

\- Exception handling using `InvalidPluginExecutionException`



\## Setup

1\. Clone or download this repository.

2\. Open the solution or C# project in Visual Studio 2022.

3\. Restore the required NuGet packages.

4\. Build the project.

5\. Register the plug-in assembly and configure the required steps in your Dataverse development environment.

6\. Test each plug-in using the relevant Dataverse table operations.



\## Important Notes

\- Update table names and column logical names to match your Dataverse environment.

\- Register each plug-in step with the correct message, stage, execution mode, and filtering attributes.

\- Do not commit passwords, client secrets, connection strings containing credentials, or private signing keys.



\## Purpose

This repository is part of my hands-on learning journey in Microsoft Dataverse plug-in development, with a focus on practical business scenarios and plug-in pipeline behavior.



