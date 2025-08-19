# 3rdPartyDataAcquisition - Detailed Technical Inventory

## Overview
This document provides a comprehensive technical inventory of the 3rdPartyDataAcquisition repository to facilitate detailed comparison with SktInsights.

## Solution Structure

### Main Solutions
1. **Common.sln** - Shared libraries and utilities
2. **ThirdPartyData.sln** - Main application with Acme provider

### Project Organization
```
3rdPartyDataAcquisition/
├── code/
│   ├── common/                              # 5 projects, 314 tests passing
│   │   ├── DP.Base/                        # Core utilities
│   │   ├── DP.Base.Contracts/              # Common interfaces
│   │   ├── DP.Base.Test/                   # Unit tests (314 tests)
│   │   ├── EEDurableFunctionsCommon/       # Durable Functions framework
│   │   └── Ingress.Lib.Base/               # Ingress processing library
│   └── thirdpartydata/                     # 5 projects
│       └── Acme/                           # Single provider implementation
│           ├── Acme.Contracts/             # Provider contracts
│           ├── Acme.AcquireGeneration/     # Data acquisition
│           ├── Acme.AcquireGeneration.Test/
│           ├── Acme.ProcessGeneration/     # Data processing
│           └── Acme.ProcessGeneration.Test/
```

## Technical Stack Analysis

### .NET Framework & Packages
- **Target Framework**: .NET 8.0
- **Platform**: x64 (AnyCPU with x64 preference)

### Core Dependencies
```json
{
  "Azure.Storage.Blobs": "12.8.1",           // ⚠️ Known vulnerability
  "CsvHelper": "27.2.1",
  "Microsoft.Azure.Functions.Worker": "1.23.0",
  "Microsoft.Azure.Functions.Worker.Extensions.DurableTask": "1.2.0",
  "Microsoft.Azure.Functions.Worker.Extensions.Http": "3.2.0",
  "Microsoft.Azure.Functions.Worker.Extensions.Http.AspNetCore": "1.3.2",
  "Microsoft.Azure.Functions.Worker.Sdk": "1.18.1",
  "Microsoft.Azure.Services.AppAuthentication": "1.6.1",
  "Microsoft.Extensions.Configuration.AzureKeyVault": "3.1.13",
  "Microsoft.Extensions.Configuration.Json": "8.0.1",
  "Microsoft.Extensions.Logging": "8.0.1",
  "SendGrid": "9.22.0",
  "Newtonsoft.Json": "implicit"               // Used over System.Text.Json
}
```

### Test Dependencies
```json
{
  "Microsoft.NET.Test.Sdk": "17.8.0",
  "xunit": "2.4.2",
  "xunit.runner.visualstudio": "2.4.5",
  "Moq": "4.20.69"
}
```

## Functional Capabilities

### 1. Data Acquisition (Acme.AcquireGeneration)

#### Key Components
- **AcmeAcquireGenerationOrchestrator**: Main workflow orchestrator
- **AcmeAcquireGenerationSubOrchestrator**: Sub-workflow handler
- **AcmeAcquireGenerationActivityFcns**: Individual activity functions
- **AcmeAcquireGenerationFcnTrigger**: HTTP trigger endpoints
- **AcmeHttpClient**: Custom HTTP client with retry logic

#### Supported Operations
- Weather-based generation data acquisition
- Wind and Solar generation types
- Asynchronous job management
- File naming and organization
- Blob storage integration

#### Configuration
- **AcquireEnelytixGenerationConfigSettings**: Service configuration
- **AcmeSubscriptionInfo**: API credentials and endpoints
- **BlobConfigInfo**: Storage configuration

### 2. Data Processing (Acme.ProcessGeneration)

#### Key Components
- **AcmeProcessGenerationOrchestrator**: Processing workflow
- **AcmeProcessGenerationActivityFcns**: Processing activities
- **AcmeGenMasterTemplate**: Data template handling
- **AcmeGenOriginal/Processed**: Data models with CSV mapping

