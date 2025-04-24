using Microsoft.Extensions.AI;
using ModelContextProtocol.Client;
using ModelContextProtocol.Protocol.Transport;

namespace MyChatApp.Web.Services
{
    public class McpService
    {
        private readonly string[] _toolsToUse = ["WhoIs", "SearchGroups"];

        private static readonly string _mcpServerName = "mcp-server";
        
        private static readonly string _httpScheme = "http";

        private readonly Task<IMcpClient> _clientTask = McpClientFactory.CreateAsync(

            clientTransport: new SseClientTransport(new SseClientTransportOptions()
            {
                Endpoint = new Uri($"{Environment.GetEnvironmentVariable($"services__{_mcpServerName}__{_httpScheme}__0")}/sse"),
            }),
               
            clientOptions: new()
            {
                ClientInfo = new()
                {
                    Name = "mcp-client",
                    Version = "1.0.0"
                }
            }
        );  

        public async Task<IEnumerable<AIFunction>> GetToolsAsync()
        {
            var client = await _clientTask;
            var tools = await client.ListToolsAsync();

            return tools.Where(tool => _toolsToUse.Contains(tool.Name));
        }
    }
}
