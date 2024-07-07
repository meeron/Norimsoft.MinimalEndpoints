build:
	dotnet restore src/Norimsoft.MinimalEndpoints/Norimsoft.MinimalEndpoints.csproj
	dotnet build src/Norimsoft.MinimalEndpoints/Norimsoft.MinimalEndpoints.csproj -c $(c)

pack: build
	dotnet pack src/Norimsoft.MinimalEndpoints/Norimsoft.MinimalEndpoints.csproj --no-build -o ./nugets -c $(c)
