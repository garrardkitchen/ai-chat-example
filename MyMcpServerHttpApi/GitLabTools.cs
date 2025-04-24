using System.ComponentModel;
using ModelContextProtocol.Server;
using Microsoft.Extensions.Configuration;
using Garrard.GitLab;

[McpServerToolType]
public class GitLabTools
{
    private readonly IConfiguration _configurationManager;
    
    public GitLabTools(IConfiguration configurationManager)
    {
        _configurationManager = configurationManager;
    }

    // Prompts:
    // 1 - table example: I would like to get a list of gitlab groups based on a search pattern and the result to be put in a  markdown table including their (1) name, (2) have web_url as a url link with the word 'click me' and (3) parent_id and (4) a suitable emoji to indicate if has_subgroups is true. if a group has the parent id that equals the group Id, then group those beneath it. include the group id in brackets after the group name
    // 2 - tree example: Create a nested tree structure of GitLab groups based on the search pattern 'upe', organized by parent ID and group ID, using text-based tree icons to highlight the hierarchy. Include group names, IDs, and connect relationships visually with authentic tree icons
    [McpServerTool, Description("Returns a list of Groups in GitLab")]
    public async Task<IEnumerable<GitLabGroupDto>> SearchGroupsAsync(string pattern) {

        var token = _configurationManager["GitLab:Token"] ?? throw new Exception("Missing GitLab:Token");
        var domain = _configurationManager["GitLab:Domain"] ?? throw new Exception("Missing GitLab:Domain");
        var groups = await GroupOperations.SearchGroups(pattern, token, domain);

        if (groups.IsSuccess)
        {
            return groups.Value.ToList();
        }
        return new List<GitLabGroupDto>();
    }

}