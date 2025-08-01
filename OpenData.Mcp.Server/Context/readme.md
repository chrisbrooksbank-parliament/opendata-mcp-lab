# UK Parliament API Context for LLM

This directory provides structured context about UK Parliament OpenData APIs for large language models working with the MCP server.

## Quick Reference for LLMs

**Key Concepts:**
- House numbers: 1=Commons, 2=Lords
- All dates use YYYY-MM-DD format
- Member IDs are unique across both houses
- Most endpoints support pagination (skip/take parameters)

## API Coverage & Capabilities

| API Category | Base URL | Key Use Cases | Common Query Patterns |
|--------------|----------|---------------|----------------------|
| **Members** | https://members-api.parliament.uk | Find MPs/Lords by name, constituency, party | "Who is the MP for [constituency]?", "What committees is [member] on?" |
| **Bills** | https://bills-api.parliament.uk | Track legislation progress, find bills by topic | "What bills mention [topic]?", "What stage is [bill] at?" |
| **Votes** | https://commonsvotes-api.parliament.uk<br/>https://lordsvotes-api.parliament.uk | Analyze voting patterns, find division results | "How did [MP] vote on [bill]?", "What were recent close votes?" |
| **Committees** | https://committees-api.parliament.uk | Committee membership, hearings, evidence | "What committees cover [topic]?", "Who gave evidence on [subject]?" |
| **Hansard** | https://hansard-api.parliament.uk | Official parliamentary record, speeches | "What did [MP] say about [topic]?", "Find debates on [subject]" |
| **Questions** | https://oralquestionsandmotions-api.parliament.uk<br/>https://questions-statements-api.parliament.uk | PMQs, written questions, ministerial statements | "Questions about [topic]", "What has [minister] been asked?" |
| **Live Data** | https://now-api.parliament.uk<br/>https://whatson-api.parliament.uk | Current chamber activity, upcoming events | "What's happening in Parliament now?", "What's scheduled this week?" |

## Data Relationships for LLMs

```
Member (MP/Lord) 
├── Has constituencies (MPs only)
├── Belongs to parties
├── Serves on committees
├── Votes in divisions
├── Asks questions
├── Makes speeches (Hansard)
└── Declares interests

Bill
├── Has stages/progress
├── Generates divisions (votes)
├── Creates committee scrutiny
├── Produces Hansard debates
└── May become Acts

Committee
├── Has members
├── Holds evidence sessions  
├── Produces reports
├── Covers specific policy areas
└── Scrutinizes bills
```

## LLM Optimization Notes

- **Start broad, then narrow**: Use search endpoints before specific ID lookups
- **Combine data sources**: Link members → votes → bills → committees for rich analysis
- **Handle pagination**: Many endpoints return 20 items by default, max 100
- **Date ranges**: Most endpoints support from/to date filtering
- **Search is powerful**: Most text fields support partial matching
- **Cache member lookups**: Member IDs are stable, names/constituencies may change

## Common LLM Query Patterns

1. **Member Analysis**: Get member → check votes → find committee work → review questions
2. **Bill Tracking**: Search bills → check stages → find votes → review debates
3. **Topic Research**: Search across Hansard + Questions + Committee evidence
4. **Temporal Analysis**: Use date filters to track changes over time
5. **Cross-reference**: Link votes to bill stages to understand legislative process