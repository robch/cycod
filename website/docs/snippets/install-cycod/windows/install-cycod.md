Use `winget` to install the .NET 8 SDK,  
Then use `dotnet` to install the CycoD CLI (and CYCODMD CLI).

```bash
winget install -e --id Microsoft.DotNet.SDK.8
dotnet tool install --global CycoD --prerelease
dotnet tool install --global cycodmd --prerelease
```

??? tip "If you don't have `winget` ..."

    [Install WinGet](https://learn.microsoft.com/en-us/windows/package-manager/winget/#install-winget)  
    Walks you through installing the Windows Package Manager.

--8<-- "tips/tip-why-need-cycodmd.md"