#### Data Models
```csharp
// CSV Processing Models
AcmeGenMasterTemplate       // Template for generation data
AcmeGenMasterTemplateMap    // CSV mapping configuration
AcmeGenOriginal            // Raw acquired data structure
AcmeGenProcessed           // Processed data structure  
AcmeGenProcessedMap        // Processed data CSV mapping
```

### 3. Shared Contracts (Acme.Contracts)

#### Core Interfaces
- **IEnelytixAcquireGenerationOrchestrator**: Acquisition orchestration
- **IEnelytixProcessGenerationOrchestrator**: Processing orchestration

#### Data Models
- **AcquireGenerationInputModel**: Input validation and processing
- **AcmeAcquireGenerationContext**: Acquisition workflow context
- **AcmeProcessGenerationContext**: Processing workflow context
- **AcmeResponseObject**: API response handling
- **AcmeApiInfo**: API configuration
- **AcmeJobInfo**: Job status tracking
- **TurbineType**: Enumeration for turbine types

### 4. Common Libraries

#### DP.Base (314 Tests Passing)
- String extensions and utilities
- Date/time utilities with DST handling
- Configuration management
- Extension methods

#### DP.Base.Contracts
- Common interface definitions
- Contract patterns
- Base types

#### Ingress.Lib.Base
- Application Insights telemetry processing
- SharePoint authentication management
- SendGrid email integration
- Base classes for HTTP clients

#### EEDurableFunctionsCommon
- Durable Functions orchestration helpers
- Workflow management utilities
- Management payload workarounds

## Architecture Patterns

### 1. Orchestration Pattern (Durable Functions)
```csharp
// Workflow Definition
[Function("AcmeAcquireGenerationOrchestrator")]
public async Task<OrchestrationResults> RunOrchestrator(
    [OrchestrationTrigger] TaskOrchestrationContext context)
{
    // Fan-out/Fan-in pattern implementation
    // Parallel processing with orchestration
    // State management and checkpointing
}
```

### 2. Repository Pattern
```csharp
// Abstracted data access
public interface IBlobContainerWrapper
{
    Task<CallResult<string>> UploadBlobAsync(string blobName, Stream content);
    Task<CallResult<Stream>> DownloadBlobAsync(string blobName);
}
```

### 3. Circuit Breaker / Retry Pattern
```csharp
// HTTP Client with retry logic
public async Task<CallResult<string>> WaitForGenerationJobAsync(
    string historyId, 
    int retryCount = 90, 
    int retryIntervalSeconds = 10)
{
    // Configurable retry with exponential backoff
    // Timeout handling
    // Circuit breaker logic
}
```

### 4. Configuration Pattern
```csharp
// Dependency injection configuration
services.AddOptions<AcmeSubscriptionInfo>()
    .Configure<IConfiguration>((settings, configuration) =>
    {
        configuration.GetSection(nameof(AcmeSubscriptionInfo)).Bind(settings);
    });
```

## Security Implementation

### Authentication & Authorization
- **Azure Key Vault**: Secret management
- **DefaultAzureCredential**: Managed identity authentication
- **User Secrets**: Local development secrets

### Configuration Security
```csharp
// Key Vault integration
if (!string.IsNullOrEmpty(keyVaultEndpoint))
{
    config.AddAzureKeyVault(new Uri(keyVaultEndpoint), new DefaultAzureCredential());
}
```

### Communication Security
- HTTPS only endpoints
- Encrypted Azure Storage connections
- Secure API key management

## Monitoring & Observability

### Application Insights Integration
```csharp
// Custom telemetry processor
public abstract class AppInsightsPrefixTelemetryProcessorBase : ITelemetryProcessor
{
    protected virtual string Prefix => "AppInsightsPrefixGoesHere";
    // Prefixes all telemetry for easier identification
}
```

