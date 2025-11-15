using Azure;
using Azure.AI.OpenAI;
using Azure.Core;
using Azure.Identity;
using System;
using System.Collections.Concurrent;

namespace GSHCommon.AzureOpenAI
{
    /// <summary>
    /// Singleton factory class for creating and managing Azure OpenAI clients.
    /// Provides thread-safe client creation and reuse based on endpoint and credential combination.
    /// </summary>
    public sealed class AzureOpenAIClientFactory
    {
        private static readonly Lazy<AzureOpenAIClientFactory> _instance = 
            new Lazy<AzureOpenAIClientFactory>(() => new AzureOpenAIClientFactory());

        /// <summary>
        /// Gets the singleton instance of the factory.
        /// </summary>
        public static AzureOpenAIClientFactory Instance => _instance.Value;

        /// <summary>
        /// Cache to store OpenAI clients by their configuration key.
        /// Key format: "{endpoint}::{credentialType}"
        /// </summary>
        private readonly ConcurrentDictionary<string, OpenAIClient> _clientCache = 
            new ConcurrentDictionary<string, OpenAIClient>();

        /// <summary>
        /// Private constructor to enforce singleton pattern.
        /// </summary>
        private AzureOpenAIClientFactory()
        {
        }

        /// <summary>
        /// Gets or creates an Azure OpenAI client using Visual Studio credentials.
        /// This method provides a singleton client per endpoint.
        /// </summary>
        /// <param name="endpoint">The Azure OpenAI endpoint URL. If null or empty, uses default endpoint.</param>
        /// <returns>A configured OpenAI client instance.</returns>
        public OpenAIClient GetOrCreateClient(string endpoint = null)
        {
            string? apiKey = Environment.GetEnvironmentVariable("AZURE_OPENAI_KEY");
            if (string.IsNullOrEmpty(apiKey))
            {     
                return GetOrCreateClient(endpoint, new VisualStudioCredential());
            }
            else
            {
                var credential = new AzureKeyCredential(apiKey!);
                return GetOrCreateClient(endpoint, credential);
            }
        }

        /// <summary>
        /// Gets or creates an Azure OpenAI client with the specified endpoint and credential.
        /// This method provides a singleton client per unique endpoint and credential type combination.
        /// </summary>
        /// <param name="endpoint">The Azure OpenAI endpoint URL. If null or empty, uses default endpoint.</param>
        /// <param name="credential">The token credential to use for authentication.</param>
        /// <returns>A configured OpenAI client instance.</returns>
        public OpenAIClient GetOrCreateClient(string endpoint, AzureKeyCredential credential)
        {
            if (string.IsNullOrWhiteSpace(endpoint))
            {
                endpoint = GetDefaultEndpoint();
            }

            if (credential == null)
            {
                throw new ArgumentNullException(nameof(credential), "Token credential cannot be null");
            }

            // Create a cache key based on endpoint and credential type
            string cacheKey = $"{endpoint}::{credential.GetHashCode}";

            return _clientCache.GetOrAdd(cacheKey, _ => CreateClient(endpoint, credential));
        }


        /// <summary>
        /// Gets or creates an Azure OpenAI client with the specified endpoint and credential.
        /// This method provides a singleton client per unique endpoint and credential type combination.
        /// </summary>
        /// <param name="endpoint">The Azure OpenAI endpoint URL. If null or empty, uses default endpoint.</param>
        /// <param name="credential">The token credential to use for authentication.</param>
        /// <returns>A configured OpenAI client instance.</returns>
        public OpenAIClient GetOrCreateClient(string endpoint, TokenCredential credential)
        {
            if (string.IsNullOrWhiteSpace(endpoint))
            {
                endpoint = GetDefaultEndpoint();
            }

            if (credential == null)
            {
                throw new ArgumentNullException(nameof(credential), "Token credential cannot be null");
            }

            // Create a cache key based on endpoint and credential type
            string cacheKey = CreateCacheKey(endpoint, credential);

            return _clientCache.GetOrAdd(cacheKey, _ => CreateClient(endpoint, credential));
        }

