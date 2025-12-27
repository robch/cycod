# Mock Output Examples - Proposed cycodj Design

## Example 1: cycodj list --last 3
```
## Chat History Conversations

Showing last 3 conversations

2025-12-22 14:15 - Date/Time Support Implementation
  Messages: 418 (125 user, 167 assistant, 126 tool)
  
  > lets go!
  > why does my git status (or better stated, my git lens in vs code) still show...
  > ok... now... i'm not sure i understand really the key differences between...

2025-12-22 08:31 - Weekend Activity Journal Review
  Messages: 89 (23 user, 35 assistant, 31 tool)
  
  > wow... very cool... can you do the same for all of last week thru now?
  > i want more details for each day ... helping show what conversational branches...
  > what rebos was i working on? what were the 'features' and 'approaches'...

2025-12-21 21:09 - Standardize cycodj Help Documentation
  Messages: 58 (15 user, 25 assistant, 18 tool)
  
  > can you help me standardize the help documentation
  > make it consistent with cycod, cycodmd, cycodgr
  > use the same format and style
```

---

## Example 2: cycodj list --last 3 --messages=1
```
## Chat History Conversations

Showing last 3 conversations

2025-12-22 14:15 - Date/Time Support Implementation
  Messages: 418 (125 user, 167 assistant, 126 tool)
  
  > lets go!

2025-12-22 08:31 - Weekend Activity Journal Review
  Messages: 89 (23 user, 35 assistant, 31 tool)
  
  > wow... very cool... can you do the same for all of last week thru now?

2025-12-21 21:09 - Standardize cycodj Help Documentation
  Messages: 58 (15 user, 25 assistant, 18 tool)
  
  > can you help me standardize the help documentation
```

---

## Example 3: cycodj list --last 3 --branches
```
## Chat History Conversations

Showing last 3 conversations

2025-12-22 14:15 - Date/Time Support Implementation
  Messages: 418 (125 user, 167 assistant, 126 tool)
  
  > lets go!
  > why does my git status (or better stated, my git lens in vs code) still show...
  > ok... now... i'm not sure i understand really the key differences between...

2025-12-22 08:31 - Weekend Activity Journal Review
  Messages: 89 (23 user, 35 assistant, 31 tool)
  
  > wow... very cool... can you do the same for all of last week thru now?
  > i want more details for each day ... helping show what conversational branches...
  > what rebos was i working on? what were the 'features' and 'approaches'...

  ↳ 2025-12-22 14:15 - Date/Time Support Implementation
    Messages: 418 (125 user, 167 assistant, 126 tool)
    Branch: Continued implementation work
    
    > lets go!
    > why does my git status (or better stated, my git lens in vs code) still show...
    > ok... now... i'm not sure i understand really the key differences between...

2025-12-21 21:09 - Standardize cycodj Help Documentation
  Messages: 58 (15 user, 25 assistant, 18 tool)
  
  > can you help me standardize the help documentation
  > make it consistent with cycod, cycodmd, cycodgr
  > use the same format and style
```

---

## Example 4: cycodj branches --yesterday
```
## Conversation Branches

Filtered: December 21, 2025 (240 conversations)

📁 09:45 - CDR Project Restart Instructions
   Messages: 45 (12 user, 20 assistant, 13 tool)
   
  ├─ 09:46 - CDR Project Iteration 1
  │  Messages: 32 (8 user, 15 assistant, 9 tool)
  │
  ├─ 09:58 - CDR Project Iteration 2
  │  Messages: 28 (7 user, 12 assistant, 9 tool)
  │
  └─ 10:01 - CDR Project Iteration 3
     Messages: 35 (9 user, 14 assistant, 12 tool)
     
     ├─ 10:06 - CDR Project Iteration 4
     │  Messages: 30 (8 user, 13 assistant, 9 tool)
     │
     └─ 10:21 - CDR Project Iteration 5
        Messages: 42 (10 user, 18 assistant, 14 tool)

📁 21:05 - Change Git Remote Repository
   Messages: 26 (7 user, 11 assistant, 8 tool)

📁 21:09 - Standardize cycodj Help Documentation
   Messages: 58 (15 user, 25 assistant, 18 tool)
```

---

