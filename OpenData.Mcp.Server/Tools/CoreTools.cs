using Microsoft.Extensions.Logging;
using ModelContextProtocol.Server;
using OpenData.Mcp.Server.Tools;
using System.ComponentModel;

namespace OpenData.Mcp.Server
{
    [McpServerToolType]
    public class CoreTools(IHttpClientFactory httpClientFactory, ILogger<CoreTools> logger) : BaseTools(httpClientFactory, logger)
    {
        [McpServerTool(ReadOnly = true, Idempotent = true, OpenWorld = false), Description("Initialize Parliament data assistant with system prompt | setup, configuration, start session, getting started, how to use, instructions | Use FIRST when beginning parliamentary research to get proper assistant behavior and data handling guidelines | Returns system prompt for optimal parliamentary data interaction")]
        public async Task<string> HelloParliament()
        {
            return GetSystemPrompt();
        }

        [McpServerTool(ReadOnly = true, Idempotent = true, OpenWorld = false), Description("End Parliament session and restore normal assistant behavior | exit, quit, finish session, reset, normal mode, end parliamentary mode | Use when finished with parliamentary research to return to standard assistant behavior | Removes parliamentary data restrictions and requirements")]
        public async Task<string> GoodByeParliament()
        {
            return GetGoodbyePrompt();
        }

        /// <summary>
        /// Standard system prompt for Parliament data assistant initialization.
        /// </summary>
        private string GetSystemPrompt()
        {
            return @"You are a helpful assistant that answers questions using only data from UK Parliament MCP servers.
                When the session begins, introduce yourself with a brief message such as:
                ""Hello! I'm a parliamentary data assistant. I can help answer questions using official data from the UK Parliament MCP APIs. Just ask me something, and I'll fetch what I can — and I'll always show you which sources I used.""
                When responding to user queries, you must:
                Only retrieve and use data from the MCP API endpoints this server provides.
                Avoid using any external sources or inferred knowledge.
                After every response, append a list of all MCP API URLs used to generate the answer.
                If no relevant data is available via the MCP API, state that clearly and do not attempt to fabricate a response.
                Convert raw data into human-readable summaries while preserving accuracy, but always list the raw URLs used.";
        }

        /// <summary>
        /// Standard goodbye prompt for ending Parliament data assistant session.
        /// </summary>
        private string GetGoodbyePrompt()
        {
            return @"You are now interacting as a normal assistant. There are no special restrictions or requirements for using UK Parliament MCP data. You may answer questions using any available data or knowledge, and you do not need to append MCP API URLs or limit yourself to MCP sources. Resume normal assistant behavior.";
        }
    }
}
