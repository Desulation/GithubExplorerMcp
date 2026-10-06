using ModelContextProtocol.Server;
using Octokit;
using System.ComponentModel;

namespace GithubExplorerMcp.Tools
{
    [McpServerToolType]
    public class GithubExplorerTools
    {
        private readonly GitHubClient _client;
        private readonly ILogger<GithubExplorerTools> _logger;

        public GithubExplorerTools(GitHubClient client, ILogger<GithubExplorerTools> logger)
        {
            _client = client;
            _logger = logger;
        }

        [McpServerTool]
        [Description("Get an overview of a GitHub repository including its README")]
        public async Task<string> ExploreRepo(
            [Description("GitHub repository owner")] string owner,
            [Description("GitHub repository name")] string name)
        {
            try
            {
                var repo = await _client.Repository.Get(owner, name);

                string readmeContent = "";
                try
                {
                    var readme = await _client.Repository.Content.GetReadme(owner, name);
                    readmeContent = readme.Content;
                }
                catch { readmeContent = "(No README found)"; }

                return $"# {repo.Name}\n\n{repo.Description ?? "No description"}\n\n" +
                       $"**Stars:** {repo.StargazersCount} | **Forks:** {repo.ForksCount} | **Open Issues:** {repo.OpenIssuesCount}\n\n" +
                       $"## README:\n\n{readmeContent}";
            }
            catch (Exception ex)
            {                
                return $"Error exploring repository: {ex.Message}";
            }
        }

        [McpServerTool]
        [Description("List files and directories at a specific path in the repository")]
        public async Task<string> ListFiles(
            [Description("GitHub repository owner")] string owner, 
            [Description("GitHub repository name")] string name,
            [Description("Path to file relative to repo base directory(optional parameter default == /)")] string path = "/")
        {
            try
            {
                var contents = await _client.Repository.Content.GetAllContents(owner, name, path);

                var result = $"Files in '{path}':\n\n";
                foreach (var item in contents)
                {
                    result += $"{(item.Type == "dir" ? "📁" : "📄")} {item.Name}\n";
                }
                return result;
            }
            catch (Exception ex)
            {
                return $"Error listing files: {ex.Message}";
            }
        }

        [McpServerTool]
        [Description("Read the contents of a specific file in the repository")]
        public async Task<string> ReadFile(
            [Description("GitHub repository owner")] string owner, 
            [Description("GitHub repository name")] string name, 
            [Description("Path to file relative to repo base directory(optional parameter default == /)")]string path = "/")
        {
            try
            {
                var content = await _client.Repository.Content.GetAllContents(owner, name, path);                

                return $"# {path}\n\n```\n{content.FirstOrDefault().Content}\n```";
            }
            catch (Exception ex)
            {
                return $"Error reading file: {ex.Message}";
            }
        }

        [McpServerTool]
        [Description("Search for code patterns across GitHub repositories")]
        public async Task<string> SearchCode(
            [Description("Search for code")] string query,
            [Description("Limit results returned(optional parameter default == 5)")] int limit = 5)
        {
            try
            {
                var searchQuery = new SearchCodeRequest(query);
                var results = await _client.Search.SearchCode(searchQuery);

                var output = $"Found {results.TotalCount} results:\n\n";
                foreach (var item in results.Items.Take(limit))
                {
                    output += $"- [{item.Name}]({item.HtmlUrl})\n  {item.Repository.FullName}\n\n";
                }
                return output;
            }
            catch (Exception ex)
            {
                return $"Error searching: {ex.Message}";
            }
        }

        [McpServerTool]
        [Description("Get recent commits for a repository to understand active development")]
        public async Task<string> RecentCommits(
            [Description("GitHub repository owner")] string owner, 
            [Description("GitHub repository name")] string name,
            [Description("Limit results returned(optional parameter default == 5)")] int limit = 5)
        {
            try
            {
                var commits = await _client.Repository.Commit.GetAll(owner, name);

                var output = $"Recent commits in {owner}/{name}:\n\n";
                foreach (var commit in commits.Take(limit))
                {
                    output += $"- [{commit.Sha.Substring(0, 7)}]({commit.HtmlUrl}) {commit.Commit.Message}\n";
                    output += $"  by {commit.Commit.Author.Name} on {commit.Commit.Author.Date:yyyy-MM-dd}\n\n";
                }
                return output;
            }
            catch (Exception ex)
            {
                return $"Error getting commits: {ex.Message}";
            }
        }

        [McpServerTool]
        [Description("Get repository issues to understand common problems and discussions")]
        public async Task<string> GetIssues(
            [Description("GitHub repository owner")] string owner, 
            [Description("GitHub repository name")] string name,
            [Description("Limit results returned(optional parameter default == 5)")] int limit = 5)
        {
            try
            {
                var shouldPrioritize = new RepositoryIssueRequest
                {
                    Assignee = "none",
                    Milestone = "none",
                    State = ItemStateFilter.Open,
                    Since = DateTimeOffset.Now.Subtract(TimeSpan.FromDays(14)),
                    Filter = IssueFilter.All
                };

                var issues = await _client.Issue.GetAllForRepository(owner, name, shouldPrioritize);

                var output = $"Recent {ItemStateFilter.Open} issues in {owner}/{name}:\n\n";
                foreach (var issue in issues.Take(limit))
                {
                    output += $"- [{issue.Number}]({issue.HtmlUrl}) {issue.Title}\n";
                    output += $"  by {issue.User.Login} on {issue.CreatedAt:yyyy-MM-dd}\n\n";
                }
                return output;
            }
            catch (Exception ex)
            {
                return $"Error getting issues: {ex.Message}";
            }
        }
    }
}
