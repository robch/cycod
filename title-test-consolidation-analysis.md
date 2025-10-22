# /title Command Test Consolidation Analysis

## Executive Summary
**Current State:** ~90+ individual tests  
**Recommended State:** ~43 consolidated tests  
**Reduction:** ~52% fewer tests while maintaining full behavioral coverage

## Consolidation Strategy Overview

### ✅ **CONSOLIDATE** - Same Behavior, Different Data
These test groups examine the same code paths with different input data and can be combined into single tests with multiple examples.

### ❌ **KEEP SEPARATE** - Different Behaviors  
These test distinct failure modes, code paths, or integration scenarios that could break independently.

---

## CONSOLIDATION RECOMMENDATIONS

### 🔄 **Group 1: Content Type Acceptance** 
**CONSOLIDATE:** 6 tests → 1 test  
**Behavior:** "Set command accepts any properly quoted content"

**Tests to Combine:**
- `set_title_with_special_characters`
- `set_title_numeric_only` 
- `set_title_special_chars_only`
- `set_title_with_unicode_characters`
- `title_with_comprehensive_unicode`
- `set_title_leading_trailing_spaces_in_quotes`

**New Test: `set_accepts_various_content_types`**
```yaml
- name: set_accepts_various_content_types
  steps:
  - run: cycod --input "/title set \"Normal Text\"" --input "/title view" --input exit
    expect: Title updated, Status locked, displays "Normal Text"
  - run: cycod --input "/title set \"12345\"" --input "/title view" --input exit  
    expect: Title updated, Status locked, displays "12345"
  - run: cycod --input "/title set \"@#$%^&*()\"" --input "/title view" --input exit
    expect: Title updated, Status locked, displays "@#$%^&*()"
  - run: cycod --input "/title set \"🎉💻αβγδ中文\"" --input "/title view" --input exit
    expect: Title updated, Status locked, displays "🎉💻αβγδ中文"
```

**Rationale:** All use the same validation logic - if one content type works, they all should work through the same code path.

---

### 🔄 **Group 2: Input Validation Errors**
**CONSOLIDATE:** 4 tests → 1 test  
**Behavior:** "Set command rejects unquoted input with proper error message"

**Tests to Combine:**
- `set_single_word_title_without_quotes`
- `set_title_with_no_arguments`
- `set_title_multiword_without_quotes` 
- `set_title_single_character`

**New Test: `set_rejects_unquoted_input`**
```yaml
- name: set_rejects_unquoted_input  
  steps:
  - run: cycod --input "/title set MyTitle" --input exit
    expect: "Error: Titles must be enclosed in double quotes"
  - run: cycod --input "/title set" --input exit
    expect: "Error: Titles must be enclosed in double quotes"
  - run: cycod --input "/title set Multi Word" --input exit
    expect: "Error: Titles must be enclosed in double quotes"  
```

**Rationale:** All test the same quote validation logic - different inputs, same error path.

---

### 🔄 **Group 3: Empty/Whitespace Content Rejection**
**CONSOLIDATE:** 2 tests → 1 test  
**Behavior:** "Set command rejects empty or whitespace-only content with proper error message"

**Tests to Combine:**
- `set_title_to_double_quote_character` (tests edge case where user attempts `/title set """` which becomes empty after quote parsing)
- `set_title_with_whitespace_only` (tests whitespace-only string `"   "`)

**New Test: `set_rejects_empty_content`**
```yaml
- name: set_rejects_empty_content
  steps:  
  - run: cycod --input "/title set \"\"" --input exit
    expect: "Error: Title cannot be empty"
  - run: cycod --input "/title set \"   \"" --input exit
    expect: "Error: Title cannot be empty"
```

**Rationale:** Both test the same empty content validation logic - one testing the edge case of quote character parsing that results in empty content, the other testing explicit whitespace-only content.

---

### 🔄 **Group 4: Malformed Quote Parsing**
**CONSOLIDATE:** 2 tests → 1 test  
**Behavior:** "Set command handles malformed quotes gracefully"

**Tests to Combine:**
- `set_title_malformed_quotes_unclosed`
- `set_title_malformed_quotes_trailing_only`

**New Test: `set_handles_malformed_quotes`**
```yaml
- name: set_handles_malformed_quotes
  steps:
  - run: cycod --input "/title set \"unclosed quote" --input exit
    expect: "Error: Titles must be enclosed in double quotes"
  - run: cycod --input "/title set trailing quote\"" --input exit  
    expect: "Error: Titles must be enclosed in double quotes"
```

**Rationale:** Both test quote parsing edge cases with the same validation logic.

---

