# Conversation Branching - Visual Examples

## Example 1: Simple Branch

```
Conversation Timeline:
======================

10:00 AM - User starts conversation
    ↓
    "How do I install Node.js?"
    ↓
    AI: "I'll help you install Node.js..."
    TOOL: RunPowershellCommand (id: toolu_001)
    ↓
    "Check if it's installed"
    ↓
    TOOL: RunPowershellCommand (id: toolu_002)
    ↓
    [BRANCH POINT - User wants to try different approach]
    
    ↙                           ↘
Branch A (10:15 AM)         Branch B (10:18 AM)
"Use nvm instead"           "Install using chocolatey"
    ↓                           ↓
TOOL: toolu_003             TOOL: toolu_005
    ↓                           ↓
SAVED AS:                   SAVED AS:
chat-history-1754437766.jsonl   chat-history-1754437862.jsonl

Tool Call IDs:              Tool Call IDs:
- toolu_001 ✓               - toolu_001 ✓
- toolu_002 ✓               - toolu_002 ✓
- toolu_003                 - toolu_005
```

## Example 2: Multi-Level Branching

```
Original Conversation (chat-history-1754437373.jsonl)
├─ toolu_001: Check git installation
├─ toolu_002: Install git
└─ toolu_003: Verify installation
    │
    ├── Branch A (chat-history-1754437766.jsonl)
    │   ├─ toolu_001 ✓ (shared)
    │   ├─ toolu_002 ✓ (shared)
    │   ├─ toolu_003 ✓ (shared)
    │   ├─ toolu_004: Try different approach
    │   └─ toolu_005: Configure settings
    │       │
    │       └── Branch A1 (chat-history-1754437999.jsonl)
    │           ├─ toolu_001 ✓ (shared)
    │           ├─ toolu_002 ✓ (shared)
    │           ├─ toolu_003 ✓ (shared)
    │           ├─ toolu_004 ✓ (shared)
    │           ├─ toolu_005 ✓ (shared)
    │           └─ toolu_006: Alternative config
    │
    └── Branch B (chat-history-1754437862.jsonl)
        ├─ toolu_001 ✓ (shared)
        ├─ toolu_002 ✓ (shared)
        ├─ toolu_003 ✓ (shared)
        └─ toolu_007: Completely different approach
```

## JSONL File Content Comparison

### Parent File (chat-history-1754437373.jsonl)
```jsonl
{"role":"system","content":"..."}
{"role":"user","content":"install git"}
{"role":"assistant","tool_calls":[{"id":"toolu_001",...}]}
{"role":"tool","tool_call_id":"toolu_001","content":"..."}
{"role":"assistant","tool_calls":[{"id":"toolu_002",...}]}
{"role":"tool","tool_call_id":"toolu_002","content":"..."}
{"role":"assistant","tool_calls":[{"id":"toolu_003",...}]}
{"role":"tool","tool_call_id":"toolu_003","content":"..."}
```

### Branch A (chat-history-1754437766.jsonl)
```jsonl
{"role":"system","content":"..."}
{"role":"user","content":"install git"}
{"role":"assistant","tool_calls":[{"id":"toolu_001",...}]}  ← SAME
{"role":"tool","tool_call_id":"toolu_001","content":"..."}  ← SAME
{"role":"assistant","tool_calls":[{"id":"toolu_002",...}]}  ← SAME
{"role":"tool","tool_call_id":"toolu_002","content":"..."}  ← SAME
{"role":"assistant","tool_calls":[{"id":"toolu_003",...}]}  ← SAME
{"role":"tool","tool_call_id":"toolu_003","content":"..."}  ← SAME
{"role":"user","content":"try using nvm instead"}          ← NEW
{"role":"assistant","tool_calls":[{"id":"toolu_004",...}]} ← NEW
{"role":"tool","tool_call_id":"toolu_004","content":"..."}  ← NEW
```

### Branch B (chat-history-1754437862.jsonl)
```jsonl
{"role":"system","content":"..."}
{"role":"user","content":"install git"}
{"role":"assistant","tool_calls":[{"id":"toolu_001",...}]}  ← SAME
{"role":"tool","tool_call_id":"toolu_001","content":"..."}  ← SAME
{"role":"assistant","tool_calls":[{"id":"toolu_002",...}]}  ← SAME
{"role":"tool","tool_call_id":"toolu_002","content":"..."}  ← SAME
{"role":"assistant","tool_calls":[{"id":"toolu_003",...}]}  ← SAME
{"role":"tool","tool_call_id":"toolu_003","content":"..."}  ← SAME
{"role":"user","content":"use chocolatey instead"}         ← NEW (different)
{"role":"assistant","tool_calls":[{"id":"toolu_007",...}]} ← NEW (different ID)
{"role":"tool","tool_call_id":"toolu_007","content":"..."}  ← NEW
```

