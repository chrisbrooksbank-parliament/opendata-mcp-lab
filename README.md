# UK Parliament Open Data - Model Context Protocol Server

## Introduction

This project makes public UK Parliamentary data accessible to large language models (LLMs/AIs) using the [Model Context Protocol (MCP)](https://www.anthropic.com/news/model-context-protocol).

It enables AI tools (e.g. Microsoft Copilot) to answer questions about UK Parliamentary data, as long as they support the MCP protocol.

> ⚠️ This project does **not** expose all possible public parliamentary data — yet.  
Support for more queries is planned and relatively easy to expand.

Since AI is involved, some responses may be inaccurate. See **Prompting Tips** below to improve reliability.

---

## Installation & Setup

This section explains how to configure Microsoft Copilot in Visual Studio Code to query UK Parliamentary data via the MCP server.

### Prerequisites

Make sure you have the following installed:

- [.NET SDK](https://dotnet.microsoft.com/en-us/download) (v9 or later recommended)  
- [Git](https://git-scm.com/downloads)  
- [Visual Studio Code](https://code.visualstudio.com/download)  

---

### Clone and Open the Project

```bash
git clone https://github.com/chrisbrooksbank-parliament/opendata-mcp-lab.git
cd opendata-mcp-lab
```

Or download manually and open the folder in VS Code.

---

### Add MCP Server in VS Code

1. Press `Ctrl+Shift+P` to open the Command Palette.  
2. Select **MCP: Add Server**.  
3. Choose **Command: Stdio**.  
4. Enter the following command (adjust path if needed):

```bash
dotnet run --project C:\code\opendata-mcp-lab\OpenData.Mcp.Server\OpenData.Mcp.Server.csproj
```

5. Press **Enter**.

---

### Start the Server

1. Press `Ctrl+Shift+P` again.  
2. Select **MCP: List Servers**.  
3. Click the server you just added and choose **Start server**.

---

### First Interaction

1. Open **Copilot Chat** in VS Code.  
2. Set **Agent mode** using the dropdown in the bottom-left.  
3. Click **Configure Tools**, and select all tools from the newly added MCP server.  
4. Try a prompt like:

```plaintext
What is happening now in the House of Commons?
```

5. Accept any permission request to allow the MCP call.

---

## Prompting Tips

### ✅ Reduce Hallucinations

Start with a system prompt like:

```plaintext
Get all data from MCP servers
```

This encourages the AI to avoid guessing and only use actual MCP data.

Typical response:
> Understood. I will only provide responses using data directly from the MCP servers.

---

### 🔄 Clear Context

Use the `+` icon (new chat) if:
- The AI seems stuck in a loop
- You want to reset the conversation context

---

### 🔗 See the API URL Used

You can ask:

```plaintext
Show me the API URL just used
```

Example response:
> The API URL just used to retrieve information about Boris Johnson is:  
> `https://members-api.parliament.uk/api/Members/Search?Name=Boris%20Johnson`

---

### 🧠 Combine Data from Multiple Sources

Example:
```plaintext
Has Chelmsford been mentioned in either the Commons or Lords?
```

The AI may:
- Query both Commons and Lords Hansard
- Combine the results
- Offer more detail if requested

---

### 🧾 See the Raw JSON

Example:
```plaintext
Show me the JSON returned from the last MCP call
```

Useful for debugging or inspecting the raw structure.

---

## Example Prompts

- What is happening now in both houses
- show me interests of Sir Keir Starmer
- Who is Boris Johnson?
- Search Erskine May for references to the mace.
- Are there any statutory instruments about harbours?
- Who is the member with ID 1471?
- What treaties involve Spain?
- Show the full data from this pasted API result: {PasteApiResultHere}
- Search Commons Divisions for the keyword "refugee"
- What recent bills are about fishing?
- Which committees are focused on women’s issues?
- Show early day motions submitted by member 1471
- What parties are represented in the House of Commons?
- List all categories of members' interests
- What bills were updated recently?
- Show all bill types
- List committee meetings scheduled for November 2024
- What government departments exist?
- What are the answering bodies in Parliament?
- List all committee types
- Show recent contributions from member 172
- Search Hansard for contributions on Brexit from November 2024
- Get published registers of interests
- Show oral question times for questions tabled in November 2024
- List all UK constituencies
- Show the election results for constituency 4359
- What is the Commons voting record for member 4129?
- What is the Lords voting record for member 3743?
- Show staff interests for Lords members
- Search Acts of Parliament that mention roads

---

## Final Thoughts

The project is under active development, with plans to increase data coverage and improve interaction quality. Contributions and feedback are welcome.
