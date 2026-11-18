FROM mcr.microsoft.com/dotnet/sdk:11.0 AS build
WORKDIR /src
COPY UserService.csproj ./
RUN dotnet restore
COPY . ./
RUN dotnet publish -c Release -o /app

FROM mcr.microsoft.com/dotnet/aspnet:11.0
WORKDIR /app
COPY --from=build /app ./
EXPOSE 8080
ENTRYPOINT ["dotnet", "UserService.dll"]
