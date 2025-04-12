Use `brew` to install the .NET 8 SDK,  
Then use `dotnet` to install the ChatX CLI (and MDX CLI).

```bash
brew install --cask dotnet-sdk
dotnet tool install --global ChatX --prerelease
dotnet tool install --global mdx --prerelease
```

??? tip "If you don't have `brew` ..."

    [Install Homebrew](https://brew.sh/)  
    Walks you through installing Homebrew, a package manager for macOS.

--8<-- "tips/tip-why-need-mdx.md"
