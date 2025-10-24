using Azure;
using Azure.AI.OpenAI;
using GSHCommon.AzureOpenAI;

namespace GSHMCPServer.AzureOpenAI
{
    public class AzureOpenAIClient
    {
        private static readonly string DefaultAzureOpenAIEndpoint = "https://ammondal-llm-test.openai.azure.com/";
        private static readonly string DefaultModelName = "gpt-4.1";

        private readonly OpenAIClient client;

        public AzureOpenAIClient()
        {
            client = GetAzureOpenAIClient();
        }


        private string GetModelName() => Environment.GetEnvironmentVariable("AZURE_OPENAI_MODEL") ?? DefaultModelName;


        public async Task<string> RunChatCompletionAsync(string systemPrompt, string userPrompt)
        {
            var chatCompletionsOptions = new ChatCompletionsOptions()
            {
                DeploymentName = GetModelName(),
                Messages =
                {
                    new ChatRequestSystemMessage(systemPrompt),
                    new ChatRequestUserMessage(userPrompt)
                },
                MaxTokens = 2000,
                Temperature = 0.7f
            };

            var response = await client.GetChatCompletionsAsync(chatCompletionsOptions);
            var generatedContent = response.Value.Choices[0].Message.Content;
            return generatedContent;
        }


        public async Task<Response<Embeddings>> GetEmbeddingsAsync(EmbeddingsOptions embeddingsOptions)
        {
            return await client.GetEmbeddingsAsync(embeddingsOptions);
        }

        /// <summary>
        /// Gets Azure OpenAI client using the singleton factory with Visual Studio credentials
        /// </summary>
        private OpenAIClient GetAzureOpenAIClient()
        {
            // Get the endpoint from environment variable or use default
            var endpoint = Environment.GetEnvironmentVariable("AZURE_OPENAI_ENDPOINT") ?? DefaultAzureOpenAIEndpoint;

            // Use the singleton factory to get or create the client
            return AzureOpenAIClientFactory.Instance.GetOrCreateClient(endpoint);
        }

        /// <summary>
        /// Alternative method to get client with different credential types
        /// </summary>
        private OpenAIClient GetAzureOpenAIClientWithManagedIdentity()
        {
            var endpoint = Environment.GetEnvironmentVariable("AZURE_OPENAI_ENDPOINT") ?? DefaultAzureOpenAIEndpoint;
            return AzureOpenAIClientFactory.Instance.GetOrCreateClientWithManagedIdentity(endpoint);
        }
    }
}
