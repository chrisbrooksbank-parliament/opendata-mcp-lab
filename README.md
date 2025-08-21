# UK Parliament AI Assistant

This project helps Artificial Intelligence (AI) assistants, like Microsoft Copilot, answer questions using comprehensive official data from the UK Parliament. It acts as a bridge, allowing the AI to access up-to-date, reliable information directly from the source, covering members, bills, voting records, committees, debates, procedures, and much more.

## 🔥 ESSENTIAL: System Prompt (Required for Best Results!)

**IMPORTANT**: You MUST use the starting system prompt when starting a new conversation. This is critical for accurate responses.

The easiest way to do this is to start all new conversations with the prompt  "Hello Parliament".

Or you can copy and past the following instead : 

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

## Improved Prompt - This more closely uses best practice in prompt engineering ( untested as yet )

```
You are acting as an AI parliamentary data assistant created to provide answers only from the official UK Parliament MCP servers.

You should maintain a clear, professional, and neutral tone.

Here is the background data you should reference when answering the user: <guide>{{DOCUMENTS}}</guide>

Here are the important rules for the interaction:
- Always stay in character as a UK Parliament MCP assistant.
- At the start of a session, introduce yourself with:
  "Hello! I’m a parliamentary data assistant. I can help answer questions using official data from the UK Parliament MCP APIs. Just ask me something, and I’ll fetch what I can — and I’ll always show you which sources I used."
- Only retrieve and use data from the MCP API endpoints this server provides.
- Do not use external sources or inferred knowledge.
- After every response, append a list of all MCP API URLs used to generate the answer.
- If no relevant data is available via the MCP API, state clearly: "No relevant data is available from the MCP APIs for this query."
- Convert raw MCP data into human-readable summaries while preserving accuracy.

Here is an example of how to respond in a standard interaction:
<example>
User: Who is the current MP for Chelmsford?
Assistant: The MCP API shows that the current MP for Chelmsford is Vicky Ford (Conservative).
Sources:
- https://mcp.parliament.uk/members/12345
</example>

<example>
User: What debates happened yesterday?
Assistant: Yesterday’s debates included:
- “Energy Security Bill – Second Reading”
- “Health and Social Care Questions”
Sources:
- https://mcp.parliament.uk/debates?date=2025-08-20
</example>

Here is the conversation history (between the user and you) prior to the question. It could be empty if there is no history:
<history>{{HISTORY}}</history>

Here is the user’s question:
<question>{{QUESTION}}</question>

Think carefully about your answer step by step before you respond:
- Identify which MCP API endpoints provide the answer.
- Retrieve and summarise that data in plain English.
- Always include the list of exact URLs used.

Put your final response between <response></response> tags.

<response>
Hello! I’m a parliamentary data assistant. I can help answer questions using official data from the UK Parliament MCP APIs. Just ask me something, and I’ll fetch what I can — and I’ll always show you which sources I used.
</response>
```

## What Can I Ask?

You can ask questions about virtually all aspects of UK Parliament data. Here are some key areas:

*   **Live Parliamentary Activity:** "What's happening in the House of Commons right now?" or "What's currently being debated in the Lords?"
*   **Members of Parliament:** "Tell me everything you know about Boris Johnson," "What are Sir Keir Starmer's registered interests?" or "Show me the voting record of member 4129"
*   **Bills & Legislation:** "Show me details of bill 425," "What amendments were proposed for bill 425?" or "What publications exist for the Environment Bill?"
*   **Voting Records:** "How did MPs vote on the climate change motion?" or "Show me Lords divisions on healthcare policy"
*   **Committees & Inquiries:** "Which committees are investigating economic issues?" or "Show me evidence submitted to the Treasury Committee"
*   **Parliamentary Procedures:** "Search Erskine May for references to Speaker's rulings" or "What are the oral question times this week?"
*   **Constituencies & Elections:** "Show me election results for Birmingham constituencies" or "List all constituencies in Scotland"
*   **Official Documents:** "Are there any statutory instruments about housing?" or "What treaties involve trade agreements?"
*   **Transparency Data:** "Show register of interests for Treasury ministers" or "What are the declared interests categories?"

## Disconnecting from parliament

Start a new chat session, or to keep context but disconnect from Parliament MCP servers prompt : "Goodbye Parliament"

## How Does It Work?

When you ask a question to an AI assistant connected to this tool, it doesn't just guess the answer. Instead, the assistant uses this project to query the official UK Parliament database and provides a response based on the data it finds.

This means the answers you get are more likely to be accurate and based on facts.

---

## Installation and Usage Guide

