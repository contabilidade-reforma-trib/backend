# Deploy — backend no Railway

## 1. Configuração do serviço

| Campo | Valor |
|---|---|
| Repositório | `contabilidade-reforma-trib/backend` |
| Root Directory | *(vazio — o repositório já é o backend)* |
| Build | detectado automaticamente pelo Nixpacks (.NET 10) |
| Start Command | `dotnet src/Praxis.Api/bin/Release/net10.0/Praxis.Api.dll` *(só se a detecção falhar)* |

O Railway injeta a porta em `PORT`. O ASP.NET Core lê `ASPNETCORE_URLS` — por isso a variável abaixo é obrigatória, senão a aplicação sobe na 5000 e o roteador não a encontra.

## 2. Variáveis de ambiente

O ASP.NET Core lê configuração aninhada com **dois underscores** no lugar do `:`. Ou seja, `ConnectionStrings:Principal` vira `ConnectionStrings__Principal`.

### Obrigatórias

| Variável | Valor |
|---|---|
| `ASPNETCORE_URLS` | `http://0.0.0.0:${PORT}` |
| `ASPNETCORE_ENVIRONMENT` | `Production` |
| `ConnectionStrings__Principal` | `Host=...neon.tech;Database=neondb;Username=...;Password=...;SSL Mode=Require` |
| `Cors__OrigensPermitidas__0` | `https://<seu-projeto>.vercel.app` |

> **CORS é lista indexada.** Cada origem é uma variável própria: `Cors__OrigensPermitidas__0`, `__1`, `__2`. Sem nenhuma configurada a API aceita qualquer origem — bom para destravar a POC, ruim para deixar assim.
>
> Os domínios de *preview* da Vercel mudam a cada deploy. Para testar preview, adicione o domínio específico como mais um índice.

### Armazenamento (Cloudflare R2)

| Variável | Valor |
|---|---|
| `ArmazenamentoObjetos__AccountId` | id da conta Cloudflare |
| `ArmazenamentoObjetos__Endpoint` | `https://<account-id>.r2.cloudflarestorage.com` |
| `ArmazenamentoObjetos__AccessKeyId` | access key do token R2 |
| `ArmazenamentoObjetos__SecretAccessKey` | secret do token R2 |
| `ArmazenamentoObjetos__BucketVideos` | `praxis-videos` |
| `ArmazenamentoObjetos__BucketDocumentos` | `praxis-documentos` |
| `ArmazenamentoObjetos__Regiao` | `auto` |

Sem essas, a API **sobe do mesmo jeito** e o healthcheck reporta `armazenamento.configurado: false`. É deliberado: não faz sentido derrubar a API inteira porque o storage de vídeo ainda não existe.

### Ainda não usadas — deixe vazias até contratar

| Variável | Observação |
|---|---|
| `Ia__ApiKey` | chave da OpenAI (crédito de API, não de assinatura do ChatGPT) |
| `Transcricao__ApiKey` | Whisper, em espera |
| `ConnectionStrings__TestesIntegracao` | só faz sentido em CI |

## 3. Migrations

O deploy **não** aplica migrations sozinho. Enquanto é POC, aplique da sua máquina apontando para o banco de produção:

```bash
dotnet ef database update --project src/Praxis.Modules --startup-project src/Praxis.Api
```

Automatizar isso no start da aplicação é tentador e perigoso: duas instâncias subindo juntas aplicariam a mesma migration em paralelo. Fica como item de backlog.

## 4. Conferindo que subiu

```bash
curl https://<seu-app>.up.railway.app/api/saude
```

Resposta esperada:

```json
{
  "status": "ok",
  "servico": "praxis-api",
  "banco": { "conectado": true, "latenciaMs": 40, "erro": null },
  "armazenamento": { "configurado": true }
}
```

`status: "degradado"` significa que a API está no ar mas o banco não respondeu — a diferença importa, e é por isso que o endpoint devolve 200 nos dois casos em vez de derrubar a checagem.

A documentação da API fica em `https://<seu-app>.up.railway.app/swagger`, ligada também em produção porque é POC e quem integra o front precisa enxergar o contrato.

## 5. Quando deixar de ser POC

- Fechar o CORS na lista exata de domínios.
- Tirar o Swagger de produção, ou colocá-lo atrás de autenticação.
- Trocar todas as credenciais — as atuais circularam por chat e por WhatsApp.
- Aplicar migration por passo de deploy, não da máquina de alguém.
