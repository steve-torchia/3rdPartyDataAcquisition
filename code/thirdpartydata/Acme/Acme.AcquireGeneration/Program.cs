using Azure.Core.Serialization;
using Azure.Identity;
using Acme.Contracts;
using Acme.AcquireGeneration;
using Ingress.Lib.Base;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System.Reflection;
using DurableFunctionsCommon;
using Microsoft.Extensions.Options;
using System.Net;

var host = new HostBuilder()
    .ConfigureFunctionsWebApplication((IFunctionsWorkerApplicationBuilder builder) =>
    {
        // SKT: Override the default use of System.Text.Json with Newtonsoft.Json.  System.Text.Json cannot handle the 
        // HttpContent type  which we use in EnverusAcquireHistoricPricesResult


        // System.Text.Json might be better in terms of performance, but it is not as flexible as Newtonsoft.Json
        // https://trevormccubbin.medium.com/net-performance-analysis-newtonsoft-json-vs-system-text-json-in-net-8-34520c21d054
        // https://medium.com/@smr.talebi/newtonsoft-json-vs-system-text-json-in-net-8-0-which-should-you-choose-c6d8b7056bd2

        builder.Services.Configure<WorkerOptions>(workerOptions =>
        {
            var settings = NewtonsoftJsonObjectSerializer.CreateJsonSerializerSettings();
            settings.ContractResolver = new CamelCasePropertyNamesContractResolver();
            settings.NullValueHandling = NullValueHandling.Ignore;
            workerOptions.Serializer = new NewtonsoftJsonObjectSerializer(settings);
        });
    })

    // Config
    .ConfigureAppConfiguration(config =>
    {
        var builtConfig = config.Build();
        var keyVaultEndpoint = builtConfig["AzureKeyVaultEndpoint"];

        config.SetBasePath(Environment.CurrentDirectory);

        // skt_todo: can refactor this to only deal with local or published - where the only difference is that on local we use the json file
        // key vault will be dealt with exclusively when published to Azure, where we will use the following syntax at the function config in azure:
        // @Microsoft.KeyVault(SecretUri={theSecretUri})
        if (!string.IsNullOrEmpty(keyVaultEndpoint))
        {
            config.AddAzureKeyVault(new Uri(keyVaultEndpoint), new DefaultAzureCredential());
        }
        else
        {
            config.AddJsonFile("local.settings.json", true);
            config.AddUserSecrets(Assembly.GetExecutingAssembly(), optional: true); //* This lets you use a localhost secret store, https://docs.microsoft.com/en-us/aspnet/core/security/app-secrets?view=aspnetcore-5.0&tabs=windows
        }
        config.AddEnvironmentVariables();
    })

    // Services
    .ConfigureServices(services =>
    {
        services.AddApplicationInsightsTelemetryWorkerService();
        services.ConfigureFunctionsApplicationInsights();

        // SKT-MgmtPayloadWorkaround : This is required to work around an issue with CreateHttpManagementPayload not being available yet.
        // using this global config setting to temporarily get around it.  Once that is addressed, we can remove this code.
        // **  Search for "SKT-MgmtPayloadWorkaround " in the EEDurableFunctionsCommon.OrchestrationHelpers Class for all the details
        services.AddOptions<GlobalConfigSettings>()
            .Configure<IConfiguration>((settings, configuration) =>
            {
                configuration.GetSection(nameof(GlobalConfigSettings)).Bind(settings);
            });


        // Configuration
        services.AddOptions<BlobConfigInfo>()
            .Configure<IConfiguration>((settings, configuration) =>
            {
                configuration.GetSection(nameof(BlobConfigInfo)).Bind(settings);
            });

        services.AddOptions<AcquireAcmeGenerationConfigSettings>()
            .Configure<IConfiguration>((settings, configuration) =>
            {
                configuration.GetSection(nameof(AcquireAcmeGenerationConfigSettings)).Bind(settings);
            });

        services.AddOptions<AcmeSubscriptionInfo>()
            .Configure<IConfiguration>((settings, configuration) =>
            {
                configuration.GetSection(nameof(AcmeSubscriptionInfo)).Bind(settings);
            });

        // HTTP Client:
        var sp = services.BuildServiceProvider();
        var cfg = sp.GetService<IOptions<AcquireAcmeGenerationConfigSettings>>().Value;
        if (cfg.UseHttpInternetMock)
        {
            services.AddHttpClient<IAcmeHttpClient, AcmeHttpClientInternetMock>();
        }
        else
        {
            services.AddHttpClient<IAcmeHttpClient, AcmeHttpClient>()
                    // Acme uses NTLM security 
                    .ConfigurePrimaryHttpMessageHandler((s) =>
                    {
                        var options = sp.GetService<IOptions<AcmeSubscriptionInfo>>().Value;
                        return new HttpClientHandler
                        {
                            Credentials = new NetworkCredential(options.Username, options.Password, options.Domain)
                        };
                    });
        }

        // The workflow (aka Durable Orchestration class)
        services.AddScoped<IAcmeAcquireGenerationOrchestrator, AcmeAcquireGenerationOrchestrator>();

        // The activity functions 
        services.AddScoped<AcmeAcquireGenerationActivityFcns>();

    })
    .ConfigureLogging(logging =>
    {
        logging.Services.Configure<LoggerFilterOptions>(options =>
        {
            // By default, the Application Insights SDK adds a logging filter that instructs the logger to capture only warnings and more 
            // severe logs. If you want to disable this behavior, remove the filter rule as part of service configuration:
            LoggerFilterRule defaultRule = options.Rules.FirstOrDefault(rule => rule.ProviderName
                == "Microsoft.Extensions.Logging.ApplicationInsights.ApplicationInsightsLoggerProvider");
            if (defaultRule is not null)
            {
                options.Rules.Remove(defaultRule);
            }
        });
    })
    .Build();


host.Run();