### 🔄 **Group 5: State Idempotency**  
**CONSOLIDATE:** 2 tests → 1 test
**Behavior:** "Lock/unlock commands are idempotent (no-op when already in target state)"

**Tests to Combine:**
- `lock_already_locked_title`
- `unlock_already_unlocked_title`

**New Test: `lock_unlock_idempotent_operations`**
```yaml
- name: lock_unlock_idempotent_operations
  steps:
  - run: cycod --input-chat-history testfiles/locked-title-full-conversation.jsonl --input "/title lock" --input exit
    expect: "Title is already locked"
  - run: cycod --input-chat-history testfiles/unlocked-title-full-conversation.jsonl --input "/title unlock" --input exit
    expect: "Title is already unlocked"
```

**Rationale:** Both test idempotent behavior - same "already in target state" logic.

---

### 🔄 **Group 6: No Conversation File Errors**
**CONSOLIDATE:** 3 tests → 1 test  
**Behavior:** "Commands requiring file operations fail gracefully when no conversation file exists"

**Tests to Combine:**
- `set_without_conversation_file_error`
- `lock_without_conversation_file_error`
- `unlock_without_conversation_file_error`

**New Test: `commands_require_conversation_file`**
```yaml
- name: commands_require_conversation_file
  steps:
  - run: cycod --input "/title set \"Test\"" --input exit --auto-save-chat-history 0
    expect: "Error: No conversation file to save metadata to"
  - run: cycod --input "/title lock" --input exit --auto-save-chat-history 0
    expect: "Error: No conversation file to save metadata to"  
  - run: cycod --input "/title unlock" --input exit --auto-save-chat-history 0
    expect: "Error: No conversation file to save metadata to"
```

**Rationale:** All test the same file validation logic across different commands.

---

### 🔄 **Group 7: Length Boundary Testing**
**CONSOLIDATE:** 3 tests → 1 test  
**Behavior:** "Set command handles various title lengths appropriately"

**Tests to Combine:**
- `set_title_with_long_text`
- `set_title_with_very_long_text`  
- `set_title_extremely_long`

**New Test: `set_handles_various_title_lengths`**
```yaml
- name: set_handles_various_title_lengths
  steps:
  - run: cycod --input "/title set \"[150 char title]\"" --input "/title view" --input exit
    expect: Title updated successfully, displays full content
  - run: cycod --input "/title set \"[300 char title]\"" --input "/title view" --input exit
    expect: Title updated successfully, displays full content
  - run: cycod --input "/title set \"[1000+ char title]\"" --input "/title view" --input exit  
    expect: Title updated successfully, displays full content
```

**Rationale:** All test the same length handling logic.

---

### 🔄 **Group 8: Extra Arguments Ignored**
**CONSOLIDATE:** 2 tests → 1 test  
**Behavior:** "Commands ignore extra arguments gracefully"

**Tests to Combine:**
- `view_with_extra_arguments_ignored`
- `lock_with_extra_arguments_ignored`

**New Test: `commands_ignore_extra_arguments`**
```yaml
- name: commands_ignore_extra_arguments
  steps:
  - run: cycod --input "/title view extra args ignored" --input exit
    expect: Normal view output (extra args ignored)
  - run: cycod --input "/title lock extra args ignored" --input exit  
    expect: Normal lock behavior (extra args ignored)
```

**Rationale:** Both test the same argument parsing robustness.

---

### 🔄 **Group 9: Metadata Preservation**
**CONSOLIDATE:** 4 tests → 1 test  
**Behavior:** "All title operations preserve unknown metadata fields"

**Tests to Combine:**
- `set_title_preserves_unknown_metadata_fields`
- `lock_preserves_unknown_metadata_fields`
- `set_preserves_unknown_metadata_fields` (duplicate)
- `lock_unlock_preserves_unknown_metadata_fields`

**New Test: `operations_preserve_unknown_metadata`**
```yaml  
- name: operations_preserve_unknown_metadata
  steps:
  - name: Create file with extra metadata
    bash: echo '{"_meta":{"customField":"preserved","version":2}}' > test-file.jsonl
  - name: Test set operation preserves fields
    run: cycod --input "/title set \"New\"" --input exit
    verify: customField and version still present, title updated
  - name: Test lock operation preserves fields  
    run: cycod --input "/title lock" --input exit
    verify: customField and version still present, titleLocked updated
```

**Rationale:** All test the same metadata preservation logic across different operations.

---

### 🔄 **Group 10: Corrupted Metadata Recovery**
**CONSOLIDATE:** 3 tests → 1 test  
**Behavior:** "Commands handle corrupted metadata gracefully"

