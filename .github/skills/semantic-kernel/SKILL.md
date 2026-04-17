---
name: semantic-kernel
description: 'Create, update, refactor, explain, or review Semantic Kernel solutions for this .NET repository using shared guidance plus .NET-specific references.'
---

# Semantic Kernel

Use this skill when working with applications, plugins, function-calling flows, or AI integrations built on Semantic Kernel.

Always ground implementation advice in the latest Semantic Kernel documentation and samples rather than memory alone.

## Use the .NET workflow

This repository is .NET-based, so follow [references/dotnet.md](references/dotnet.md) when making recommendations or code changes.

## Always consult live documentation

- Read the Semantic Kernel overview first: <https://learn.microsoft.com/semantic-kernel/overview/>
- Prefer official docs and samples for the current API surface.
- Use the Microsoft Docs MCP tooling when available to fetch up-to-date framework guidance and examples.

## Shared guidance

When working with Semantic Kernel here:

- Use async patterns for kernel operations.
- Follow official plugin and function-calling patterns.
- Implement explicit error handling and logging.
- Prefer strong typing, clear abstractions, and maintainable composition patterns.
- Use built-in connectors for Azure AI Foundry, Azure OpenAI, OpenAI, and other AI services, while preferring Azure AI Foundry services for new projects when that fits the task.
- Use the kernel's memory and context-management capabilities when they simplify the solution.
- Use `DefaultAzureCredential` when Azure authentication is appropriate.

## Workflow

1. Read the .NET reference file.
2. Fetch the latest official docs and samples before making implementation choices.
3. Apply the shared Semantic Kernel guidance from this skill.
4. Use the .NET package, repository paths, sample paths, and coding practices from the reference.
5. When examples in the repo differ from current docs, explain the difference and follow the current supported pattern.

## References

- [.NET reference](references/dotnet.md)

## Completion criteria

- Recommendations match the .NET target language.
- Package names, repository paths, and sample locations match the selected ecosystem.
- Guidance reflects current Semantic Kernel documentation rather than stale assumptions.
