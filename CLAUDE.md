# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

This is a UK Parliament OpenData MCP (Model Context Protocol) Server that provides comprehensive access to UK Parliamentary data through various APIs. The server acts as a bridge between AI assistants and official UK Parliament data sources, enabling queries about MPs, Lords, bills, voting records, committees, and more.

## Core Architecture

- **Single C# Project**: `OpenData.Mcp.Server` - A .NET 9.0 console application
- **MCP Server**: Uses the ModelContextProtocol NuGet package to expose Parliament APIs as MCP tools
- **HTTP Client Factory**: Centralized HTTP client management with timeout and retry logic
- **Comprehensive API Coverage**: Integrates with 10+ UK Parliament APIs including Members, Bills, Committees, Voting records, and more

## Key Files

- `Program.cs`: MCP server setup with dependency injection, logging, and stdio transport
- `ParliamentApiTool.cs`: Main tool class containing 100+ MCP tools for Parliament APIs
- `OpenData.Mcp.Server.csproj`: Project configuration targeting .NET 9.0

## Common Commands

### Build and Run
```bash
dotnet build OpenData.Mcp.Server/OpenData.Mcp.Server.csproj
dotnet run --project OpenData.Mcp.Server/OpenData.Mcp.Server.csproj
```

### Development
```bash
# Restore dependencies
dotnet restore

# Build in debug mode
dotnet build --configuration Debug

# Build in release mode
dotnet build --configuration Release

# Run with specific verbosity
dotnet run --project OpenData.Mcp.Server/OpenData.Mcp.Server.csproj --verbosity normal
```

## MCP Tool Development

All MCP tools are defined in `ParliamentApiTool.cs` with the following pattern:
- Use `[McpServerTool]` attribute with `ReadOnly = true, Idempotent = true, OpenWorld = false`
- Include detailed `[Description]` attributes for tool discovery
- Parameter descriptions using `[Description]` on parameters for better usability
- Consistent error handling and retry logic in `GetResult()` method

### Parameter Description Patterns

When adding parameter descriptions, follow these patterns:
- **IDs**: "Parliament member ID", "Unique bill ID number", "Committee ID to get events for"
- **Dates**: "Start date in YYYY-MM-DD format", "Optional: end date in YYYY-MM-DD format"
- **Search terms**: "Search term for bill titles or content", "Optional: search term for evidence content"
- **Pagination**: "Number of records to skip (for pagination)", "Number of records to return (default 20, max 100)"
- **Filters**: "Optional: filter by specific committee ID", "House number (1=Commons, 2=Lords)"
- **Booleans**: "Include event attendees in response", "Show only events visible on website"

### Common Parameter Types

- **Member IDs**: Always describe as "Parliament member ID to get [data] for"
- **House numbers**: Always specify "1=Commons, 2=Lords"
- **Optional parameters**: Prefix with "Optional:" when parameter is nullable
- **Pagination**: Include default values and max recommendations
- **Search terms**: Provide example search queries when helpful

## API Integration

The server integrates with these UK Parliament APIs:
- Members API (MPs/Lords data)
- Bills API (legislation tracking)
- Committees API (committee activities)
- Voting APIs (Commons/Lords divisions)
- Hansard API (parliamentary record)
- Now API (live chamber activity)
- And more...

## Error Handling

- HTTP timeout: 30 seconds
- Retry logic: 3 attempts with exponential backoff
- Transient failure detection for 5xx errors and timeouts
- Structured JSON error responses with URL and error details

## Usage in MCP Clients

The server is designed to be used with MCP-compatible clients like Visual Studio Code with Copilot. Users should start with the "Hello Parliament" tool to get proper system prompt instructions for optimal AI assistant behavior.