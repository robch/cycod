# Lightweight Code Review Process

This document outlines our lightweight code review process for systematic review of files against our coding standards. This process complements our [comprehensive code review process](Code-Review-Process.md) by providing a structured approach to reviewing files against established style guidelines.

## Purpose

The lightweight code review process serves several key purposes:

1. **Systematic Identification**: Efficiently identify specific code style violations across the codebase
2. **Style Conformance**: Verify that files adhere to our [C# Coding Style Essential Guide](C%23-Coding-Style-Essential.md) 
3. **Precise Documentation**: Create a clear record of violations with exact line references for future correction
4. **Actionable Feedback**: Provide specific, implementable solutions for each identified issue
5. **Localized Reviews**: Keep review documentation alongside the source code for better discoverability

## Process Overview

### 1. Create a Master Review Checklist

First, create a master checklist of files to review, called `Files-To-Review.md`, in the root of your project repository. This checklist should include all files you plan to review.

````markdown
# Files to Review

## C# Source Files
- [ ] src/path/to/File1.cs
- [ ] src/path/to/File2.cs

## Documentation Files
- [ ] docs/SomeDoc.md
- [ ] docs/AnotherDoc.md

````

### File Selection Strategy

When creating your master checklist, consider these approaches for selecting files to review:

- **Branch Changes**: Include files modified in your current branch since it branched from main
- **Recent Changes**: Include files changed in the last few days or 24 hours
- **Personal Changes**: Focus on files you've recently modified yourself
- **High-Impact Areas**: Focus on core or critical code that would benefit most from style compliance

### 2. Review Each File

For each file in your master checklist:

1. **Create a review file** in the same directory as the source file using the naming convention: `{original-filename}.review.md`
2. **Copy the review template** (provided at the end of this document) into this file
3. **Review the file systematically** against all 30 sections of the [C# Coding Style Essential Guide](C%23-Coding-Style-Essential.md)
4. **Break down each section** into specific rules and mark each with a visual indicator:
   - ✅ Compliant with the rule
   - ❌ Violates the rule (critical issue)
   - ⚠️ Partially compliant or minor issue
5. **Document issues** found, with references to both the style guide and the code
6. **Mark the file as reviewed** in your master checklist once complete

## AI-Assisted Workflow

This lightweight code review process is designed to work with AI assistants like cycod:

1. **AI-Generated Reviews**: Rather than manually creating review files, developers can use AI tools to analyze source code and generate the review files automatically.
2. **Human Verification**: The developer reviews the AI-generated findings before proceeding with any changes.
3. **AI-Assisted Fixes**: After reviewing, the developer can ask the AI to update the corresponding source files based on the agreed-upon issues.

### Instructions for AI Code Reviewers

When using AI assistants for code review:

1. **First pass**: Have the AI scan the file against the entire style guide
2. **Detailed analysis**: For each style guide section, ask the AI to break it down into specific rules and check compliance with each
3. **Visual indicators**: Have the AI use ✅, ❌, and ⚠️ icons to clearly show compliance status
4. **Issue generation**: Have the AI generate detailed issue descriptions with style guide references, code violations, and recommended changes
5. **Prioritization**: Ask the AI to prioritize issues based on severity and impact

The review files (`.review.md`) are intended to be temporary artifacts that serve as an intermediate step between code analysis and code improvement, not as permanent documentation to be committed to the repository.

## Detailed Section Review Approach

When reviewing each style guide section, rather than simply checking off the entire section, break it down into specific rules and principles to verify:

1. **Identify Key Rules**: For each section, extract the specific rules and principles mentioned in the style guide
2. **Check Individual Rules**: Verify compliance with each specific rule independently
3. **Document Violations**: For any rule violations, document exactly which aspect of the section was violated
4. **Comprehensive Coverage**: Ensure all aspects of each section are checked, not just the most obvious ones

### Example of Detailed Section Review

For the "Asynchronous Programming" section, instead of a single checkbox, break it down like this:

```markdown
- [x] Asynchronous Programming
  - ✅ Methods use async/await for asynchronous operations
  - ✅ Async methods return Task or Task<T>, not void
  - ✅ Async methods are named with the "Async" suffix
  - ❌ No blocking calls (.Wait(), .Result, etc.) in async methods
    - **Violation**: `SendInputToShellOrProcess` uses `.Result` on line 374
  - ✅ ConfigureAwait(false) is not used in application code
```

This approach makes violations immediately obvious and ensures thorough review of all aspects of each style guide section.

## Review Approach

Our process emphasizes thoroughness and actionable feedback:

1. **Systematic Review**: Work through all 30 style guide areas methodically, checking specific rules within each
2. **Evidence-Based Feedback**: For each issue, provide:
   - The exact style guide reference with line numbers
   - The exact code in violation with line numbers
   - A clear recommendation for fixing the issue
3. **Completion Tracking**: Use visual indicators (✅, ❌, ⚠️) to clearly show compliance status of each rule
4. **Focus on Impact**: Prioritize meaningful issues over trivial style preferences

## Example Issue Documentation

Here's a streamlined example of how to document an issue in the review file:

````markdown
### Issue 1: Use var for Local Variable Declarations

**Style Guide Reference:**
```
// Use var for local variables
var customer = GetCustomerById(123);
var isValid = Validate(customer);
var orders = customer.Orders.Where(o => o.IsActive).ToList();
```
(Lines 11-13)

**Code in Violation:**
```csharp
string name = GetUserName();
int count = users.Count();
List<Order> orders = GetOrders();
```
(Lines 27-29)

**Recommended Change:**
Use var for local variable declarations when the type is obvious from the right side:
```csharp
var name = GetUserName();
var count = users.Count();
var orders = GetOrders();
```

````

## When to Use This Process

Use the lightweight code review process when:

1. **Systematic Review**: Reviewing large numbers of files systematically against the style guide
2. **Style Validation**: Validating code against specific sections of the style guide
3. **Actionable Feedback**: Creating specific, implementable solutions for each issue
4. **Team Onboarding**: Helping new team members understand codebase standards
5. **Incremental Improvement**: Performing gradual code quality improvements across a codebase
6. **Pre-PR Quality Check**: Using before creating Pull Requests to catch style issues proactively
7. **Codebase Familiarization**: Helping developers quickly understand style expectations in a project
8. **AI-Assisted Code Cleanup**: Leveraging AI tools to systematically improve code quality

## Per-File Review Template

Copy this template to create a review file for each source file:

````markdown
# Code Review: [Filename]

- **File Path**: [Full path to file]
- **Review Date**: [YYYY-MM-DD]

## Style Guide Checklist

Check off each section as you complete reviewing the file against it:

- [ ] Variables and Types
  - [ ] Use var for local variable declarations
  - [ ] ...
  - ...
- [ ] Method and Property Declarations
  - [ ] Methods start with verbs and use PascalCase
  - [ ] ...
  - ...
- [ ] Control Flow
  - [ ] ...
  - ...
- [ ] Collections
  - ...
- [ ] Exception Handling and Error Returns
- [ ] Class Structure
- [ ] Comments and Documentation
- [ ] LINQ
- [ ] String Handling
- [ ] Expression-Bodied Members
- [ ] Null Handling
- [ ] Asynchronous Programming
- [ ] Static Methods and Classes
- [ ] Parameters
- [ ] Code Organization
- [ ] Method Returns
- [ ] Parameter Handling
- [ ] Method Chaining
- [ ] Resource Cleanup
- [ ] Field Initialization
- [ ] Logging Conventions
- [ ] Class Design and Relationships
- [ ] Condition Checking Style
- [ ] Builder Patterns and Fluent Interfaces
- [ ] Using Directives and Namespaces
- [ ] Default Values and Constants
- [ ] Extension Methods
- [ ] Attributes
- [ ] Generics
- [ ] Project Organization

## Issues Found

### Issue 1: [Brief Description]

**Style Guide Reference:**
```
[Quote from style guide with line numbers]
```

**Code in Violation:**
```csharp
[Quote relevant code with line numbers]
```

**Recommended Change:**
[Description of how to fix the issue]

## Review Summary

[Overall assessment of file's compliance with the style guide]

- [ ] No issues found - file fully complies with style guidelines
- [ ] Minor issues found - easily addressable
- [ ] Significant issues found - requires attention
````

## Example Review Output

Here's an example of what a completed review might look like:

````markdown
# Code Review: UnifiedShellAndProcessHelperFunctions.cs

- **File Path**: src/cycod/FunctionCallingTools/UnifiedShellAndProcessHelperFunctions.cs
- **Review Date**: 2023-11-30

## Style Guide Checklist

- [x] Variables and Types
  - [✅] Use var for local variable declarations
  - [✅] Use underscore prefix for private fields
  - [✅] Use PascalCase for constants
  - [✅] Use descriptive variable names
  - [✅] Prefix boolean variables with Is, Has, or Can

- [x] Method and Property Declarations
  - [✅] Methods start with verbs and use PascalCase
  - [✅] Boolean members use Is/Has/Can prefix
  - [✅] Use auto-properties for simple cases
  - [✅] Keep methods short and focused
  - [✅] Design APIs with the caller's perspective in mind

- [x] Control Flow
  - [✅] Use early returns to reduce nesting
  - [✅] Use ternary operators for simple conditions
  - [✅] Create descriptive variables for complex conditions
  - [✅] Prefer positive conditions over negative ones

- [x] Collections
  - [✅] Use appropriate collection types
  - [✅] Use collection initializers
  - [✅] Return empty collections instead of null

- [x] Exception Handling and Error Returns
  - [✅] Use specific exception types
  - [✅] Include context in error messages
  - [✅] Validate parameters early
  - [⚠️] Consistent error message formatting
    - **Issue**: Error message formats vary between methods
  - [✅] Catches only exceptions that can be meaningfully handled

// ... other sections ...

- [x] Asynchronous Programming
  - [✅] Methods use async/await for asynchronous operations
  - [✅] Async methods return Task or Task<T>, not void
  - [✅] Async methods are named with the "Async" suffix
  - [❌] No blocking calls (.Wait(), .Result, etc.) in async methods
    - **Violation**: `SendInputToShellOrProcess` uses `.Result` on line 374
  - [✅] ConfigureAwait(false) is not used in application code

// ... remaining sections ...
````

## Conclusion

The lightweight code review process provides a structured approach to systematically identify and document coding standard violations on a file-by-file basis. By creating individual review files that remain with the source code, we ensure:

1. Reviews are easily discoverable by developers working on specific files
2. The thoroughness of reviews is trackable through the detailed visual status indicators
3. Each file's compliance with specific rules within all 30 style guide areas is clearly documented
4. Issues are documented close to where they occur, making remediation more straightforward

This process is particularly effective when integrated with AI coding assistants. By generating these reviews automatically and using AI to implement the suggested fixes (after human verification), teams can maintain consistent code style with minimal manual effort. The temporary nature of the review files means they serve as a communication medium between the AI assistant and the developer, rather than as permanent documentation.