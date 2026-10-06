FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ["EnglishLearning.Api/EnglishLearning.Api.csproj", "EnglishLearning.Api/"]
COPY ["EnglishLearning.Application/EnglishLearning.Application.csproj", "EnglishLearning.Application/"]
COPY ["EnglishLearning.Domain/EnglishLearning.Domain.csproj", "EnglishLearning.Domain/"]
COPY ["EnglishLearning.Infrastructure/EnglishLearning.Infrastructure.csproj", "EnglishLearning.Infrastructure/"]
RUN dotnet restore "EnglishLearning.Api/EnglishLearning.Api.csproj"
COPY . .
WORKDIR "/src/EnglishLearning.Api"
RUN dotnet build "EnglishLearning.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "EnglishLearning.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "EnglishLearning.Api.dll"]