## Detection Algorithm Pseudocode

```python
def detect_branches(conversations):
    # Step 1: Extract tool_call_id sequences
    for conv in conversations:
        conv.tool_call_ids = extract_tool_call_ids(conv)
    
    # Step 2: Build prefix groups
    prefix_groups = {}
    for conv in conversations:
        for i in range(len(conv.tool_call_ids)):
            prefix = tuple(conv.tool_call_ids[:i+1])
            if prefix not in prefix_groups:
                prefix_groups[prefix] = []
            prefix_groups[prefix].append(conv)
    
    # Step 3: Identify parent-child relationships
    for conv in conversations:
        # Find longest matching prefix from other conversations
        longest_match = None
        longest_length = 0
        
        for other in conversations:
            if other == conv:
                continue
            
            common_length = get_common_prefix_length(
                conv.tool_call_ids, 
                other.tool_call_ids
            )
            
            # If other is exact prefix, it's the parent
            if (common_length == len(other.tool_call_ids) and 
                common_length < len(conv.tool_call_ids) and
                common_length > longest_length):
                longest_match = other
                longest_length = common_length
        
        if longest_match:
            conv.parent_id = longest_match.id
            longest_match.branch_ids.append(conv.id)
    
    return build_tree(conversations)

def get_common_prefix_length(list_a, list_b):
    length = 0
    for a, b in zip(list_a, list_b):
        if a == b:
            length += 1
        else:
            break
    return length
```

## Journal Display Example

```
# Journal for December 20, 2024

## 10:00 AM - Installing Git CLI

> install git cli
> check if it works

Checked for git, installed using winget, verified version.

  ↳ 10:15 AM - Trying NVM approach (Branch A)
    Used nvm to install Node.js instead, configured path.
  
  ↳ 10:18 AM - Chocolatey installation (Branch B)
    Installed using chocolatey, added to PATH.

---

## 11:30 AM - Fixing Build Errors

> build is failing with CS0103

Added missing using statement, fixed compilation.

  ↳ 11:35 AM - Alternative fix with IEnumerable
    Tried different approach using IEnumerable.
```

## Tree Display Example

```
$ cycodj branches

Conversation Tree:

📁 2024-12-20 10:00 AM - Installing Git CLI
   chat-history-1754437373.jsonl
   ├─ 📄 10:15 AM - Trying NVM approach
   │    chat-history-1754437766.jsonl
   │    └─ 📄 10:20 AM - NVM configuration tweaks
   │         chat-history-1754437999.jsonl
   │
   └─ 📄 10:18 AM - Chocolatey installation
        chat-history-1754437862.jsonl

📁 2024-12-20 11:30 AM - Fixing Build Errors
   chat-history-1754438564.jsonl
   ├─ 📄 11:35 AM - Alternative fix with IEnumerable
   │    chat-history-1754438623.jsonl
   │
   └─ 📄 11:40 AM - Refactored to LINQ
        chat-history-1754438701.jsonl
```

## Real Data Example (from findings)

From your actual history:
```
chat-history-1754437766985.jsonl  (10:22 AM)
├─ toolu_vrtx_01HEpSDWBG6MSN9es1qX18ej
├─ toolu_vrtx_01BBxwL7zWLpF5R1aEfL9gLb
└─ toolu_vrtx_01BWxB4oA4eJUnnjZHsDDc4M

chat-history-1754437862035.jsonl  (10:25 AM)  ← Branched from above!
├─ toolu_vrtx_01HEpSDWBG6MSN9es1qX18ej  ← Same
├─ toolu_vrtx_01BBxwL7zWLpF5R1aEfL9gLb  ← Same
├─ toolu_vrtx_01BWxB4oA4eJUnnjZHsDDc4M  ← Same
└─ [... different IDs after this point ...]
```

These two files share the first 3 tool_call_ids, meaning:
- They started from the same conversation
- They diverged after the 3rd tool call
- The second file (10:25 AM) is a branch of the first (10:22 AM)
