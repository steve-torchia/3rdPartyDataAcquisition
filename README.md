# Third Party Weather Data Pipeline

A robust, cloud-native data pipeline for acquiring and processing weather-based generation data from a third-party API. Built with .NET 8, Azure Functions, and Azure Durable Functions for scalable, reliable data processing.

## 🏗️ Architecture Overview

This solution implements a **microservices architecture** with clean separation of concerns:

- **Data Acquisition**: Fetches weather generation data from external APIs
- **Data Processing**: Transforms and validates raw data 
- **Data Storage**: Persists processed data to Azure Blob Storage
- **Orchestration**: Manages complex workflows with Azure Durable Functions

## 🚀 Technology Stack

- **C# / .NET 8** - Modern, cross-platform framework
- **Azure Functions v4** - Serverless compute platform
- **Azure Durable Functions** - Stateful functions for complex workflows
- **Azure Blob Storage** - Scalable object storage
- **Azure Key Vault** - Secure configuration management
- **Azure Application Insights** - Monitoring and telemetry
- **xUnit & Moq** - Comprehensive testing framework
- **Newtonsoft.Json** - JSON serialization
- **CsvHelper** - CSV data processing

## 🔄 Data Flow

1. **HTTP Trigger** → Receives request to acquire weather data
2. **Orchestrator** → Launches durable function workflow
3. **Activity Functions** → Execute individual tasks:
   - Launch generation job at third-party API
   - Poll for job completion
   - Download and validate results
   - Save data to Azure Blob Storage
4. **Sub-Orchestrator** → Manages concurrent processing with configurable limits
5. **Result** → Returns management URLs for status monitoring

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
## 🛠️ Key Features

### Reliability & Resilience
- **Retry Logic**: Configurable retry policies for external API calls
- **Error Handling**: Comprehensive error handling with `CallResult<T>` pattern
- **Timeout Management**: Configurable timeouts for long-running operations
- **Status Monitoring**: Real-time workflow status via management APIs

### Scalability & Performance
- **Concurrent Processing**: Configurable concurrency limits (1-10 jobs)
- **Async/Await**: Non-blocking operations throughout
- **Durable Functions**: Checkpointing for long-running workflows
- **Memory Optimization**: Streaming data processing

### Data Quality & Validation
- **Structured Logging**: Comprehensive telemetry and debugging info
- **Time Zone Handling**: UTC conversions to/from localtime
- **CSV Validation**: Data integrity checks before storage


## 📋 Prerequisites

- **.NET 8 SDK**
- **Azure Functions Core Tools v4**
- **Azure Storage Account**
- **Azure Key Vault** (for production)
- **Visual Studio 2022** or **VS Code**

## ⚙️ Configuration

### Local Development

1. **User Secrets**: Configure sensitive settings
   ```bash
   dotnet user-secrets set "AcmeSubscriptionInfo:ApiInfo:BaseUrl" "https://api.acme.com"
   dotnet user-secrets set "AcmeSubscriptionInfo:ApiKey" "your-api-key"
   ```

2. **Local Settings**: Create `local.settings.json`
   ```json
   {
     "IsEncrypted": false,
     "Values": {
       "AzureWebJobsStorage": "your-storage-connection-string",
       "FUNCTIONS_WORKER_RUNTIME": "dotnet-isolated"
     }
   }
   ```

### Production Deployment

- **Azure Key Vault**: Store secrets securely
- **Application Settings**: Configure via Azure Portal or ARM templates
- **Managed Identity**: Use for secure Azure resource access


### Test Coverage
- **Activity Functions**: Complete test coverage with mocked dependencies
- **Orchestrators**: Workflow testing with durable functions test framework
- **HTTP Clients**: Mock external API responses
- **Helpers**: Utility function validation


## 🔧 Key Design Patterns

### Orchestration Pattern
- **Durable Functions** for stateful workflows
- **Fan-out/Fan-in** for parallel processing
- **Human Interaction** possible to call these functions manually (e.g. Postman)

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
- **Custom Metrics**: Business-specific KPIs

## 🔐 Security

- **Azure Key Vault**: Secure secret management
- **Managed Identity**: Password-less authentication
- **HTTPS Only**: Encrypted communication
- **Input Validation**: Sanitized user inputs

## 📈 Performance Considerations

- **Concurrency Limits**: Configurable to prevent API throttling
- **Memory Management**: Streaming for large datasets
- **Connection Pooling**: Efficient HTTP client usage
---
