Use `winget` to install the .NET 8 SDK,  
Then use `dotnet` to install the ChatX CLI (and MDX CLI).

```bash
winget install -e --id Microsoft.DotNet.SDK.8
dotnet tool install --global ChatX --prerelease
dotnet tool install --global mdx --prerelease
```

??? tip "If you don't have `winget` ..."

    [Install WinGet](https://learn.microsoft.com/en-us/windows/package-manager/winget/#install-winget)  
    Walks you through installing the Windows Package Manager.
