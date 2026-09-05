# Estado atual

Atualize este arquivo ao final de **toda** sessão. É por aqui que a próxima sessão — sua, minha ou de outra IA — descobre onde a coisa parou sem ter que ler o histórico de conversa.

**Última atualização:** 2026-09-05

## Onde estamos

API de pé, conectada ao Neon, com as entidades de Identidade e Assinaturas, Swagger e healthcheck. **24 testes verdes** (18 de unidade, 6 de integração). Nenhum endpoint de negócio ainda — só saúde.

## Stack

| Peça | Versão | Observação |
|---|---|---|
| .NET | 10 | Solução em `Praxis.slnx` (formato novo) |
| EF Core + Npgsql | 10.0.11 | Migration `Inicial` já aplicada no Neon |
| Swashbuckle | — | Swagger UI em `/swagger`, ligado também em produção |
| AWSSDK.S3 | — | Falando com o Cloudflare R2, que usa o protocolo S3 |
| xUnit | 2.9.3 | `FatoDeIntegracao` pula o teste quando não há banco configurado |

## Já existe

**Compartilhado (`Praxis.Shared`)**
- `Result` / `Result<T>` e `Erro` — falha esperada não vira exceção
- `IRelogio` + `RelogioDoSistema` — tempo injetado, testável
- `EntidadeBase` — Id, CriadoEm, AtualizadoEm em UTC
- `IArmazenamentoDeObjetos` + `ArmazenamentoEmR2` — envio, URL assinada de leitura e de escrita, existência, remoção
- `OpcoesDeArmazenamento.EstaConfigurado` — sem credencial a API sobe mesmo assim

**Identidade**
- `Organizacao` — valida CPF/CNPJ, impede e-mail duplicado na organização
- `Usuario` — normaliza e-mail, papel, registro profissional, telefone
- `PerfilDeUso` — área, regime, setores e dor atual: as quatro perguntas do cadastro
- `IRepositorioDeOrganizacoes` + implementação

**Assinaturas**
- `Assinatura`, `DireitoDeUso`, `Pagamento`, com `Produto`, `SituacaoDaAssinatura`, `MeioDePagamento`
- Regra: conceder acesso a produto já vigente **estende** a vigência em vez de criar direito duplicado
- `IConsultaDeDireitoDeUso` — contrato público, devolve DTO, é a única porta do módulo
- `IRepositorioDeAssinaturas` + implementação

**Persistência**
- `PraxisDbContext` com schema configurável — é o que permite o teste isolado
- Mapeamento por módulo, tabelas prefixadas: `identidade_*`, `assinaturas_*`
- Sem chave estrangeira entre módulos, de propósito: a fronteira vale também no schema
- Migration `Inicial`, aplicada no Neon

**API**
- `GET /api/saude` — status, versão, ambiente, banco (conectado + latência) e armazenamento
- `GET /api/saude/ping` — verificação mínima, não toca no banco
- Swagger em `/swagger`; a raiz redireciona para lá
- CORS existe como escape, mas **não é usado**: o front chega pelo BFF do Next (D-14). Lista vazia = nenhuma origem permitida

**Testes**
- 18 de unidade: validação de organização e usuário, vigência, extensão de direito, pagamento
- 6 de integração em schema isolado no Neon, derrubado ao final, com varredura de órfãos acima de 24h
- Suíte roda duas vezes seguidas sem sujeira — verificado

## Ainda não existe

- Autenticação — nenhum endpoint é protegido
- Endpoints de negócio: cadastro, compra simulada, perfil
- Módulos `Copiloto` e `Mentoria` — pastas ainda vazias
- `IProvedorDeIa` — depende de chave de IA
- Esteira de ingestão e pgvector
- Aplicação de migration no deploy; hoje é manual, da máquina de quem desenvolve

## Como rodar

```bash
dotnet run --project src/Praxis.Api --urls http://localhost:5000
```

```bash
dotnet test Praxis.slnx
```

Swagger em `http://localhost:5000/swagger`. Deploy e variáveis de ambiente em [deploy.md](deploy.md).

## Travas conhecidas

| Trava | Efeito |
|---|---|
| Sem chave de IA | Copiloto não sai do papel; a abstração pode ser escrita e testada com implementação falsa |
| Whisper em espera | Transcrição fora da esteira |
| Entrega de vídeo indefinida (D-08) | O R2 guarda o arquivo, mas transcode e HLS continuam em aberto |
| Credenciais descartáveis | Circularam por chat; trocar antes de qualquer uso real |
