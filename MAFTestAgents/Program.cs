using Azure;
using Azure.AI.OpenAI;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Configuration;

namespace MAFTestAgents
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("Setting up Config...");
            var configurationBuilder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddUserSecrets<Program>(optional: true, reloadOnChange: true);
            var config = configurationBuilder.Build();

            // IMPORTANT: You ONLY NEED either Azure OpenAI or OpenAI connectiopn info, not both.
            // Azure OpenAI Connection Info
            var azureOpenAIEndpoint = config["AzureOpenAI:Endpoint"];
            var azureOpenAIAPIKey = config["AzureOpenAI:APIKey"];
            var azureOpenAIModelDeploymentName = config["AzureOpenAI:ModelDeploymentName"];

            var azureOpenAIEndpointUri = new Uri(azureOpenAIEndpoint);
            var azureApiKeyCredential = new AzureKeyCredential(azureOpenAIAPIKey);
            var azureOpenAIClient = new AzureOpenAIClient(
                azureOpenAIEndpointUri, azureApiKeyCredential);

            // Retrieve the Chat Client
            var azureOpenAIChatClient = azureOpenAIClient.GetChatClient(azureOpenAIModelDeploymentName).AsIChatClient();

            var agentOptions = new ChatClientAgentOptions
            {
                Name = "Writer",
                Instructions = "Write stories that are engaging and creative."
            };

            var agentWriter = new ChatClientAgent(
                azureOpenAIChatClient,
                agentOptions
                );

            AgentRunResponse response = await agentWriter.RunAsync("Write a short story about a haunted house.");

            Console.WriteLine(response.Text);
        }
    }
}
