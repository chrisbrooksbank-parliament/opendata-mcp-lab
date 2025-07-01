# UK Parliament AI Assistant

This project helps Artificial Intelligence (AI) assistants, like Microsoft Copilot, answer questions using official data from the UK Parliament. It acts as a bridge, allowing the AI to access up-to-date, reliable information directly from the source.

## Starting ( system ) prompt
I suggest beginning a new conversation and entering the following system prompt before asking questions on Parliament.

```plaintext
You are a helpful assistant that answers questions using only data from UK Parliament MCP servers.

When the session begins, introduce yourself with a brief message such as:

"Hello! I’m a parliamentary data assistant. I can help answer questions using official data from the UK Parliament MCP APIs. Just ask me something, and I’ll fetch what I can — and I’ll always show you which sources I used."

When responding to user queries, you must:

Only retrieve and use data from the MCP API endpoints this server provides.

Avoid using any external sources or inferred knowledge.

After every response, append a list of all MCP API URLs used to generate the answer.

If no relevant data is available via the MCP API, state that clearly and do not attempt to fabricate a response.

Convert raw data into human-readable summaries while preserving accuracy, but always list the raw URLs used.
```

## What Can I Ask?

You can ask questions about many aspects of Parliament. Here are a few examples:

*   **Current Events:** "What's happening in the House of Commons right now?"
*   **Members of Parliament (MPs):** "Tell me everything you know about Boris Johnson." or "What are Sir Keir Starmers registered interests?"
*   **Legislation:** "Are there any recent bills about environmental protection?"
*   **Debates:** "What has been said about healthcare in the House of Lords this week?"
*   **Committees:** "Which committees are looking into economic issues?"

## How Does It Work?

When you ask a question in an AI assistant that is connected to this tool, the AI doesn't just guess the answer. Instead, it uses this project to look up the information in the official UK Parliament database and gives you a response based on the real data it finds.

This means the answers you get are more likely to be accurate and based on facts.

---

## For Developers: Technical Details

This project makes public UK Parliamentary data accessible to large language models (LLMs/AIs) using the [Model Context Protocol (MCP)](https://www.anthropic.com/news/model-context-protocol).

It enables AI tools (e.g. Microsoft Copilot) to answer questions about UK Parliamentary data, as long as they support the MCP protocol.

> ⚠️ This project does **not** expose all possible public parliamentary data — yet.
> Support for more queries is planned and relatively easy to expand.

Since AI is involved, some responses may be inaccurate. See **Prompting Tips** below to improve reliability.

### Installation & Setup

This section explains how to configure Microsoft Copilot in Visual Studio Code to query UK Parliamentary data via the MCP server.

#### Prerequisites

Make sure you have the following installed:

- [.NET SDK](https://dotnet.microsoft.com/en-us/download) (v9 or later recommended)
- [Git](https://git-scm.com/downloads)
- [Visual Studio Code](https://code.visualstudio.com/download)

#### Clone and Open the Project

```bash
git clone https://github.com/chrisbrooksbank-parliament/opendata-mcp-lab.git
cd opendata-mcp-lab
```

Or download manually and open the folder in VS Code.

#### Add MCP Server in VS Code

1.  Press `Ctrl+Shift+P` to open the Command Palette.
2.  Select **MCP: Add Server**.
3.  Choose **Command: Stdio**.
4.  Enter the following command (adjust path if needed):

```bash
dotnet run --project C:\code\opendata-mcp-lab\OpenData.Mcp.Server\OpenData.Mcp.Server.csproj
```

5.  Press **Enter**.

#### Start the Server

1.  Press `Ctrl+Shift+P` again.
2.  Select **MCP: List Servers**.
3.  Click the server you just added and choose **Start server**.

#### First Interaction

1.  Open **Copilot Chat** in VS Code.
2.  Set **Agent mode** using the dropdown in the bottom-left.
3.  Select your prefferred model e.g. Claude Sonnet 4
4.  Click **Configure Tools**, and select all tools from the newly added MCP server.
5.  (enter the system prompt and then) Try a prompt such as:

```plaintext
What is happening now in the House of Commons?
```

5.  Accept any permission request to allow the MCP call.

### Prompting Tips

#### ✅ System Prompt

Start with a system prompt like:

```plaintext
You are a helpful assistant that answers questions using only data from UK Parliament MCP servers.

When the session begins, introduce yourself with a brief message such as:

"Hello! I’m a parliamentary data assistant. I can help answer questions using official data from the UK Parliament MCP APIs. Just ask me something, and I’ll fetch what I can — and I’ll always show you which sources I used."

When responding to user queries, you must:

Only retrieve and use data from the MCP API endpoints this server provides.

Avoid using any external sources or inferred knowledge.

After every response, append a list of all MCP API URLs used to generate the answer.

If no relevant data is available via the MCP API, state that clearly and do not attempt to fabricate a response.

Convert raw data into human-readable summaries while preserving accuracy, but always list the raw URLs used.
```

#### 🔄 Clear Context

Use the `+` icon (new chat) if:
- The AI seems stuck in a loop
- You want to reset the conversation context

#### 🔗 See the API URL Used

You can ask:

```plaintext
Show me the API URL just used
```

Example response:
> The API URL just used to retrieve information about Boris Johnson is:
> `https://members-api.parliament.uk/api/Members/Search?Name=Boris%20Johnson`

#### 🧠 Combine Data from Multiple Sources

Example:
```plaintext
Has Chelmsford been mentioned in either the Commons or Lords?
```

The AI may:
- Query both Commons and Lords Hansard
- Combine the results
- Offer more detail if requested

#### 🧾 See the Raw JSON

Example:
```plaintext
Show me the JSON returned from the last MCP call
```

Useful for debugging or inspecting the raw structure.

### Example Prompts

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