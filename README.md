# Azure Service Bus Error Handling demo

- [Azure Service Bus Error Handling demo](#azure-service-bus-error-handling-demo)
  - [Purpose: to be a hands-on demo for explicit dead-lettering.](#purpose-to-be-a-hands-on-demo-for-explicit-dead-lettering)
  - [Credits](#credits)
  - [Changelog](#changelog)
  - [Branches](#branches)
    - [servicebus](#servicebus)
    - [master](#master)
  - [Usage](#usage)
    - [Prerequisites](#prerequisites)
    - [MicrosoftAzureServiceBusDemo.slnf](#microsoftazureservicebusdemoslnf)
    - [MassTransitDemo.slnf](#masstransitdemoslnf)
    - [AzureFunctionDemo.slnf](#azurefunctiondemoslnf)
      - [Healthy message](#healthy-message)
      - [Poison message](#poison-message)
      - [Message with timeout](#message-with-timeout)
      - [Message with processing error](#message-with-processing-error)


## Purpose: to be a hands-on demo for explicit dead-lettering.

The current repo is derived from the demo provided by Alan Smith in the Pluralsight course ["Azure Service Bus In-depth"](https://app.pluralsight.com/library/courses/azure-service-bus-in-depth/table-of-contents). 

## Credits

Alan Smith - Original Source code: https://github.com/asith-w/azure-service-bus-in-depth/tree/master/09/demos/ErrorHandling  

## Changelog

Diff changelog against original source code:

- the code was migrated to .NET5 with minor changes
- renaming and reorganizing the projects
- some updates for deferred messages
- some updates on the sender
- minor refactoring to better capture the scope of the demo
- added new demo with Console app with Mass Transit on top of Azure Service Bus Topics
- branch master: added new demo with AF on Azure Queue Storage
- branch servicebus: added new demo with AF on Azure Service Bus Topics

## Branches

### servicebus

- demo with Console app with on Microsoft.Azure.ServiceBus on Azure Service Bus Queue
- demo with Console app with Mass Transit on top of Azure Service Bus Topics
- demo with AF on Azure Service Bus Topics

### master
 
- demo with Console app with on Microsoft.Azure.ServiceBus on Azure Service Bus Queue
- demo with Console app with Mass Transit on top of Azure Service Bus Topics
- demo with AF on Azure Queue Storage

## Usage

### Prerequisites

An Azure Service Bus endpoint with a key with management access (MicrosoftAzureServiceBusDemo.slnf and MassTransitDemo.slnf are creating the queue and topics unattended)

Note: The AzureFunctionDemo.slnf does not create the topic unattended and needs the following prerequisites:

- An existing topic named **conferences**
- An existing subscription named **mysubscription** with a Lock duration of 10 seconds

### MicrosoftAzureServiceBusDemo.slnf 

Demo with Console app with on Microsoft.Azure.ServiceBus on **Azure Service Bus Queue**

Change the ConnectionString in [Settings.cs](./CommonHelpers/Settings.cs) with your values.
Build.
Run/Debug.

### MassTransitDemo.slnf 

Demo with Console app with Mass Transit on top of **Azure Service Bus Topics**

Change the ConnectionString in [Settings.cs](./CommonHelpers/Settings.cs) with your values.
Build.
Run/Debug.

### AzureFunctionDemo.slnf 

Demo with Azure Functions on **Azure Service Bus Topics**

Change the key AzureWebJobsServiceBus in [local.settings.json](./MessageBusDemo.AzureFunctionSender/local.settings.json) with your values.
Build.
Run/Debug.

Use Powershell (or Postman) to call the http endpoint exposed by AzureFunctionSender

#### Healthy message

Simulates a healthy message

```powershell
iwr -Method POST -Uri http://localhost:7071/api/ConferenceAnnouncement -Headers @{"Content-Type"="application/json"} -Body '{ "ConferenceId":"100", "RoomName":"Atica"}'
```

#### Poison message

Simulates a poison message with a handled, specific, exception

```powershell
iwr -Method POST -Uri http://localhost:7071/api/ConferenceAnnouncement -Headers @{"Content-Type"="application/json"} -Body '{ "ConferenceId":"101", "RoomName":"Chemical"}'
```

#### Message with timeout

Simulates a resource with delayed answer

```powershell
iwr -Method POST -Uri http://localhost:7071/api/ConferenceAnnouncement -Headers @{"Content-Type"="application/json"} -Body '{ "ConferenceId":"102", "RoomName":"Lazy"}'
```

#### Message with processing error

Simulates a message that raise an exception handled under the generic catch

```powershell
iwr -Method POST -Uri http://localhost:7071/api/ConferenceAnnouncement -Headers @{"Content-Type"="application/json"} -Body '{ "ConferenceId":"103", "RoomName":"Phantom"}'
```