### Logging Configuration
```json
{
  "version": "2.0",
  "logging": {
    "applicationInsights": {
      "samplingExcludedTypes": "Request",
      "samplingSettings": {
        "isEnabled": true,
        "excludedTypes": "Request"
      }
    },
    "logLevel": {
      "default": "Information"
    }
  }
}
```

### Structured Logging
- Consistent log formatting
- Correlation IDs for request tracing
- Performance metrics collection
- Error tracking and alerting

## Testing Strategy

### Unit Test Coverage
- **Common Libraries**: 314 tests passing
- **Test Helpers**: Standardized mock creation
- **Moq Framework**: Dependency mocking
- **xUnit Framework**: Test execution

### Test Patterns
```csharp
// Standardized test helper methods
public static Mock<IAcmeHttpClient> GetHttpClientMock(
    bool success, 
    string successMsg = null, 
    string failureMsg = null, 
    string returnVal = null)
```

### Test Categories
- Unit tests for business logic
- Integration test helpers
- Mock data generation
- Configuration testing

## Data Flow Architecture

### Acquisition Workflow
1. **HTTP Trigger** → Input validation
2. **Orchestrator** → Context creation
3. **Activity Functions** → API calls
4. **Sub-orchestrator** → Job monitoring
5. **Activity Functions** → Data retrieval
6. **Storage** → Blob persistence

### Processing Workflow
1. **Blob Trigger** → New data detection
2. **Orchestrator** → Processing context
3. **Activity Functions** → Data transformation
4. **CSV Processing** → Data mapping
5. **Storage** → Processed data output

## Configuration Management

### Environment-specific Configuration
- **local.settings.json**: Local development
- **Azure Key Vault**: Production secrets
- **Environment Variables**: Runtime configuration
- **User Secrets**: Developer-specific settings

### Configuration Classes
```csharp
public class AcmeSubscriptionInfo
{
    public AcmeApiInfo ApiInfo { get; set; }
}

public class BlobConfigInfo
{
    public string ConnectionString { get; set; }
    public string ContainerName { get; set; }
}
```

## Known Issues & Technical Debt

### Critical Issues
1. **Security Vulnerability**: Azure.Storage.Blobs v12.8.1 has known moderate severity vulnerability
2. **Build Failures**: Missing assembly references in project files
3. **Package Conflicts**: Microsoft.Extensions.DependencyInjection.Abstractions version conflicts

### Code Quality Issues
1. **Magic Strings**: Hard-coded values in file naming logic
2. **TODO Comments**: Incomplete features marked with "skt_todo"
3. **Error Handling**: Inconsistent error handling patterns
4. **Documentation**: Missing API documentation

### Technical Debt
1. **Project References**: Complex DLL reference paths instead of project references
2. **Workarounds**: Management payload workarounds in orchestration
3. **Configuration**: Mixed configuration patterns
4. **Testing**: Limited integration test coverage

## API Endpoints & Integration

### HTTP Triggers
- Data acquisition workflow initiation
- Manual orchestration triggers
- Status monitoring endpoints

### External Dependencies
- **Acme API**: Third-party data provider
- **Azure Storage**: Blob storage for data persistence
- **SendGrid**: Email notification service
- **Application Insights**: Telemetry and monitoring

### Data Formats
- **CSV**: Primary data format for generation data
- **JSON**: Configuration and API communication
- **Binary**: Blob storage for large datasets

## Deployment & Infrastructure

### Azure Functions Configuration
- Consumption plan compatibility
- Durable Functions runtime
- Application Insights integration
- Key Vault integration

### Storage Requirements
- Azure Blob Storage containers
- Hierarchical namespace support
- Access tier optimization
- Lifecycle management policies

## Extension Points & Modularity

### Provider Pattern
- Single Acme provider implementation
- Extensible for additional providers
- Common contracts for provider abstraction
- Shared orchestration patterns

### Activity Function Pattern
- Modular activity implementations
- Reusable orchestration components
- Configurable retry policies
- Error handling standardization

---

*This technical inventory provides the foundation for detailed feature-by-feature comparison with SktInsights once that repository becomes available.*