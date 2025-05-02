dotnet restore
dotnet build --configuration Release --no-restore

dotnet publish src/cycod/cycod.csproj -c Release -r win-x64
dotnet publish src/cycod/cycod.csproj -c Release -r linux-x64
dotnet publish src/cycod/cycod.csproj -c Release -r osx-x64
dotnet build src/cycod/cycod.csproj -c Release
dotnet pack src/cycod/cycod.csproj --configuration Release --no-build -o nuget-packages

dotnet publish src/cycodt/cycodt.csproj -c Release -r win-x64
dotnet publish src/cycodt/cycodt.csproj -c Release -r linux-x64
dotnet publish src/cycodt/cycodt.csproj -c Release -r osx-x64
dotnet build src/cycodt/cycodt.csproj -c Release
dotnet pack src/cycodt/cycodt.csproj --configuration Release --no-build -o nuget-packages

dotnet publish src/cycodmd/cycodmd.csproj -c Release -r win-x64
dotnet publish src/cycodmd/cycodmd.csproj -c Release -r linux-x64
dotnet publish src/cycodmd/cycodmd.csproj -c Release -r osx-x64
dotnet build src/cycodmd/cycodmd.csproj -c Release
dotnet pack src/cycodmd/cycodmd.csproj --configuration Release --no-build -o nuget-packages
