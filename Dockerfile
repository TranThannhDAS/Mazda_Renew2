# See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

# Use the SDK image for building
FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src

# Install libgdiplus
RUN apt-get update \
    && apt-get install -y -q libgdiplus \
    && apt-get clean \
    && rm -rf /var/lib/apt/lists/*

COPY ["Mazda/Mazda.csproj", "Mazda/"]
RUN dotnet restore "Mazda/Mazda.csproj"
COPY . .

# Build the application
WORKDIR "/src/Mazda"
RUN dotnet build "Mazda.csproj" -c Release -o /app/build

# Publish the application
FROM build AS publish
RUN dotnet publish "Mazda.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Use the smaller runtime image for the final stage
FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Set the entry point
ENTRYPOINT ["dotnet", "Mazda.dll"]
