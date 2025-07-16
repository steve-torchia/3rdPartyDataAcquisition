# Third Party Data Pipeline

A robust, cloud-native data pipeline for acquiring and processing data from a third-party API. Built with .NET 8, Azure Functions, and Azure Durable Functions for scalable, reliable data processing.

## 🏗️ High Level Architecture Overview

This solution implements a **microservices architecture** with clean separation of concerns:

- **Data Acquisition**: Fetches weather generation data from external APIs
- **Data Processing**: Transforms and validates raw data 
- **Data Storage**: Persists processed data to Azure Blob Storage
- **Orchestration**: Manages stateful workflows with Azure Durable Functions



## 🔄 <u>Data Acquisition</u> WorkFlow

📄 **[View Detailed Workflow Diagram (PDF)](./3rdParty%20Data%20Acquisition.pdf)**

1. **HTTP Trigger** → Receives request to start the  acquire data flow. Starts up the Orchestrator
2. **Orchestrator** → Launches durable function workflow and returns management URLs for status monitoring.  Manages configurable concurrency using Sub Orchestrators.
3. **Sub-Orchestrator** → Manages stateful workflow of Activity Functions
4. **Activity Functions** → Execute individual tasks:
   A. Launch generation job at third-party API
   B. Wait (Poll) for job completion
   C. Download and save data to Azure Blob Storage

## 🔍 Code Review Guide: Execution Flow

To assist in understanding this solution, follow the execution flow in this order:

1. **`AcmeAcquireGenerationFcnTrigger.cs`** → HTTP trigger and entry point
2. **`AcmeAcquireGenerationOrchestrator.cs`** → Top-level orchestrator, manages concurrency and status reporting  
3. **`AcmeAcquireGenerationSubOrchestrator.cs`** → Coordinates the sequence of API interaction steps
4. **`AcmeAcquireGenerationActivityFcns.cs`** → Contains individual activity functions for interacting with the remote API

## 🚀 Technology Stack

- **C# / .NET 8** - Modern, cross-platform framework
- **Azure Functions v4** - Serverless compute platform
- **Azure Durable Functions** - Stateful functions for complex workflows (code rather than declarative)
- **Azure Blob Storage** - Scalable object storage
- **Azure Key Vault** - Secure secrets and configuration management
- **Azure Application Insights** - Monitoring and telemetry
- **xUnit & Moq** - Comprehensive testing framework
- **Newtonsoft.Json** - JSON serialization
- **CsvHelper** - CSV data processing

---

## 🛠️ Key Features

### Reliability & Resilience

- **Retry Logic**: Configurable retry policies for external API calls
- **Error Handling**: Comprehensive error handling with `CallResult<T>` pattern
- **Timeout Management**: Configurable timeouts for long-running operations
- **Status Monitoring**: Real-time workflow status via management APIs

### Scalability & Performance

- **Concurrent Processing**: Fan Out/Fan In pattern with configurable concurrency
- **Async/Await**: Non-blocking operations throughout
- **Durable Functions**: Checkpointing for long-running workflows
- **Memory Optimization**: Streaming data processing

### Data Quality & Validation

- **Structured Logging**: Comprehensive telemetry and debugging info for operational insights
- **Time Zone Handling**: UTC conversions to/from localtime
- **CSV Validation**: Data integrity checks before storage

## 🔧 Key Design Patterns

### Orchestration Pattern

- **Durable Functions** for stateful workflows
- **Fan-out/Fan-in** for parallel processing
- **Human Interaction** possible to call and monitor these functions manually (e.g. Postman)

### Repository Pattern

- **Abstracted data access** via interfaces
- **Dependency injection** for testability
- **Blob storage wrapper** for consistent API

### Circuit Breaker Pattern

- **Retry policies** with exponential backoff
- **Timeout handling** for external dependencies
- **Graceful degradation** on failures

## 📊 Monitoring & Observability

- **Application Insights**: Performance metrics and telemetry
- **Structured Logging**: Consistent log formatting
- **Health Checks**: Endpoint monitoring

## 🔐 Security

- **Azure Key Vault**: Secure secret management
- **Managed Identity**: Password-less authentication
- **HTTPS Only**: Encrypted communication
- **Input Validation**: Sanitized user inputs

## 📈 Performance Considerations

- **Concurrency Limits**: Configurable to prevent API throttling
- **Memory Management**: Streaming for large datasets
- **Connection Pooling**: Efficient HTTP client usage

## 📁 Project Structure

```
ThirdPartyData.sln
├── Acme/
│   ├── Acme.AcquireGeneration/          # Data acquisition service
│   │   ├── HttpClient/                  # HTTP client implementations
│   │   ├── AcmeAcquireGenerationOrchestrator.cs
│   │   ├── AcmeAcquireGenerationActivityFcns.cs
│   │   └── Program.cs
│   ├── Acme.AcquireGeneration.Test/     # Unit tests for acquisition
│   ├── Acme.ProcessGeneration/          # Data processing service
│   ├── Acme.ProcessGeneration.Test/     # Unit tests for processing
│   └── Acme.Contracts/                  # Shared contracts and models
│       ├── AcmeApiInfo.cs
│       ├── AcmeHelpers.cs
│       └── TurbineType.cs
```