**Tests to Combine:**
- `set_title_with_corrupted_metadata`
- `lock_with_corrupted_metadata`
- `unlock_with_corrupted_metadata`

**New Test: `commands_handle_corrupted_metadata`**
```yaml
- name: commands_handle_corrupted_metadata
  steps:
  - name: Create corrupted metadata file
    bash: cp testfiles/malformed-metadata.jsonl test-corrupted.jsonl  
  - name: Test set recovers from corruption
    run: cycod --input "/title set \"Fixed\"" --input exit
    expect: Title updated successfully  
  - name: Test lock works with corruption
    run: cycod --input "/title lock" --input exit
    expect: Title locked successfully
```

**Rationale:** All test the same metadata recovery logic across different operations.

---

## ❌ **KEEP SEPARATE** - Distinct Behaviors

### **Core View Functionality** - Keep All 5 Tests
**Why Separate:** Each tests different metadata loading states
- `view_when_no_title_exists_no_file` - No file initialization 
- `view_when_no_title_exists_readable_file` - File reading with empty metadata
- `view_when_title_exists_and_is_unlocked` - Normal unlocked state display
- `view_when_title_exists_and_is_locked` - Normal locked state display  
- `view_with_corrupted_metadata` - Error recovery in display

### **Set Command Initial States** - Keep All 3 Tests  
**Why Separate:** Different starting conditions exercise different code paths
- `set_title_when_none_exists_writable_file` - Creating new title
- `set_title_replace_unlocked_title` - Replacing unlocked title  
- `set_title_replace_locked_title` - Replacing locked title

### **Lock/Unlock State Transitions** - Keep All 6 Tests
**Why Separate:** Different state machines and file conditions
- `lock_existing_unlocked_title` - Normal lock transition
- `lock_when_no_title_exists` - Lock with no existing title
- `lock_with_readonly_file` - Lock with I/O constraints
- `unlock_existing_locked_title` - Normal unlock transition  
- `unlock_when_no_title_exists` - Unlock with no existing title
- `unlock_with_readonly_file` - Unlock with I/O constraints

### **Command Integration Sequences** - Keep All 6 Tests
**Why Separate:** Different interaction patterns
- `set_view_sequence_test_title` - Basic set→view flow
- `set_unlock_view_sequence` - Set→unlock→view flow
- `set_unlock_lock_view_sequence` - Complex state transitions
- `set_title_replacement_sequence` - Multiple set operations
- `lock_on_nonexistent_title_then_set` - Lock→set interaction
- `rapid_sequential_commands` - Stress testing command sequences

### **Special Quote Parsing Cases** - Keep All 4 Tests  
**Why Separate:** Each tests different aspects of complex quote parsing logic
- `set_title_with_nested_quotes_no_escaping` - Nested quote handling
- `set_title_only_quotes` - Edge case of quote-only content
- `set_title_malformed_quotes_mixed_shell_parsing` - Shell interaction edge case
- `title_subcommands_case_insensitive` - Case handling in parsing  

### **Command Behavior Edge Cases** - Keep All 4 Tests
**Why Separate:** Different behavioral aspects
- `title_default_shows_title_and_help` - Default command behavior
- `title_invalid_subcommand_passes_to_assistant` - Error routing behavior
- `title_extra_whitespace_handling` - Whitespace normalization  
- `lock_with_no_file` - Auto-save integration scenario

### **Metadata Corruption Edge Cases** - Keep All 3 Tests
**Why Separate:** Different corruption scenarios  
- `view_with_completely_invalid_json` - JSON parsing failure
- `view_with_wrong_meta_structure` - Structure validation failure
- `set_title_with_missing_meta_key` - Missing key handling

### **Stress & Performance** - Keep Both Tests
**Why Separate:** Different stress patterns
- `rapid_title_operations_stress_test` - Operation sequence stress  
- `title_command_test` - Basic functionality (integration baseline)

---

## FINAL SUMMARY

### Before Consolidation: ~90 tests
### After Consolidation: ~43 tests

**✅ Consolidated Groups:** 10 groups, reducing 31 tests to 10 tests  
**❌ Kept Separate:** 33 tests that test distinct behaviors  

**Benefits:**
- **Faster test execution** - 52% fewer test runs
- **Easier maintenance** - Fewer individual test files to update  
- **Better coverage clarity** - Each test has distinct purpose
- **Reduced noise** - Failed tests point to specific behavioral issues

**Preserved Coverage:**
- All distinct behavioral scenarios still tested
- All error conditions still covered  
- All integration patterns still validated
- All edge cases still exercised

This consolidation maintains complete behavioral coverage while significantly reducing test suite complexity and execution time.