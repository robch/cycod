# Transitioning to Microsoft.Extensions.AI.IChatClient

This directory contains documentation and sample code for transitioning the chatx application from using OpenAI's ChatCompletions API directly to using Microsoft.Extensions.AI's IChatClient interface.

## Documentation

1. [Transition Plan](01-transition-plan.md) - Overview of the transition strategy and goals
2. [Implementation Comparison](02-implementation-comparison.md) - Side-by-side comparison of current and target implementations
3. [ChatClientFactory Design](03-chatclientfactory-design.md) - Detailed design for updating the ChatClientFactory
4. [FunctionCallingChat Design](04-functioncallingchat-design.md) - Detailed design for updating FunctionCallingChat
5. [Function Calling Adaptation](05-function-calling-adaptation.md) - Strategy for adapting function calling between systems
6. [Step-by-Step Guide](06-step-by-step-guide.md) - Practical guide for implementing the transition
7. [Benefits and Future Considerations](07-benefits-and-future-considerations.md) - Advantages of the transition and future opportunities
8. [Reference Links](08-reference-links.md) - Direct links to relevant source code in Microsoft.Extensions.AI and chatx

## Sample Code

The `sample-code` directory contains example implementations for key components:

- [ChatClientFactory.cs](sample-code/ChatClientFactory.cs) - Updated factory that returns IChatClient instances
- [MessageConversionExtensions.cs](sample-code/MessageConversionExtensions.cs) - Utilities for converting between message formats
- [FunctionCallDetector.cs](sample-code/FunctionCallDetector.cs) - Helper for detecting function calls in streaming responses
- [FunctionCallingChat.cs](sample-code/FunctionCallingChat.cs) - Updated implementation that works with IChatClient

## Key Benefits of the Transition

1. **Standardized Abstraction** - A consistent interface for interacting with AI services
2. **Enhanced Interoperability** - Easier integration with different AI providers
3. **Middleware Pipeline** - Ability to add functionality like caching, logging, and telemetry
4. **Future-Proofing** - Alignment with Microsoft's AI strategy
5. **Consistent Error Handling** - Standardized approach across providers

## Transition Strategy

The transition approach focuses on:

1. Maintaining existing function calling capabilities
2. Minimizing changes to the public API
3. Creating adapter utilities to bridge between the two systems
4. Incrementally updating components

## Implementation Approach

1. Add required NuGet packages
2. Create message conversion utilities
3. Update ChatClientFactory to return IChatClient instances
4. Create function call detection adapters
5. Update FunctionCallingChat to work with IChatClient
6. Test with all supported providers

## Testing Strategy

The implementation should be verified with:

1. Unit tests for conversion utilities
2. Integration tests for each provider:
   - OpenAI
   - Azure OpenAI
   - GitHub Copilot (token-based)
   - GitHub Copilot (HMAC-based)
3. Function calling tests with various function types
4. Streaming response tests