FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app
COPY /OrderProcessor.csproj ./
RUN dotnet restore
COPY . ./
RUN dotnet publish -c Release -o out


FROM mcr.microsoft.com/dotnet/runtime:8.0
WORKDIR /app
COPY --from=build /app/out ./
COPY appsettings.json ./
CMD ["dotnet", "OrderProcessor.dll"]
