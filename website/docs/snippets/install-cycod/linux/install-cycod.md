Install the .NET 8 SDK for your distribution,  
Then use `dotnet` to install the CycoD CLI (and CYCODMD CLI).

```bash
# For Ubuntu/Debian:
wget https://packages.microsoft.com/config/ubuntu/$(lsb_release -rs)/packages-microsoft-prod.deb -O packages-microsoft-prod.deb
sudo dpkg -i packages-microsoft-prod.deb
sudo apt-get update
sudo apt-get install -y dotnet-sdk-8.0

# Install CycoD CLI
dotnet tool install --global CycoD --prerelease
dotnet tool install --global cycodmd --prerelease
```

??? tip "Other Linux distributions"

    For other Linux distributions, follow the .NET installation instructions for your specific distribution from the [Microsoft .NET documentation](https://docs.microsoft.com/en-us/dotnet/core/install/linux).

--8<-- "tips/tip-why-need-cycodmd.md"
