# Estado atual

Atualize este arquivo ao final de **toda** sessão. É por aqui que a próxima sessão — sua, minha ou de outra IA — descobre onde a coisa parou sem ter que ler o histórico de conversa.

**Última atualização:** 2026-09-05

## Onde estamos

API de pé com **`User`** e a **estrutura de RAG** funcionando ponta a ponta contra o Neon. **24 testes verdes** (17 de unidade, 7 de integração). Código todo em inglês (D-15), domínio deliberadamente mínimo até o negócio estar claro (D-16).

## Estrutura

Um projeto por camada de cada módulo — a fronteira é imposta pelo compilador (D-19).

```
src/
  Praxis.Api/                    endpoints, DI, Swagger
  Praxis.Shared/                 Result, IClock, IAiProvider, IObjectStorage, R2ObjectStorage
  Modules/
    Identity/                    Praxis.Identity.{Domain,Application,Infrastructure}
    Copilot/                     Praxis.Copilot.{Domain,Application,Infrastructure}
tests/
  Praxis.UnitTests/  Praxis.IntegrationTests/
```

**Um DbContext por módulo**, mesmo banco, tabela de histórico de migration própria para cada.

## Stack

| Peça | Versão | Observação |
|---|---|---|
| .NET | 10 | Solução em `Praxis.slnx` |
| EF Core + Npgsql | 10.0.11 | Migration `Initial` por contexto, aplicada no Neon |
| pgvector | — | `Pgvector.EntityFrameworkCore`, coluna `vector(1536)` |
| Swashbuckle | — | Swagger em `/swagger`, ligado também em produção |
| AWSSDK.S3 | — | Cloudflare R2 pelo protocolo S3 |
| xUnit | 2.9.3 | `IntegrationFact` pula o teste sem banco configurado |

## Já existe

**Identity** — `User` com nome, e-mail (único, normalizado) e telefone. `IUserRepository` com paginação.

**Copilot (RAG)** — `KnowledgeDocument` + `DocumentChunk` com embedding em pgvector. `IngestDocument` (fatia, gera embedding, grava; reindexar substitui em vez de duplicar) e `SearchKnowledge` (pergunta → vetor → vizinhos mais próximos, filtrando vigência e status). Vigência (`ValidFrom`/`ValidUntil`) já modelada e filtrada na consulta.

**IA** — `IAiProvider` com `DeterministicAiProvider` de reserva: vetores derivados de hash, centrados em zero. Roda a esteira inteira sem chave e sem custo. **Não** prova que a recuperação acha a passagem certa — isso exige embeddings reais.

**Armazenamento** — `IObjectStorage` + `R2ObjectStorage`: envio, URL assinada de leitura e de escrita, existência, remoção.

**API** — `/api/health`, `/api/health/ping`, `POST|GET /api/users`, `GET /api/users/{id}`, `POST /api/knowledge/documents`, `POST /api/knowledge/search`, `POST /api/videos/upload-url`, `GET /api/videos/playback-url`. Todos no Swagger.

**Testes** — 17 de unidade (validação de usuário, fatiamento, vigência, reindexação, determinismo do provedor) e 7 de integração em schema isolado no Neon, derrubado ao final, com varredura de órfãos.

## Ainda não existe

- Autenticação — nenhum endpoint é protegido
- Provedor de IA real — `Ai:ApiKey` preenchida hoje faz a API falhar no boot, de propósito
- Organização, assinatura, mentoria — voltam quando o domínio estiver definido (D-16)
- Ingestão a partir de arquivo: hoje o texto vai no corpo da requisição
- Aplicação de migration no deploy; hoje é manual

## Como rodar

```bash
dotnet run --project src/Praxis.Api --urls http://localhost:5000
```

```bash
dotnet test Praxis.slnx
```

Swagger em `http://localhost:5000/swagger`. Deploy e variáveis em [deploy.md](deploy.md); Cloudflare em [cloudflare.md](cloudflare.md).

## Travas conhecidas

| Trava | Efeito |
|---|---|
| Sem chave de IA | Recuperação roda, mas com vetores de reserva; qualidade não é mensurável |
| Sem conjunto de perguntas-gabarito | Não há como afirmar que a recuperação melhorou ou piorou |
| Whisper em espera | Transcrição de aula fora da esteira |
| Stream não contratado | Vídeo fica no R2, sem qualidade adaptativa (ver cloudflare.md) |
| Credenciais descartáveis | Circularam por chat; trocar antes de qualquer uso real |
