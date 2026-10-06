# GitHub Explorer MCP Server

A Model Context Protocol (MCP) server that enables AI assistants to explore and learn about GitHub repositories. Built with C#, the Microsoft MCP SDK, and Octokit.NET.

## Features

- 📖 Explore repository overviews with README content
- 📁 Browse directory structures and file listings
- 🔍 Read specific files from any repository
- 🔎 Search code across GitHub repositories
- 📝 View recent commits to understand active development
- 💬 Browse issues to see common problems and discussions

## Getting Your GitHub Token
1. Go to github.com → Click your profile picture → Settings
2. Scroll down left sidebar → Developer settings
3. Click Personal access tokens → Tokens (classic)
4. Click Generate new token (classic)
5. Set expiration and check these scopes:
	- ✅ repo (full control of private/public repos)
	- ✅ read:user (basic user info)
6. Click Generate token
7. Copy the token immediately (you won't see it again!)

## Add Environment Variable to windows
1. Press Win + R, type sysdm.cpl, press Enter
2. Click Advanced tab → Environment Variables
3. Under "User variables", click New...
4. Name: GITHUB_TOKEN
5. Value: Your token (e.g., ghp_xxxxxxxxxxxxxxxxxxxx)
6. Click OK on all dialogs
7. Restart your terminal or LM Studio
8. Test by opening windows terminal and run "echo $env:GITHUB_TOKEN"

## Setup LM Studio
1. Open up the right sidebar clocking the icon in the top right.
2. Under the "Integrations" tab click Install to edit your mcp.json
3. Edit your mcp.json to look something like this.

* Using URL
```json
{
  "mcpServers": {
    "GitExplorer": {
      "url": "http://localhost:6248",
      "headers": {
        "Authorization": "Bearer <token-placeholder>"
      }
    }
  }
}
```

* Using source
```json
{
  "mcpServers": {
    "GitExplorer": {
      "command": "dotnet",
      "args": [
        "run",
        "--project",
        "<path-placeholder(escape directories with double forward slashes)>"
      ],
      "env": {
        "GITHUB_TOKEN": "<token-placeholder>"
      }
    }
  }
}
```

* Using release DLL
```json
{
  "mcpServers": {
    "GitExplorer": {
      "command": "dotnet",
      "args": ["<path-placeholder(escape directories with double forward slashes)>"],
      "env": {
        "GITHUB_TOKEN": "<token-placeholder>"
      }
    }
  }
}
```