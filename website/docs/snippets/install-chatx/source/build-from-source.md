Clone the repository and build ChatX from source:

```bash
# Clone the repository
git clone https://github.com/robch/chatx.git
cd chatx

# Build the project
dotnet build

# Run the application
dotnet run

# Update your PATH to include the chatx executable
# ... 
```

--8<-- "tips/tip-chatx-requires-mdx.md"

??? tip "How do I install or build the `mdx` CLI"

    ChatX requires the `mdx` CLI to run. You can either install it from NuGet or build it from source.  

    === "Install `mdx` CLI"

        ```bash
        dotnet tool install -g mdx --prerelease
        ```
    
        After installation, you can verify that MDX is available by running:

        --8<-- "tips/mdx-cli-verify-version-info.md"

    === "Build `mdx` from source"

        ```bash
        # Clone the repository
        git clone https://github.com/robch/mdx.git
        cd mdx

        # Build the project
        dotnet build

        # Update your PATH to include the mdx executable
        # ... 
        ```

        After building, you can verify that MDX is available by running:

        --8<-- "tips/mdx-cli-verify-version-info.md"