        /// <summary>
        /// Gets or creates an Azure OpenAI client using Managed Identity credentials.
        /// </summary>
        /// <param name="endpoint">The Azure OpenAI endpoint URL. If null or empty, uses default endpoint.</param>
        /// <returns>A configured OpenAI client instance.</returns>
        public OpenAIClient GetOrCreateClientWithManagedIdentity(string endpoint = null)
        {
            return GetOrCreateClient(endpoint, new ManagedIdentityCredential());
        }

        /// <summary>
        /// Gets or creates an Azure OpenAI client using Default Azure credentials.
        /// This will try multiple credential sources in order: Environment, Managed Identity, Visual Studio, etc.
        /// </summary>
        /// <param name="endpoint">The Azure OpenAI endpoint URL. If null or empty, uses default endpoint.</param>
        /// <returns>A configured OpenAI client instance.</returns>
        public OpenAIClient GetOrCreateClientWithDefaultCredentials(string endpoint = null)
        {
            return GetOrCreateClient(endpoint, new DefaultAzureCredential());
        }

        /// <summary>
        /// Clears all cached clients and disposes them if they implement IDisposable.
        /// This should typically only be called during application shutdown.
        /// </summary>
        public void ClearCache()
        {
            foreach (var client in _clientCache.Values)
            {
                if (client is IDisposable disposableClient)
                {
                    try
                    {
                        disposableClient.Dispose();
                    }
                    catch (Exception ex)
                    {
                        // Log disposal errors but don't throw, as this is cleanup code
                        System.Diagnostics.Debug.WriteLine($"Error disposing OpenAI chatCompletionsClient: {ex.Message}");
                    }
                }
            }

            _clientCache.Clear();
        }

        /// <summary>
        /// Gets the current number of cached clients.
        /// </summary>
        public int CachedClientCount => _clientCache.Count;

        /// <summary>
        /// Creates a new OpenAI client instance.
        /// </summary>
        /// <param name="endpoint">The Azure OpenAI endpoint URL.</param>
        /// <param name="credential">The token credential for authentication.</param>
        /// <returns>A new OpenAI client instance.</returns>
        private static OpenAIClient CreateClient(string endpoint, TokenCredential credential)
        {
            try
            {
                var uri = new Uri(endpoint);
                return new OpenAIClient(uri, credential);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(
                    $"Failed to create Azure OpenAI chatCompletionsClient for endpoint '{endpoint}': {ex.Message}", ex);
            }
        }


        /// <summary>
        /// Creates a new OpenAI client instance.
        /// </summary>
        /// <param name="endpoint">The Azure OpenAI endpoint URL.</param>
        /// <param name="credential">The token credential for authentication.</param>
        /// <returns>A new OpenAI client instance.</returns>
        private static OpenAIClient CreateClient(string endpoint, AzureKeyCredential credential)
        {
            try
            {
                var uri = new Uri(endpoint);
                return new OpenAIClient(uri, credential);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(
                    $"Failed to create Azure OpenAI chatCompletionsClient for endpoint '{endpoint}': {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Creates a cache key for the client based on endpoint and credential type.
        /// </summary>
        /// <param name="endpoint">The endpoint URL.</param>
        /// <param name="credential">The credential instance.</param>
        /// <returns>A unique cache key string.</returns>
        private static string CreateCacheKey(string endpoint, TokenCredential credential)
        {
            string credentialTypeName = credential.GetType().Name;
            return $"{endpoint}::{credentialTypeName}";
        }

        /// <summary>
        /// Gets the default Azure OpenAI endpoint.
        /// This can be configured via environment variable or falls back to a default value.
        /// </summary>
        /// <returns>The default endpoint URL.</returns>
        private static string GetDefaultEndpoint()
        {
            // Try to get from environment variable first
            string endpoint = Environment.GetEnvironmentVariable("AZURE_OPENAI_ENDPOINT");
            
            if (string.IsNullOrWhiteSpace(endpoint))
            {
                // Fallback to default endpoint - you should replace this with your actual default
                endpoint = "https://ammondal-llm-test.openai.azure.com/";
            }

            return endpoint;
        }
    }
}