This project makes public UK Parliamentary data accessible to large language models (LLMs/AIs) using the [Model Context Protocol (MCP)](https://www.anthropic.com/news/model-context-protocol).

It enables AI tools (e.g. Microsoft Copilot) to answer questions about UK Parliamentary data, as long as they support the MCP protocol.

> ✅ This project provides comprehensive coverage of UK Parliamentary data including members, bills, amendments, voting records, committees, debates, procedures, constituencies, and official documents.
> Additional data sources and endpoints are continuously being added.

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

#### Add MCP Server in Claude Desktop Application  

* Open the claude desktop application  
* Click settings   
* Click Developer tag  
* Click Edit Config  
* Edit file and save with UTF-8 encoding  
* Exit claude ( with TaskMon if needed ) and restart it  
* Open developer tab again and check it is running  
* Enter the system prompt and test its working ok  

Example claude_desktop_config.json file contents : 

```json
{
  "mcpServers": {
    "opendata-server": {
      "command": "dotnet",
      "args": [
        "run",
        "--project",
        "C:\\code\\opendata-mcp-lab\\OpenData.Mcp.Server\\OpenData.Mcp.Server.csproj"
      ]
    }
  }
}
```





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

6.  Accept any permission request to allow the MCP call.

### Prompting Tips

#### ✅ System Prompt

Always begin your session with the **System Prompt** defined at the top of this guide. This is the most crucial step for ensuring the AI assistant stays on track, uses only the provided data, and cites its sources correctly.

#### 🔄 Clear Context

Use the `+` icon (new chat) if:
- The AI seems stuck in a loop
- You want to reset the conversation context

#### 🔗 Re-Display the API URL

While the AI is instructed to list source URLs automatically, you can ask for them again at any time. This is useful for troubleshooting or if you simply want to re-confirm the source for the last response.

You can ask : 

```plaintext
Show me the API URL you just used.
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

For debugging or to inspect the raw data structure, you can ask the assistant to show you the full JSON response from its last API call. This is particularly useful for developers who want to understand exactly what information the AI is working with before it is summarized.

Example prompt:
```plaintext
Show me the JSON returned from the last MCP call.
```

### Example Prompts

#### 🏛️ Live Parliamentary Activity
- What is happening now in both Houses?
- What's currently happening in the House of Commons?
- What's currently happening in the House of Lords?

#### 👥 Members of Parliament
- Show me the interests of Sir Keir Starmer
- Who is Boris Johnson?
- Who is the member with ID 1471?
- Get the biography of member 172
- Show me contact details for member 4129
- What are the registered interests of member 3743?
- Show recent contributions from member 172
- What is the Commons voting record for member 4129?
- What is the Lords voting record for member 3743?
- Show the professional experience of member 1471
- What policy areas does member 172 focus on?
- Show early day motions submitted by member 1471
- Get the constituency election results for member 4129
- Show me the portrait and thumbnail images for member 172

#### 📜 Bills and Legislation
- What recent bills are about fishing?
- What bills were updated recently?
- Show me details of bill 425
- What stages has bill 425 been through?
- What amendments were proposed for bill 425 at stage 15?
- Show me amendment 1234 for bill 425 stage 15
- What publications exist for bill 425?
- What news articles are there about bill 425?
- Show me all bill types available
- What are the different stages a bill can go through?
- Search for bills containing the word "environment"
- Get the RSS feed for all bills
- Get the RSS feed for public bills only
- Get the RSS feed for bill 425

#### 🗳️ Voting and Divisions
- Search Commons Divisions for the keyword "refugee"
- Show details of Commons division 1234
- Show details of Lords division 5678
- Get Commons divisions grouped by party for keyword "climate"
- Get Lords divisions grouped by party for member 3743
- How many divisions match the search term "brexit"?

#### 🏢 Committees and Inquiries
- Which committees are focused on women's issues?
- List committee meetings scheduled for November 2024
- Show me details of committee 789
- What events has committee 789 held?
- Who are the members of committee 789?
- Search for committee publications about healthcare
- Show me written evidence submitted to committee 789
- Show me oral evidence from committee 789 hearings
- What are all the committee types?

#### 🏛️ Parliamentary Procedures
- Search Erskine May for references to the Mace
- Show oral question times for questions tabled in November 2024
- Search Hansard for contributions on Brexit from November 2024
- What government departments exist?
- What are the answering bodies in Parliament?
- What parties are represented in the House of Commons?
- What parties are represented in the House of Lords?
- Show parliamentary calendar events for Commons in December 2024
- When is Parliament not sitting in January 2025?

#### 📍 Constituencies and Elections
- List all UK constituencies
- Show the election results for constituency 4359
- Search for constituencies containing "london"

#### 💰 Transparency and Interests
- List all categories of members' interests
- Get published registers of interests
- Show staff interests for Lords members
- Search the register of interests for member 1471

#### 📋 Official Documents and Publications
- Are there any statutory instruments about harbours?
- Search Acts of Parliament that mention roads
- What treaties involve Spain?
- What publication types are available for bills?
- Show me document 123 from publication 456

#### 🔍 Advanced Queries
- Show the full data from this pasted API result: {PasteApiResultHere}
- Show me the JSON returned from the last MCP call
- Show me the API URL you just used
- Search for bills sponsored by member 172 from the Environment department
- Find all committee meetings about climate change between November and December 2024

---

## Final Thoughts

The project is under active development, with plans to increase data coverage and improve interaction quality. Contributions and feedback are welcome.
