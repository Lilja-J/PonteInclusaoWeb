# Dockerfile
# Use the official ASP.NET Core runtime as a parent image
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080

# Use the SDK image to build the app
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ["PonteInclusaoWeb.csproj", "."]
RUN dotnet restore "PonteInclusaoWeb.csproj"
COPY . .
WORKDIR "/src/."
RUN dotnet build "PonteInclusaoWeb.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "PonteInclusaoWeb.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Build the final image
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "PonteInclusaoWeb.dll"]