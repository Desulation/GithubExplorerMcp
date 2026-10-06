using GithubExplorerMcp.Tools;
using Octokit;

class Program
{
    static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Configure logging to stderr
        builder.Logging.AddConsole(consoleLogOptions =>
        {
            consoleLogOptions.LogToStandardErrorThreshold = LogLevel.Trace;
        });

        // Register HttpContextAccessor (Required for Scoped services to access headers)
        builder.Services.AddHttpContextAccessor();

        // Register GitHubClient as SCOPED (Per-Request)
        builder.Services.AddScoped(sp =>
        {
            var httpContextAccessor = sp.GetRequiredService<IHttpContextAccessor>();
            var httpContext = httpContextAccessor.HttpContext;

            if (httpContext == null)
            {
                throw new InvalidOperationException("HttpContext is not available.");
            }

            // Extract token from headers
            string token = String.Empty;

            // Option 1: Authorization: Bearer <token>
            if (httpContext.Request.Headers.ContainsKey("Authorization"))
            {
                var authHeader = httpContext.Request.Headers["Authorization"];
                if (authHeader[0].Length > 0 && authHeader[0].StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
                {
                    token = authHeader[0].Substring("Bearer ".Length).Trim();
                }
            }

            // Option 2: X-GitHub-Token: <token>
            if (string.IsNullOrEmpty(token) && httpContext.Request.Headers.ContainsKey("X-GitHub-Token"))
            {
                token = httpContext.Request.Headers["X-GitHub-Token"][0];
            }

            if (string.IsNullOrEmpty(token))
            {
                throw new UnauthorizedAccessException("GitHub token not provided in request headers.");
            }

            var product = new ProductHeaderValue("github-mcp-explorer", "1.0.0");
            var client = new GitHubClient(product);
            client.Credentials = new Credentials(token, AuthenticationType.Bearer);
            return client;
        });

        // Register tools with DI (github client and logger)
        builder.Services.AddScoped<GithubExplorerTools>();

        // Configure MCP Server
        builder.Services
            .AddMcpServer()
            .WithHttpTransport(options =>
            {
                options.Stateless = true;
            })
            .WithToolsFromAssembly();

        var app = builder.Build();
        app.MapMcp();
        app.UseHttpsRedirection();

        app.Run();
    }
}
