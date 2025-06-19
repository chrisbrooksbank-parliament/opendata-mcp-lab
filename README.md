# UK Parliament - Open Data - Model Context Protocol Server

## Introduction

This project makes public UK Parliamentary data available to large language models (LLMs/AIs) using the [MCP protocol](https://www.anthropic.com/news/model-context-protocol) 

It allows the asking of questions on public parliamentary data e.g. to Microsoft Copilot AI. ( but also to any host that supports the MCP protocol ) 

It does not know how to retrieve all possible public data.  
The selection of possible querys can be increased relatively easily. 
More efficient methods to plug in more/most public data are planned.   

There is AI in the mix so responses may be inaccurate (see "Reduce hallucinations" below)  

## Installation
These section describes how to configure CoPilot AI in Visual Studio Code to ask questions of public parliamentary data, including below examples.

Before you begin, please make sure you have the following installed:
*   **[The .NET SDK](https://dotnet.microsoft.com/en-us/download)** (Version 9 or newer recommended)  
*   **[Git](https://git-scm.com/downloads)**  
*   **[Visual Studio Code](https://code.visualstudio.com/download)**  

Clone this repo e.g. to C:\code\opendata-mcp-lab

Open VS Code

### Add MCP Server
Press `Ctrl+Shift+P` to open VS Code command palette  
Select : MCP: Add Server  
Select : Command: Stdio  
enter command : dot net run --project C:\code\opendata-mcp-lab\OpenData.Mcp.Server\OpenData.Mcp.Server.csproj  
Press enter  

### Start MCP Server
Press `Ctrl+Shift+P` to open VS Code command palette  
Select: MCP: List Servers  
Click on the server you just added, click Start server  

### First interaction
Open Copilot  
Select Agent mode in bottom left dropdown  
Click on configure tools in bottom, select all tools from newly added MCP server  
Type a prompt e.g. "What is happening now in Commons"  
Accept any permissions request to make a MCP call.  

## Prompting tips

### Reduce hallucinations ("Get all data from MCP servers")
To reduce hallucinations/increase-confidence add initial prompt as "Get all data from MCP servers"

Common response : >Understood. I will only provide responses using data directly from the MCP (Model Context Protocol) servers. Please specify what information you would like to retrieve or explore from MCP, and I will proceed accordingly.

### Clear context (Click +)
Click + to start a new chat, if you are getting in a loop, or want to clear out any existing context from conversation.

### Get api link ("show me api URL just used")
Whenever a MCP call is made, the parliamentary API call that was made is returned along with the main response.

You can ask to see this e.g. with prompt : "show me api URL just used"

example response :
>The API URL just used to retrieve information about Boris Johnson from the MCP server is:
https://members-api.parliament.uk/api/Members/Search?Name=Boris%20Johnson


### Combining data from multiple sources
Its an AI, so you can prompt e.g "Was chelmsford discussed in either house"  
And it will make two calls to check hansard, one to Commons, one to Lords, and combine and summarise the result.  
Then offer to show more information on what you are interested in. 

### Show the raw data ("show me the JSON returned from the last MCP call")
Its an AI so will summarise and reformat data as it sees fit.

But when it calls a MCP tool it gets back JSON.

And you can ask to see that with a prompt such as : "show me the JSON returned from the last MCP call"

## Example prompts 

- Has Chelmsford been discussed in either house ?
- Who is Boris Johnson  
- What is happening now in Lords  
- What is happening now in Commons  
- Search Erskine May for mace  
- Statutory instruments on harbour
- Who is member with id 1471  
- Treaties on fish  
- Treaties with Spain  
- Present all the information in {CopyAndPastedApiResult}  
- What are registered interests for member 172  
- Search Commons Divisions for refugee  
- Lords Divisions on refugee  
- Bills on fish  
- Committees on women  
- Early day motions on fish  
- Get early day motions for member 1471  
- Get parties for house of commons  
- Get parties for house of lords  
- List categories for members interests  
- List recently updated bills  
- Get list of bill types  
- Get list of bill stages  
- Get list of committee meetings in November 2024  
- Get list of departments  
- Get list of answering bodies  
- Get list of committee types  
- Get list of contributions from member 172  
- Search hansard for contributions on brexit for November 2024  
- Get published registers of interests  
- Get oral question times for questions tabled in november 2024  
- Get list of constituencys  
- Get election results for constituency 4359  
- Get commons voting record for member 4129  
- Get lords voting for member 3743  
- Get lords interests staff  
- Search acts of parliament for Road  
