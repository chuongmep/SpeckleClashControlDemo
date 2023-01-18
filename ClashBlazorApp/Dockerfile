FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["ClashBlazorApp/ClashBlazorApp.csproj", "ClashBlazorApp/"]
RUN dotnet restore "ClashBlazorApp/ClashBlazorApp.csproj"
COPY . .
WORKDIR "/src/ClashBlazorApp"
RUN dotnet build "ClashBlazorApp.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "ClashBlazorApp.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ClashBlazorApp.dll"]
