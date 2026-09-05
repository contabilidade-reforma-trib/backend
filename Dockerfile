# Build explícito em vez de detecção automática.
#
# O Railpack, detector padrão do Railway, procura um .csproj ou .sln na raiz do
# repositório. Aqui a raiz tem Praxis.slnx (formato novo, que ele ainda não
# reconhece) e os projetos vivem em src/ — então a detecção falha antes de
# começar. Com Dockerfile o Railway usa este arquivo e a detecção sai do caminho.

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Os .csproj vêm primeiro e sozinhos: assim a camada de restore só é refeita
# quando uma dependência muda, e não a cada alteração de código.
COPY src/Praxis.Shared/Praxis.Shared.csproj src/Praxis.Shared/
COPY src/Praxis.Modules/Praxis.Modules.csproj src/Praxis.Modules/
COPY src/Praxis.Api/Praxis.Api.csproj src/Praxis.Api/
RUN dotnet restore src/Praxis.Api/Praxis.Api.csproj

COPY src/ src/
RUN dotnet publish src/Praxis.Api/Praxis.Api.csproj \
    -c Release \
    -o /app/publish \
    --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .

ENV ASPNETCORE_ENVIRONMENT=Production
EXPOSE 8080

# A porta é lida de PORT em tempo de execução, com 8080 como padrão. O Railway
# injeta PORT dinamicamente, e resolver isso aqui evita depender de interpolação
# de variável na plataforma — que é onde esse tipo de deploy costuma quebrar.
ENTRYPOINT ["sh", "-c", "dotnet Praxis.Api.dll --urls http://0.0.0.0:${PORT:-8080}"]
