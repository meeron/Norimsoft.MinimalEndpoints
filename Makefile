build:
	dotnet restore src/Norimsoft.MinimalEndpoints/Norimsoft.MinimalEndpoints.csproj
	dotnet build src/Norimsoft.MinimalEndpoints/Norimsoft.MinimalEndpoints.csproj -c $(c) -f net9.0
	dotnet build src/Norimsoft.MinimalEndpoints/Norimsoft.MinimalEndpoints.csproj -c $(c) -f net8.0

pack: build
	dotnet pack src/Norimsoft.MinimalEndpoints/Norimsoft.MinimalEndpoints.csproj --no-build -o ./nugets -c $(c)
