using System.ComponentModel;
using ModelContextProtocol.Server;

[McpServerToolType]
public static class WhoIsTool
{
    [McpServerTool, Description("Echoes the message back to the client.")]
    public static string WhoIs(string fullname) => $" {fullname}";

}