# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy all csproj files to maintain Clean Architecture dependencies
COPY ["Api/Api.csproj", "Api/"]
COPY ["Application/Application.csproj", "Application/"]
COPY ["Domain/Domain.csproj", "Domain/"]
COPY ["Infrastructure/Infrastructure.csproj", "Infrastructure/"]

# Restore using the Api project as the entry point
RUN dotnet restore "Api/Api.csproj"

# Copy the entire source code
COPY . .

# Build the Api project
WORKDIR "/src/Api"
RUN dotnet publish "Api.csproj" -c Release -o /app/publish --no-restore

# Stage 2: Final Runtime
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Api.dll"]