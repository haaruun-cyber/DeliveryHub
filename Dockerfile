# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

WORKDIR /src

# Copy project files and restore as distinct layers
COPY ["DeliveryHub.API/DeliveryHub.API.csproj", "DeliveryHub.API/"]
COPY ["DeliveryHub.Application/DeliveryHub.Application.csproj", "DeliveryHub.Application/"]
COPY ["DeliveryHub.Domain/DeliveryHub.Domain.csproj", "DeliveryHub.Domain/"]
COPY ["DeliveryHub.Infrastructure/DeliveryHub.Infrastructure.csproj", "DeliveryHub.Infrastructure/"]

RUN dotnet restore "DeliveryHub.API/DeliveryHub.API.csproj"

# Copy all the source code
COPY . .

WORKDIR /src/DeliveryHub.API
RUN dotnet publish "DeliveryHub.API.csproj" -c Release -o /app/publish

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0

WORKDIR /app

COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:10000
EXPOSE 10000

ENTRYPOINT ["dotnet", "DeliveryHub.API.dll"]
