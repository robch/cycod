# Whois MCP Server for C#

This is a C# implementation of the Whois MCP (Model Context Protocol) server. It allows AI agents to perform WHOIS lookups and retrieve domain, IP, TLD, and ASN details through a standardized interface.

## Overview

This MCP server provides tools for querying WHOIS information about domains, IP addresses, top-level domains (TLDs), and autonomous system numbers (ASNs).

## Available Tools

The following tools are available:

| Tool           | Description                                                  |
|----------------|--------------------------------------------------------------|
| `WhoisDomain`  | Looks up WHOIS information about a domain                    |
| `WhoisTld`     | Looks up WHOIS information about a Top Level Domain (TLD)    |
| `WhoisIp`      | Looks up WHOIS information about an IP address               |
| `WhoisAs`      | Looks up WHOIS information about an Autonomous System Number |

## Building and Running

### Requirements

- .NET SDK 8.0 or later

### Build

```bash
dotnet build
```

### Run

```bash
dotnet run
```

## Using with MCP Clients

This server implements the Model Context Protocol, making it compatible with various MCP clients.

## Implementation Details

This C# implementation uses:
- [ModelContextProtocol](https://www.nuget.org/packages/ModelContextProtocol/) library for MCP implementation
- [Whois](https://www.nuget.org/packages/Whois/) library for WHOIS lookups