## Example 5: cycodj branches --yesterday --messages
```
## Conversation Branches

Filtered: December 21, 2025 (240 conversations)

📁 09:45 - CDR Project Restart Instructions
   Messages: 45 (12 user, 20 assistant, 13 tool)
   
   > Read cdr/STATUS.md and begin your work
   > Create the documentation structure
   > Start with phase 1
   
  ├─ 09:46 - CDR Project Iteration 1
  │  Messages: 32 (8 user, 15 assistant, 9 tool)
  │  
  │  > Continue from where we left off
  │  > Read STATUS.md and do next step
  │  > Update the progress tracking
  │
  ├─ 09:58 - CDR Project Iteration 2
  │  Messages: 28 (7 user, 12 assistant, 9 tool)
  │  
  │  > Read attempt1/STATUS.md and do the next step
  │  > Continue with phase 2
  │  > Document the findings
  │
  └─ 10:01 - CDR Project Iteration 3
     Messages: 35 (9 user, 14 assistant, 12 tool)
     
     > Read STATUS.md and proceed
     > Implement the next phase
     > Update documentation
```

---

## Example 6: cycodj search "bug" --last 30d
```
## Search Results: "bug"

Searched last 30 days (318 conversations)
Found 15 matches in 8 conversations

───────────────────────────────────────

2025-12-20 06:45 - Implement Cycodgr AI Task
  Messages: 359 (95 user, 145 assistant, 119 tool)
  Matches: 3
  
  > implement the cycodgr AI task from the TODO
  > there's a bug in the line number handling
  > let's fix the GitHub search bug

───────────────────────────────────────

2025-12-19 15:22 - Fix Release Pipeline
  Messages: 42 (12 user, 18 assistant, 12 tool)
  Matches: 2
  
  > the release pipeline has a bug
  > fix the NuGet naming bug
  > test the fix
```

---

## Example 7: cycodj search "bug" --last 30d --messages=1
```
## Search Results: "bug"

Searched last 30 days (318 conversations)
Found 15 matches in 8 conversations

───────────────────────────────────────

2025-12-20 06:45 - Implement Cycodgr AI Task
  Messages: 359 (95 user, 145 assistant, 119 tool)
  Matches: 3
  
  > implement the cycodgr AI task from the TODO

───────────────────────────────────────

2025-12-19 15:22 - Fix Release Pipeline
  Messages: 42 (12 user, 18 assistant, 12 tool)
  Matches: 2
  
  > the release pipeline has a bug
```

---

## Example 8: cycodj list --last 7d --stats
```
## Chat History Statistics

Date Range: December 15-22, 2025 (7 days)

Overall Summary:
  Total conversations: 467
  Total messages: 58,245
    User messages: 6,234 (10.7%)
    Assistant messages: 28,156 (48.4%)
    Tool messages: 23,855 (40.9%)
  
  Average messages per conversation: 125
  
  Conversations with branches: 95 (20.3%)
  Total branches: 123
  
Per-Date Breakdown:
  2025-12-22 (Sun): 15 conversations, 1,245 messages, 3 branches
  2025-12-21 (Sat): 240 conversations, 18,523 messages, 15 branches
  2025-12-20 (Fri): 21 conversations, 5,234 messages, 12 branches
  2025-12-19 (Thu): 95 conversations, 12,456 messages, 45 branches
  2025-12-18 (Wed): 37 conversations, 8,234 messages, 18 branches
  2025-12-17 (Tue): 27 conversations, 4,523 messages, 15 branches
  2025-12-16 (Mon): 32 conversations, 8,030 messages, 15 branches
  
Most active day: Saturday, December 21 (240 conversations)
Most branches: Thursday, December 19 (45 branches)
```

---

## Key Differences Summary:

**Default behavior:**
- `list` → 3 messages per conversation
- `branches` → 0 messages (structure only)
- `search` → 3 messages per conversation
- `show` → ALL messages

**With --messages=N:**
- `--messages=1` → Minimal preview
- `--messages=5` → More context
- `--messages=all` → Everything (like current export)

**With --branches:**
- Shows `↳` indicators
- Indents child conversations
- Shows branch relationships inline

**With --stats:**
- Shows numbers instead of conversations
- Message counts, branch counts, trends
- Per-date breakdown

Does this look right? 😊
