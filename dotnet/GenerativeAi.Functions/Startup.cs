﻿using Azure;
using Azure.AI.FormRecognizer.DocumentAnalysis;
using Azure.AI.OpenAI;
using Azure.Storage.Blobs;

using GenerativeAi.Functions;
using GenerativeAi.Functions.ingestion;
using GenerativeAi.Functions.settings;

using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using Redis;

using RedisSettings=GenerativeAi.Functions.settings.RedisSettings;

[assembly:FunctionsStartup(typeof(Startup))]

namespace GenerativeAi.Functions;

public class Startup : FunctionsStartup
{
    public override void Configure(IFunctionsHostBuilder builder)
    {
        builder.Services.AddSingleton<IConfiguration>(new ConfigurationBuilder().SetBasePath(builder.GetContext().ApplicationRootPath)
                                                                                .AddJsonFile("appsettings.json", optional:false, reloadOnChange:true)
                                                                                .AddJsonFile($"appsettings.{builder.GetContext().EnvironmentName}.json", optional:true, reloadOnChange:true)
                                                                                .AddEnvironmentVariables()
                                                                                .AddUserSecrets<Startup>()
                                                                                .Build());
        builder.Services.AddSingleton(provider =>
                                      {
                                          var settings = provider.GetService<IConfiguration>()
                                                                 .GetSection("azure_cognitiveservices")
                                                                 .Get<AzureCognativeServicesSettings>();
                                          return new DocumentAnalysisClient(settings.EndPoint, new AzureKeyCredential(settings.Key));
                                      });
        builder.Services.AddSingleton(provider =>
                                      {
                                          var settings = provider.GetService<IConfiguration>()
                                                                 .GetSection("azure_openai")
                                                                 .Get<AzureOpenAiSettings>();
                                          return new OpenAIClient(settings.EndPoint, new AzureKeyCredential(settings.Key));
                                      });
        builder.Services.AddSingleton(provider =>
                                      {
                                          var settings = provider.GetService<IConfiguration>()
                                                                 .GetSection("azure_storage")
                                                                 .Get<AzureStorageSettings>();
                                          return new BlobServiceClient(settings.ConnectionString);
                                      });

        builder.Services.AddSingleton(provider => new RedisFactory(provider.GetService<IConfiguration>()
                                                                           .GetSection("azure_cache")
                                                                           .Get<RedisSettings>()
                                                                           .AsSettings()).Documents());
        builder.Services.AddSingleton<Embed, TextAda002Embedding>();
        builder.Services.AddSingleton(_ => new ModelSelector("prebuilt-document"));
    }
}