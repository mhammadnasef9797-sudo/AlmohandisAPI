# 1. Use the official .NET SDK image to build the application
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# Copy csproj and restore dependencies
COPY *.csproj ./
RUN dotnet restore

# Copy the rest of the application files
COPY . ./
WORKDIR /app
RUN dotnet publish -c Release -o out

# 2. Use the official .NET runtime image to run the application
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/out ./

# Expose the port the app runs on
EXPOSE 10000

# Define environment variables
ENV ASPNETCORE_URLS=http://+:10000

# The command to run the application
ENTRYPOINT ["dotnet", "AlmohandisAPI.dll"]