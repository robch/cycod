# C# to TypeScript Namespace Mappings

| .NET Namespace                        | TypeScript Equivalent                               |
|--------------------------------------|--------------------------------------------------|
| Amazon.BedrockRuntime, Amazon        | `@aws-sdk/client-bedrock`                         |
| Anthropic.SDK.Messaging, Anthropic.SDK | HTTP client (`axios`, `node-fetch`)               |
| Azure.AI.OpenAI, Azure               | `@azure/ai-openai`                                |
| GeminiDotnet.Extensions.AI, GeminiDotnet | HTTP client or JavaScript SDK                   |
| Microsoft.Extensions.AI              | Direct API calls or JavaScript AI library         |
| ModelContextProtocol.Client          | Custom implementation or protocol handling        |
| OpenAI.Chat, OpenAI                  | Libraries like `openai-api` or direct API calls   |
| System.ClientModel.Primitives, System.ClientModel | Define similar structures in TypeScript |
| System.Collections.Generic, System.Collections | `Array`, `Set`, `Map`                       |
| System.ComponentModel                | TypeScript interfaces or classes                  |
| System.IO                            | Node.js `fs` module                                |
| System.Linq                          | `map`, `filter`, `reduce` on arrays               |
| System.Net.Http.Headers, System.Net.Http | `fetch` API or libraries like `axios`          |
| System.Reflection.Metadata.Ecma335, System.Reflection | `reflect-metadata` package             |
| System.Runtime.Intrinsics.X86        | Custom implementation for low-level operations   |
| System.Text.Json.Nodes, System.Text.Json.Serialization, System.Text.Json, System.Text.RegularExpressions, System.Text | `JSON.parse`, `regex` |
| System.Threading.Tasks, System.Threading | `Promise`, `setTimeout`, `setInterval`         |
| System                               | Native JavaScript types and utility functions     |
