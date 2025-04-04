# Use official ASP.NET Core runtime as the base image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

# Use the SDK image to build the app
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["OnBoarding.csproj", "./"]
RUN dotnet restore "./OnBoarding.csproj"

COPY . .
WORKDIR "/src"
RUN dotnet publish "./OnBoarding.csproj" -c Release -o /app/publish

# Final runtime image
FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
CMD ["dotnet", "OnBoarding.dll"]
