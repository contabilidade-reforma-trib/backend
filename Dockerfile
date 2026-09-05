# Build explícito em vez de detecção automática.
#
# O Railpack, detector padrão do Railway, procura um .csproj ou .sln na raiz do
# repositório. Aqui a raiz tem Praxis.slnx (formato novo, que ele ainda não
# reconhece) e os projetos vivem em src/. Com Dockerfile o Railway usa este
# arquivo e a detecção sai do caminho.

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copiar o código inteiro e restaurar de uma vez.
#
# A versão anterior copiava cada .csproj antes, para o restore virar uma camada
# de cache. Com um projeto por camada de cada módulo isso vira uma lista que
# precisa ser editada a cada módulo novo — e foi exatamente o que quebrou o
# deploy quando a estrutura mudou. Trocamos alguns segundos de build por um
# arquivo que não sabe quantos projetos existem.
COPY src/ src/

RUN dotnet publish src/Praxis.Api/Praxis.Api.csproj \
    -c Release \
    -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .

ENV ASPNETCORE_ENVIRONMENT=Production
EXPOSE 8080

# A porta é lida de PORT em tempo de execução, com 8080 como padrão. O Railway
# injeta PORT dinamicamente, e resolver isso aqui evita depender de interpolação
# de variável na plataforma — que é onde esse tipo de deploy costuma quebrar.
ENTRYPOINT ["sh", "-c", "dotnet Praxis.Api.dll --urls http://0.0.0.0:${PORT:-8080}"]
