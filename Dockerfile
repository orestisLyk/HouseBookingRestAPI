# Build Stage
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy csproj and restore dependencies
COPY ["HouseBookingRestApi/HouseBookingRestApi.csproj", "HouseBookingRestApi/"]
RUN dotnet restore "HouseBookingRestApi/HouseBookingRestApi.csproj"

# Copy everything else and build
COPY . .
WORKDIR "/src/HouseBookingRestApi"
RUN dotnet build "HouseBookingRestApi.csproj" -c Release -o /app/build

# Publish Stage
FROM build AS publish
RUN dotnet publish "HouseBookingRestApi.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime Stage
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
EXPOSE 8080
COPY --from=publish /app/publish .
COPY .env .env
ENTRYPOINT ["dotnet", "HouseBookingRestApi.dll"]