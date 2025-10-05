# C# Coding Style Guide: Expanded

## Introduction

This expanded coding style guide builds upon the [C# Coding Style Essential Guide](C%23-Coding-Style-Essential.md) by providing deeper explanations, rationales, and learning resources specifically designed for junior developers or those new to C#.

While the Essential guide provides concise, practical guidance for experienced developers, this expanded guide aims to:

- Explain **why** certain practices are recommended
- Highlight **common mistakes** and their consequences
- Show **progressive examples** of code evolution from poor to best practice
- Build **mental models** to deepen your understanding of C# concepts

Each section follows a consistent structure:
- **Examples** - Code samples demonstrating the concept
- **Core Principles** - Key guidelines to follow
- **Why It Matters** - Explanations of the rationale behind the guidelines
- **Common Mistakes** - Pitfalls to avoid
- **Evolution Example** - Progressive improvement of code samples
- **Deeper Understanding** - Additional technical details and conceptual frameworks

This guide serves as "Layer 2" in our multi-layered documentation approach, providing the explanatory depth that the more concise Essential guide intentionally omits.

## Table of Contents

- [1. Variables and Types](#1-variables-and-types)
- [2. Method and Property Declarations](#2-method-and-property-declarations)
- [3. Control Flow](#3-control-flow)
- [4. Collections](#4-collections)
- [5. Exception Handling and Error Returns](#5-exception-handling-and-error-returns)
- [6. Class Structure](#6-class-structure)
- [7. Comments and Documentation](#7-comments-and-documentation)
- [8. LINQ](#8-linq)
- [9. String Handling](#9-string-handling)
- [10. Expression-Bodied Members](#10-expression-bodied-members)
- [11. Null Handling](#11-null-handling)
- [12. Asynchronous Programming](#12-asynchronous-programming)
- [13. Static Methods and Classes](#13-static-methods-and-classes)
- [14. Parameters](#14-parameters)
- [15. Code Organization](#15-code-organization)
- [16. Method Returns](#16-method-returns)
- [17. Parameter Handling](#17-parameter-handling)
- [18. Method Chaining](#18-method-chaining)
- [19. Resource Cleanup](#19-resource-cleanup)
- [20. Field Initialization](#20-field-initialization)
- [21. Logging Conventions](#21-logging-conventions)
- [22. Class Design and Relationships](#22-class-design-and-relationships)
- [23. Condition Checking Style](#23-condition-checking-style)
- [24. Builder Patterns and Fluent Interfaces](#24-builder-patterns-and-fluent-interfaces)
- [25. Using Directives](#25-using-directives)
- [26. Default Values and Constants](#26-default-values-and-constants)
- [27. Extension Methods](#27-extension-methods)
- [28. Attributes](#28-attributes)
- [29. Generics](#29-generics)
- [30. Project Organization](#30-project-organization)

## 1. Variables and Types

### Examples

### Core Principles

### Why It Matters

### Common Mistakes

### Evolution Example

### Deeper Understanding

## 2. Method and Property Declarations

### Examples

### Core Principles

### Why It Matters

### Common Mistakes

### Evolution Example

### Deeper Understanding

## 3. Control Flow

### Examples

### Core Principles

### Why It Matters

### Common Mistakes

### Evolution Example

### Deeper Understanding

## 4. Collections

### Examples

### Core Principles

### Why It Matters

### Common Mistakes

### Evolution Example

### Deeper Understanding

## 5. Exception Handling and Error Returns

### Examples

### Core Principles

### Why It Matters

### Common Mistakes

### Evolution Example

### Deeper Understanding

## 6. Class Structure

### Examples

### Core Principles

### Why It Matters

### Common Mistakes

### Evolution Example

### Deeper Understanding

## 7. Comments and Documentation

### Examples

### Core Principles

### Why It Matters

### Common Mistakes

### Evolution Example

### Deeper Understanding

## 8. LINQ

### Examples

### Core Principles

### Why It Matters

### Common Mistakes

### Evolution Example

### Deeper Understanding

## 9. String Handling

### Examples

### Core Principles

### Why It Matters

### Common Mistakes

### Evolution Example

### Deeper Understanding

## 10. Expression-Bodied Members

### Examples

### Core Principles

### Why It Matters

### Common Mistakes

### Evolution Example

### Deeper Understanding

## 11. Null Handling

### Examples

### Core Principles

### Why It Matters

### Common Mistakes

### Evolution Example

### Deeper Understanding

## 12. Asynchronous Programming

### Examples

### Core Principles

### Why It Matters

### Common Mistakes

### Evolution Example

### Deeper Understanding

## 13. Static Methods and Classes

### Examples

### Core Principles

### Why It Matters

### Common Mistakes

### Evolution Example

### Deeper Understanding

## 14. Parameters

### Examples

```csharp
// Standard method parameter conventions
public void ProcessOrder(Order order, bool sendConfirmationEmail = false, bool expediteShipping = false)
{
    // Implementation
}

// AI-callable function conventions (with Description attributes)
[Description("Terminates a running process or shell")]
public string TerminateProcess(
    [Description("Name or ID of the process to terminate")] string processId,
    [Description("Whether to force kill if graceful termination fails")] bool force = false)
{
    // Implementation
}
```

### Core Principles

- Use clear, descriptive parameter names that indicate purpose
- Place required parameters before optional parameters
- Set sensible defaults for optional parameters
- Use nullable reference types for optional object parameters
- For boolean parameters, default to false for safer behavior
- Use different naming conventions for standard methods vs. AI-callable functions

### Why It Matters

Well-designed parameters make methods more usable and less error-prone. Clear parameter names and sensible defaults reduce the cognitive load on developers using your API. For methods exposed to AI tools, simpler parameter names make the API more accessible for AI consumption.

### Common Mistakes

- Using non-descriptive parameter names (e.g., `bool flag` instead of `bool includeInactive`)
- Placing optional parameters before required ones
- Not using nullable reference types for optional parameters
- Using overly long parameter names for AI-callable functions
- Not providing clear descriptions in Description attributes for AI-callable functions

### Evolution Example

**Initial version (standard method):**
```csharp
// Original - unclear boolean parameter
public void SendEmail(string recipient, string subject, string body, bool flag)
{
    // Implementation
}

// Usage is unclear
SendEmail("user@example.com", "Hello", "Message body", true);
```

**Improved version (standard method):**
```csharp
// Improved - descriptive boolean parameter
public void SendEmail(string recipient, string subject, string body, bool highPriority = false)
{
    // Implementation
}

// Usage is clear
SendEmail("user@example.com", "Hello", "Message body", highPriority: true);
```

**AI-callable version:**
```csharp
[Description("Sends an email to the specified recipient")]
public string SendEmail(
    [Description("Email address of the recipient")] string recipient,
    [Description("Subject line of the email")] string subject,
    [Description("Body content of the email")] string body,
    [Description("Whether to mark the email as high priority")] bool priority = false)
{
    // Implementation
}

// AI can call this with simple parameter names
// e.g., SendEmail("user@example.com", "Hello", "Message body", priority: true)
```

### Deeper Understanding

Parameter naming conventions differ based on the intended consumer of your API:

1. **For standard C# methods used by developers:**
   - Use Is/Has/Can prefixes for boolean parameters (e.g., `isActive`, `hasPermission`)
   - Use descriptive names even if they're longer
   - Encourage use of named arguments for boolean parameters

2. **For AI-callable functions (with Description attributes):**
   - Use simpler, more direct parameter names (e.g., `force` instead of `shouldForceKill`)
   - Follow CLI tool conventions where appropriate (e.g., `force`, `quiet`, `recursive`)
   - Put the detailed explanation in the Description attribute
   - Prioritize conciseness and clarity over verbosity

This distinction recognizes that AI tools interact with APIs differently than human developers, and parameter naming should be optimized for the intended consumer.

## 15. Code Organization

### Examples

### Core Principles

### Why It Matters

### Common Mistakes

### Evolution Example

### Deeper Understanding

## 16. Method Returns

### Examples

### Core Principles

### Why It Matters

### Common Mistakes

### Evolution Example

### Deeper Understanding

## 17. Parameter Handling

### Examples

### Core Principles

### Why It Matters

### Common Mistakes

### Evolution Example

### Deeper Understanding

## 18. Method Chaining

### Examples

### Core Principles

### Why It Matters

### Common Mistakes

### Evolution Example

### Deeper Understanding

## 19. Resource Cleanup

### Examples

### Core Principles

### Why It Matters

### Common Mistakes

### Evolution Example

### Deeper Understanding

## 20. Field Initialization

### Examples

### Core Principles

### Why It Matters

### Common Mistakes

### Evolution Example

### Deeper Understanding

## 21. Logging Conventions

### Examples

### Core Principles

### Why It Matters

### Common Mistakes

### Evolution Example

### Deeper Understanding

## 22. Class Design and Relationships

### Examples

### Core Principles

### Why It Matters

### Common Mistakes

### Evolution Example

### Deeper Understanding

## 23. Condition Checking Style

### Examples

### Core Principles

### Why It Matters

### Common Mistakes

### Evolution Example

### Deeper Understanding

## 24. Builder Patterns and Fluent Interfaces

### Examples

### Core Principles

### Why It Matters

### Common Mistakes

### Evolution Example

### Deeper Understanding

## 25. Using Directives

### Examples

```csharp
// Group System namespaces first, then others, alphabetized within groups
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

// Our codebase doesn't use explicit namespace declarations
```

### Core Principles

- Group using directives by type (System namespaces first, then others)
- Alphabetize using directives within each group
- Keep a blank line between different import groups
- Do not use namespace declarations in our codebase
- Place classes at the top level of the file

### Why It Matters

Well-organized using directives make it easier to understand the external dependencies of a file. Our project convention of not using namespace declarations simplifies the code structure and avoids unnecessary nesting. This is consistent with modern C# trends toward top-level statements and simplified file organization.

### Common Mistakes

- Mixing System and non-System using directives without proper grouping
- Forgetting to alphabetize using directives, making it harder to spot duplicates
- Accidentally adding namespace declarations, creating inconsistency with the rest of the codebase
- Using fully qualified type names instead of appropriate using directives

### Evolution Example

**Initial version - disorganized:**
```csharp
using Newtonsoft.Json;
using System;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;

namespace MyProject.Services
{
    public class UserService
    {
        // Service implementation
    }
}
```

**Improved version - organized and following project conventions:**
```csharp
using System;
using System.Collections.Generic;
using System.Linq;

using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

public class UserService
{
    // Service implementation
}
```

### Deeper Understanding

While C# 10 introduced file-scoped namespaces for more concise namespace declarations, our project convention is to avoid namespace declarations entirely. This aligns with the trend toward simplified file organization in modern C# and keeps our codebase consistent.

Using directives should be organized thoughtfully at the top of each file. This makes dependencies immediately visible and helps maintain clean, readable code. By grouping System namespaces separately from others, we create a visual distinction between .NET framework dependencies and third-party or project-specific dependencies.

## 26. Default Values and Constants

### Examples

### Core Principles

### Why It Matters

### Common Mistakes

### Evolution Example

### Deeper Understanding

## 27. Extension Methods

### Examples

### Core Principles

### Why It Matters

### Common Mistakes

### Evolution Example

### Deeper Understanding

## 28. Attributes

### Examples

### Core Principles

### Why It Matters

### Common Mistakes

### Evolution Example

### Deeper Understanding

## 29. Generics

### Examples

### Core Principles

### Why It Matters

### Common Mistakes

### Evolution Example

### Deeper Understanding

## 30. Project Organization

### Examples

### Core Principles

### Why It Matters

### Common Mistakes

### Evolution Example

### Deeper Understanding