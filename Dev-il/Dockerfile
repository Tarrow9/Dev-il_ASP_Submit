#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
#TZ KST
ENV TZ=Asia/Seoul
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["bangsoo.csproj", "."]
RUN dotnet restore "./bangsoo.csproj"
COPY . .
WORKDIR "/src/."
RUN dotnet build "bangsoo.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "bangsoo.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "bangsoo.dll"]
