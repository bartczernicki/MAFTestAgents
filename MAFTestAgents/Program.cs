using Azure;
using Azure.AI.OpenAI;
using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Workflows;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Configuration;
using OpenAI.Chat;

namespace MAFTestAgents
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            // https://devblogs.microsoft.com/dotnet/introducing-microsoft-agent-framework-preview/ 

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

            var agentWriterOptions = new ChatClientAgentOptions
            {
                Name = "Writer",
                Instructions = "Write stories that are engaging and creative."
            };

            var agentEditorOptions = new ChatClientAgentOptions
            {
                Name = "Editor",
                Instructions = "Make the story more engaging, fix grammar, and enhance the plot."
            };


            var agentWriter = new ChatClientAgent(
                azureOpenAIChatClient,
                agentWriterOptions
                );

            var agentEditor = new ChatClientAgent(
                azureOpenAIChatClient,
                agentEditorOptions
                );

            // AgentRunResponse response = await agentWriter.RunAsync("Write a short story about a haunted house.");
            // Console.WriteLine(response.Text);

            // Create a workflow that connects writer to editor
            Workflow workflow =
                AgentWorkflowBuilder
                    .BuildSequential(agentWriter, agentEditor);

            AIAgent workflowAgent = await workflow.AsAgentAsync();


            AgentRunResponse workflowResponse =
                await workflowAgent.RunAsync("Write a short story about a haunted house.");

            Console.WriteLine(workflowResponse.Text);
        }
    }
}
