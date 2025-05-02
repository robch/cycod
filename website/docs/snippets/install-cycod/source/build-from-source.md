Clone the repository and build CycoD from source:

```bash
# Clone the repository
git clone https://github.com/robch/cycod.git
cd cycod

# Build the project
dotnet build

# Run the application
dotnet run

# Update your PATH to include the cycod executable
# ... 
```

--8<-- "tips/tip-cycod-requires-cycodmd.md"

??? tip "How do I install or build the `cycodmd` CLI"

    CycoD requires the `cycodmd` CLI to run. You can either install it from NuGet or build it from source.  

    === "Install `cycodmd` CLI"

        ```bash
        dotnet tool install -g cycodmd --prerelease
        ```
    
        After installation, you can verify that CYCODMD is available by running:

        --8<-- "tips/cycodmd-cli-verify-version-info.md"

    === "Build `cycodmd` from source"

        ```bash
        # Clone the repository
        git clone https://github.com/robch/cycod.git
        cd cycod

        # Build the project
        dotnet build

        # Update your PATH to include the cycodmd executable
        # ... 
        ```

        After building, you can verify that CYCODMD is available by running:

        --8<-- "tips/cycodmd-cli-verify-version-info.md"
