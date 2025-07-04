# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Overview

This is a .NET 9 console application that implements a Model Context Protocol (MCP) server for UK Parliament data. The server provides AI assistants with access to official UK Parliament APIs through MCP tools, enabling them to answer questions about MPs, Lords, legislation, debates, and parliamentary procedures.

## Development Commands

### Build and Run
```bash
# Build the project
dotnet build

# Run the project
dotnet run --project OpenData.Mcp.Server/OpenData.Mcp.Server.csproj

# Run in release mode
dotnet run --project OpenData.Mcp.Server/OpenData.Mcp.Server.csproj --configuration Release
```

### Development Workflow
- The project uses .NET 9 with nullable reference types enabled
- All logs are configured to go to stderr (see Program.cs:9)
- The server communicates via stdio transport for MCP protocol
- No explicit test framework is configured - check if testing is needed before adding tests

## Architecture

### Core Components

**Program.cs**: Entry point that configures the MCP server with:
- Console logging to stderr
- Stdio transport for MCP communication
- Automatic tool discovery from assembly
- HTTP client and memory caching services

**ParliamentApiTool.cs**: Main tool class containing 40+ MCP server tools that wrap UK Parliament APIs:
- Member information (MPs, Lords, search, contacts, biographies)
- Legislative data (bills, statutory instruments, acts)
- Parliamentary procedures (Hansard, divisions, debates)
- Committee information and meetings
- Real-time chamber activity ("happening now")
- Register of interests and transparency data
- Calendar and scheduling information

### Key Design Patterns

- **Dependency Injection**: Uses IHttpClientFactory for HTTP requests
- **MCP Tool Attributes**: Each method uses `[McpServerTool]` with descriptive attributes
- **URL Building**: Consistent URL construction with proper escaping
- **Response Format**: All tools return JSON with both URL and data for transparency
- **Error Handling**: Uses EnsureSuccessStatusCode() for HTTP responses

### API Integration

The server integrates with multiple UK Parliament APIs:
- members-api.parliament.uk (MP/Lord details, contacts, interests)
- bills-api.parliament.uk (legislation tracking)
- committees-api.parliament.uk (committee information)
- hansard-api.parliament.uk (parliamentary debates)
- commonsvotes-api.parliament.uk / lordsvotes-api.parliament.uk (voting records)
- now-api.parliament.uk (live chamber activity)
- interests-api.parliament.uk (register of interests)
- And 10+ other specialized APIs

### Tool Characteristics

All tools are marked as:
- ReadOnly = true (no data modification)
- Idempotent = true (safe to retry)
- OpenWorld = false (controlled scope)

Tools include comprehensive descriptions to help AI assistants understand when and how to use each API endpoint.

## Usage Context

This server is designed to be used with AI assistants (like Microsoft Copilot) that support MCP protocol. The README.md contains detailed setup instructions for VS Code integration and recommended system prompts for optimal AI interaction.

The server enables AI assistants to provide factual, source-cited answers about UK Parliament using official data rather than potentially outdated training data.