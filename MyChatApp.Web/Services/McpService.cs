using Microsoft.Extensions.AI;
using ModelContextProtocol.Client;
// using ModelContextProtocol.Configuration;
using ModelContextProtocol.Protocol.Transport;
using ModelContextProtocol.Protocol.Types;
using ModelContextProtocol.Server;

namespace MyChatApp.Web.Services
{
    public class McpService
    {
        private readonly string[] _toolsToUse = ["WhoIs"];

        private readonly Task<IMcpClient> _clientTask = McpClientFactory.CreateAsync(

            clientTransport: new SseClientTransport(new SseClientTransportOptions()
            {
                Endpoint = new Uri($"{Environment.GetEnvironmentVariable($"services__mcp-server__http__0")}/sse"),
            }),
                // serverConfig: new()
                // {
                //     Id = "mcp-server",
                //     Name = "Mcp-Server",
                //     Location = $"{Environment.GetEnvironmentVariable($"services__mcp-server__http__0")}/sse",
                //     TransportType = TransportTypes.Sse
                // },
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
            try
            {

                // Fix for CS1061: Use 'ToListAsync()' to materialize the IAsyncEnumerable<Tool> into a list
                // .ToListAsync();
                var tools = await client.ListToolsAsync();

                //var myTools = tools.Select(tool => client..AsAIFunction(tool));

                //return myTools;

                return tools;
                

                // var allTools = await client.GetAIFunctionsAsync();
                // return allTools.Where(tool => _toolsToUse.Contains(tool.Name));
            }
            catch (Exception ex)
            {
                throw;
            }
           
        }
        //public async Task<IMcpClient> GetClient() {

        //    McpClientOptions mcpClientOptions = new()
        //    { ClientInfo = new() { Name = "mcp-client", Version = "1.0.0" } };

        //    var client = new HttpClient();
        //    //client.BaseAddress = new("https+http://mcp-server");
        //    client.BaseAddress = new("http://mcp-server");

        //    // can't use the service discovery for ["https +http://aspnetsseserver"]
        //    // fix: read the environment value for the key 'services__aspnetsseserver__https__0' to get the url for the aspnet core sse server
        //    var serviceName = "mcp-server";
        //    var nameSSL = $"services__{serviceName}__https__0";
        //    var name = $"services__{serviceName}__http__0";
        //    var urlSSL = Environment.GetEnvironmentVariable(nameSSL) + "/sse";
        //    var url = Environment.GetEnvironmentVariable(name) + "/sse";

        //    McpServerConfig mcpServerConfig = new()
        //    {
        //        Id = "mcp-server",
        //        Name = "Mcp-Server",
        //        Location = url, //$"{client.BaseAddress}/sse",
        //        TransportType = TransportTypes.Sse
        //    };

        //    try
        //    {
        //        var mcpClient = await McpClientFactory.CreateAsync(mcpServerConfig, mcpClientOptions);

        //        return mcpClient;
        //    }
        //    catch (Exception ex)
        //    {
        //        throw;
        //    }            
            
        //}
    }
      
}
