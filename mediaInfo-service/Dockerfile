#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["_MediaInfo.Service/_MediaInfo.Service.csproj", "_MediaInfo.Service/"]
RUN dotnet restore "_MediaInfo.Service/_MediaInfo.Service.csproj"
COPY . .
WORKDIR "/src/_MediaInfo.Service"
RUN dotnet build "_MediaInfo.Service.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "_MediaInfo.Service.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "_MediaInfo.Service.dll"]