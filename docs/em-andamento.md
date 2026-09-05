# Em andamento — backend

**Leia este arquivo no começo de toda sessão.** É onde está a feature em curso e em que passo ela parou.

Regra: **uma feature por vez.** Se precisar começar outra antes de terminar esta, mova a atual para "Pausadas" com o motivo, em vez de deixar duas meio-feitas.

Marcação: `[x]` feito · `[>]` em execução agora · `[ ]` ainda não · `[!]` travado

---

## Feature atual

**Nenhuma.** A fundação e as entidades básicas ficaram prontas em 2026-09-05.

Próxima sugerida: **autenticação e os primeiros endpoints de negócio** (cadastro de organização + compra simulada). Sem autenticação, nenhum endpoint pode verificar direito de uso, e a regra "acesso é verificado a cada requisição" fica no papel.

---

## Concluídas recentemente

### Fundação e entidades básicas — 2026-09-05

Objetivo: sair do esqueleto e ter API conectada ao banco, com as entidades que
sustentam "usuário com dados cadastrais e qual plano".

```
[x] Praxis.Shared: Result, Erro, IRelogio, EntidadeBase
[x] IArmazenamentoDeObjetos + ArmazenamentoEmR2 (Cloudflare R2 via protocolo S3)
[x] Identidade: Organizacao, Usuario, PerfilDeUso, AreaDeAtuacao, RegimeTributario
[x] Assinaturas: Assinatura, DireitoDeUso, Pagamento e enums
[x] Contrato público IConsultaDeDireitoDeUso, trocando DTO
[x] Repositórios com EF Core
[x] PraxisDbContext com schema configurável
[x] Migration Inicial, aplicada no Neon
[x] Swagger (Swashbuckle) e CORS por configuração
[x] GET /api/saude e /api/saude/ping
[x] 18 testes de unidade
[x] BancoDeTesteFixture com schema isolado e varredura de órfãos
[x] 6 testes de integração; suíte roda duas vezes sem sujeira
[x] docs/deploy.md com as variáveis do Railway
```

Decisões tomadas no caminho, todas registradas em [decisoes.md](decisoes.md):

- **Vigência sobreposta soma, não substitui.** Quem assina o Combo já tendo
  Copiloto ativo estende a data em vez de perder o que pagou. Sem isso,
  apareceriam dois direitos concorrentes para o mesmo produto e a leitura de
  acesso viraria loteria.
- **Sem chave estrangeira entre módulos.** `assinaturas_assinatura` referencia a
  organização por identificador, não por FK, para que a fronteira valha também
  no schema do banco.
- **A API sobe sem credencial de storage.** O healthcheck reporta
  `armazenamento.configurado: false` em vez de derrubar tudo — não faz sentido a
  API inteira cair porque o vídeo ainda não tem credencial.
- **Healthcheck responde 200 mesmo com o banco fora.** Quem chama precisa
  distinguir "a API não respondeu" de "a API respondeu e o banco caiu".
- **Teste de integração cria tabelas por `GenerateCreateScript`,** não por
  `EnsureCreated` nem por migration — o porquê está em [teste-integracao.md](teste-integracao.md).

---

## Pausadas

Nenhuma.
