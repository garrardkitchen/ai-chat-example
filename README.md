# AI Chat with Custom Data

This project is an AI chat application that demonstrates how to chat with custom data using an AI language model. Please note that this template is currently in an early preview stage. If you have feedback, please take a [brief survey](https://aka.ms/dotnet-chat-templatePreview2-survey).

>[!NOTE]
> Before running this project you need to configure the API keys or endpoints for the providers you have chosen. See below for details specific to your choices.

# Configure the AI Model Provider

## Using GitHub Models
To use models hosted by GitHub Models, you will need to create a GitHub personal access token. The token should not have any scopes or permissions. See [Managing your personal access tokens](https://docs.github.com/en/authentication/keeping-your-account-and-data-secure/managing-your-personal-access-tokens).

Configure your token for this project using .NET User Secrets:

1. In Visual Studio, right-click on the MyChatApp.AppHost project in the Solution Explorer and select "Manage User Secrets".
2. This opens a `secrets.json` file where you can store your API keys without them being tracked in source control. Add the following key and value:

   ```json
   {
     "ConnectionStrings:openai": "Endpoint=https://models.inference.ai.azure.com;Key=YOUR-API-KEY"
   }
   ```

Learn more about [prototyping with AI models using GitHub Models](https://docs.github.com/github-models/prototyping-with-ai-models).

# Running the application

## Using Visual Studio

1. Open the `.sln` file in Visual Studio.
2. Press `Ctrl+F5` or click the "Start" button in the toolbar to run the project.

## Using Visual Studio Code

1. Open the project folder in Visual Studio Code.
2. Install the [C# Dev Kit extension](https://marketplace.visualstudio.com/items?itemName=ms-dotnettools.csdevkit) for Visual Studio Code.
3. Once installed, Open the `Program.cs` file in the MyChatApp.AppHost project.
4. Run the project by clicking the "Run" button in the Debug view.

## Trust the localhost certificate

Several .NET Aspire templates include ASP.NET Core projects that are configured to use HTTPS by default. If this is the first time you're running the project, an exception might occur when loading the Aspire dashboard. This error can be resolved by trusting the self-signed development certificate with the .NET CLI.

See [Troubleshoot untrusted localhost certificate in .NET Aspire](https://learn.microsoft.com/dotnet/aspire/troubleshooting/untrusted-localhost-certificate) for more information.

# Learn More
To learn more about development with .NET and AI, check out the following links:

* [AI for .NET Developers](https://learn.microsoft.com/dotnet/ai/)


---

# Secrets

## MyChatApp.Web


GitHub PAT

_should not have any scopes or permissions_

```bash
dotnet user-secrets set "GitHubModels:Token" "<your-token-value>"
```

ConnectionStrings for OpenAI (use for GH Model)

```bash
dotnet user-secrets set "ConnectionStrings:openai" "Endpoint=https://models.inference.ai.azure.com;Key=<your-token-value>"
```

## MyMcpServerHttpApi

GitLab PAT

```bash

dotnet user-secrets set "GitLab:Token" "<your-token-value>"
```

GitLab Domain

```bash

dotnet user-secrets set "GitLab:domain" "<your-domain-value>"
```

---

# Example prompts:

You will be prompt for a group name after you enter these prommpts.

1 - A markdown table

>[!NOTE]
> I would like to get a list of gitlab groups based on a search pattern and the result to be put in a  markdown table including their (1) name, (2) have web_url as a url link with the word 'click me' and (3) parent_id and (4) a suitable emoji to indicate if has_subgroups is true. if a group has the parent id that equals the group Id, then group those beneath it. include the group id in brackets after the group name

![alt text](images/readme-table.png)

2 - A Tree structure

>[!NOTE]
> Create a tree structure nesting the groups by parent id and group id

![alt text](images/readme-tree.png)

