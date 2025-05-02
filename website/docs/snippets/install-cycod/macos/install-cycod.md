Use `brew` to install the .NET 8 SDK,  
Then use `dotnet` to install the CycoD CLI (and CYCODMD CLI).

```bash
brew install --cask dotnet-sdk
dotnet tool install --global CycoD --prerelease
dotnet tool install --global cycodmd --prerelease
```

??? tip "If you don't have `brew` ..."

    [Install Homebrew](https://brew.sh/)  
    Walks you through installing Homebrew, a package manager for macOS.

--8<-- "tips/tip-why-need-cycodmd.md"
