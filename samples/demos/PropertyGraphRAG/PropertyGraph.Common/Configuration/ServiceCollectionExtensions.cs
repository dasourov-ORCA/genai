﻿using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;
using System.Text.Json;

namespace PropertyGraph.Common;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddChatCompletionService(this IServiceCollection serviceCollection, OpenAIOptions openAIOptions)
    {
        switch (openAIOptions.Source)
        {
            case "OpenAI":
                serviceCollection = serviceCollection.AddOpenAIChatCompletion(modelId: openAIOptions.ChatModelId, apiKey: openAIOptions.ApiKey);
                break;

            default:
                throw new ArgumentException($"Invalid source: {openAIOptions.Source}");
        }

        return serviceCollection;
    }
}
public enum ApiLoggingLevel
{
    None = 0,
    RequestOnly = 1,
    ResponseAndRequest = 2,
}

public static class IKernelBuilderExtensions
{
	public static IKernelBuilder AddChatCompletionService(this IKernelBuilder kernelBuilder, OpenAIOptions openAIOptions, ApiLoggingLevel apiLoggingLevel = ApiLoggingLevel.None)
    {
        switch (openAIOptions.Source)
        {
            case "OpenAI":
                {
                    if (apiLoggingLevel == ApiLoggingLevel.None)
                    {
                        kernelBuilder = kernelBuilder.AddOpenAIChatCompletion(modelId: openAIOptions.ChatModelId, apiKey: openAIOptions.ApiKey);
                        break;
                    }
                    else
                    {
                        var client = CreateHttpClient(apiLoggingLevel);
                        kernelBuilder.AddOpenAIChatCompletion(openAIOptions.ChatModelId, openAIOptions.ApiKey, null, null, client);
                    }
                    break;
                }
            default:
                throw new ArgumentException($"Invalid source: {openAIOptions.Source}");
        }

        return kernelBuilder;
    }

    public static HttpClient CreateHttpClient(ApiLoggingLevel apiLoggingLevel)
    {
        HttpClientHandler httpClientHandler;
        if (apiLoggingLevel == ApiLoggingLevel.RequestOnly)
        {
            httpClientHandler = new RequestLoggingHttpClientHandler();
        }
        else
        {
            httpClientHandler = new RequestAndResponseLoggingHttpClientHandler();
        }
        var client = new HttpClient(httpClientHandler);
        return client;
    }
}

// Found most of this implementation via: https://github.com/microsoft/semantic-kernel/issues/5107
public class RequestAndResponseLoggingHttpClientHandler : HttpClientHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request, CancellationToken cancellationToken)
    {
		Console.WriteLine("***********************************************");
		Console.WriteLine($"Request: {request.Method} {request.RequestUri}");
		if (request.Content is not null)
        {
            var content = await request.Content.ReadAsStringAsync(cancellationToken);
            var json = JsonSerializer.Serialize(JsonSerializer.Deserialize<JsonDocument>(content),
                new JsonSerializerOptions { WriteIndented = true });
            Console.WriteLine(json);
        }
        
        var result = await base.SendAsync(request, cancellationToken);

        if (result.Content is not null)
        {
            var content = await result.Content.ReadAsStringAsync(cancellationToken);
            //var json = JsonSerializer.Serialize(JsonSerializer.Deserialize<JsonDocument>(content),
            //    new JsonSerializerOptions { WriteIndented = true });
            Console.WriteLine("***********************************************");
            Console.WriteLine("Response:");
            //Console.WriteLine(json);
            Console.WriteLine(content);
        }

        return result;
    }
}
public class RequestLoggingHttpClientHandler : HttpClientHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request, CancellationToken cancellationToken)
    {
		Console.WriteLine("***********************************************");
		Console.WriteLine($"Request: {request.Method} {request.RequestUri}");
		if (request.Content is not null)
        {
            var content = await request.Content.ReadAsStringAsync(cancellationToken);
            var json = JsonSerializer.Serialize(JsonSerializer.Deserialize<JsonDocument>(content),
                new JsonSerializerOptions { WriteIndented = true });
            Console.WriteLine(json);
        }
        
        return await base.SendAsync(request, cancellationToken);
    }
}
