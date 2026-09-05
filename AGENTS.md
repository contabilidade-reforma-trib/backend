# Praxis — Backend

**Este é o arquivo canônico de regras deste repositório.** Vale para qualquer ferramenta de IA — Codex, Cursor, Copilot, Claude Code, Gemini, Windsurf — e para qualquer pessoa. `CLAUDE.md` e `.github/copilot-instructions.md` apenas apontam para cá; não há uma segunda cópia das regras, de propósito: duas cópias divergem.

Contexto obrigatório para qualquer sessão de IA neste repositório. Leia antes de escrever a primeira linha.

Primeira vez no projeto? Comece por **[COMECE-AQUI.md](COMECE-AQUI.md)** — instalação, credenciais, hooks e prompt de abertura para IA.

Ative os hooks de git uma vez por clone:

```bash
git config core.hooksPath .githooks
```

Documentos complementares: [produto](docs/produto.md) · [arquitetura](docs/arquitetura.md) · [regras de desenvolvimento](docs/regras-desenvolvimento.md) · [glossário](docs/glossario.md) · [decisões](docs/decisoes.md) · [estado atual](docs/estado-atual.md) · [em andamento](docs/em-andamento.md) · [backlog](docs/backlog.md)

**Comece toda sessão lendo [docs/em-andamento.md](docs/em-andamento.md)** — é onde está a feature em curso e em que passo ela parou.

---

## 1. O que é o sistema

Plataforma para contadores brasileiros, especializada na **Reforma Tributária do consumo** (IBS, CBS e Imposto Seletivo). São **dois produtos vendidos separadamente**, sob o mesmo login:

1. **Copiloto** — assistente de IA (RAG) que responde dúvidas práticas de contabilidade tributária a partir da documentação e da experiência das mentoras. Responde *como resolver*, não *o que a lei diz*.
2. **Mentoria** — plataforma de ensino em vídeo (trilhas → módulos → aulas), com material de apoio. Ensina o **assunto**, não o uso do copiloto.

O cliente pode comprar **só o copiloto**, **só a mentoria**, **os dois juntos**, ou comprar um e adicionar o outro depois. Nenhum dos dois depende do outro para funcionar.

O usuário final é o contador. O cliente do contador nunca acessa a plataforma e não sabe que ela existe.

## 2. Regras inegociáveis

Estas regras valem para toda alteração, sem exceção. Se uma delas atrapalhar a tarefa, **pare e pergunte** — não contorne.

### 2.1 Teste é parte da entrega, não etapa seguinte

- **Toda** alteração — feature nova, correção de bug, refatoração — entra com **teste de unidade**. Sem teste, a tarefa não está pronta.
- Correção de bug começa pelo teste que reproduz o bug falhando.
- **Sempre que o comportamento tocar banco, storage ou serviço externo**, entra também **teste de integração**.
- Teste de integração usa **schema isolado por execução** no Neon (`teste_<guid>`), criado no início e derrubado no fim. A limpeza roda em `finally` / `IAsyncLifetime.DisposeAsync`, de modo que **um teste que falha no meio ainda derruba o schema**. Detalhes em [regras de desenvolvimento](docs/regras-desenvolvimento.md).
- Nunca aponte teste para o schema `public` do banco principal.

### 2.2 Peça revisão explícita antes de mexer no núcleo

Antes de alterar qualquer coisa desta lista, **pare, descreva o que pretende fazer e peça revisão explícita ao usuário**. Só siga após resposta.

- Regra de negócio de qualquer módulo (cálculo, elegibilidade, liberação de acesso, apuração).
- Modelo de dados: entidade nova, remoção de campo, mudança de relacionamento, migration destrutiva.
- Contrato público entre módulos, ou contrato de API já consumido pelo front.
- Regras de acesso: quem enxerga o quê, o que cada assinatura libera.
- Prompt do copiloto, estratégia de recuperação (RAG) ou qualquer coisa que mude a resposta ao contador.
- Troca de biblioteca, provedor ou infraestrutura.

Alteração cosmética, ajuste de teste, log, rename local e correção óbvia não precisam disso.

### 2.3 Nomes dizem o que a coisa faz

- Nome de classe, método, variável e tabela deve ser legível por quem nunca viu o código. `CalcularNecessidadeDeGiroNoMesDeVirada` vence `CalcGiro`.
- Nada de abreviação inventada, `Helper`, `Manager`, `Util`, `Service` genérico, `data`, `obj`, `tmp`.
- **Idioma:** o domínio é escrito em **português** (`Trilha`, `Aula`, `Assinatura`, `SaldoCredor`, `Aliquota`), porque os termos são intraduzíveis sem perda. Termos técnicos e de infraestrutura ficam em **inglês** (`Repository`, `Handler`, `CancellationToken`, `Endpoint`). Não misture no mesmo identificador.
- Boolean começa com `Esta`, `Possui`, `Deve`, `Pode` (`PossuiAssinaturaAtiva`).

### 2.4 Escalabilidade não é otimização prematura aqui

- Todo IO é `async` com `CancellationToken` propagado até o fim.
- Nada de consulta em laço (N+1). Paginação obrigatória em qualquer listagem que possa crescer.
- Nenhuma operação longa (transcode, transcrição, indexação) roda dentro do request HTTP — vai para fila/worker.
- Custo de IA é medido e gravado por consulta e por usuário desde o primeiro dia (ver [decisões](docs/decisoes.md), D-07).

### 2.5 Segredo não entra no repositório

- Credenciais ficam em `appsettings.Development.json` (ignorado pelo git) e, em produção, nas variáveis de ambiente do Railway.
- `appsettings.json` é versionado e só tem chaves vazias.
- Se você precisar de um segredo novo, adicione a **chave vazia** no `appsettings.json` e peça o valor ao usuário.

### 2.6 Antes de commitar: revisão obrigatória

Quando o commit é feito por IA, ele **não** é feito direto. Antes de `git commit`:

1. Faça a revisão do que está preparado (`git diff --staged`) seguindo **[docs/revisao-pre-commit.md](docs/revisao-pre-commit.md)**. No Claude Code há a skill `revisao-pre-commit` como atalho; em qualquer outra ferramenta, abra o documento e siga.
2. A revisão avalia: separação de responsabilidade, centralização do que se repete, **simplificação (KISS)**, legibilidade e nomenclatura, e aderência às regras deste arquivo.
3. **Relate os achados à pessoa que está desenvolvendo**, com arquivo e linha, antes de commitar. Não commite silenciosamente por cima de um achado.
4. Achado grave (regra de negócio no lugar errado, módulo lendo tabela de outro, duplicação de regra, nome que engana) **bloqueia o commit** até ser resolvido ou dispensado explicitamente pela pessoa.

A revisão é sobre o diff preparado, não sobre o repositório inteiro.

### 2.7 Antes de dar push: suíte inteira verde

Quando o push é feito por IA, antes de `git push`:

1. Rode **toda** a suíte — unidade **e** integração, do repositório inteiro, não só os testes da feature em questão.

```bash
dotnet test
```

2. Push só acontece com **tudo** verde. Um único teste vermelho, mesmo em área não tocada, cancela o push.
3. Se algo alheio à sua alteração quebrou, **avise a pessoa** e pergunte o que fazer. Não pule o teste, não marque como ignorado, não empurre para depois.

## 3. Arquitetura em uma tela

Monolito modular. Um processo, módulos com fronteira real.

```
backend/
  Praxis.slnx
  src/
    Praxis.Api/            camada de API: endpoints, DI, middleware, autenticação
    Praxis.Modules/        um projeto, uma pasta por contexto delimitado
      Identidade/          conta, organização, usuários, acesso
      Assinaturas/         planos, compra, o que cada assinatura libera
      Copiloto/            consultas, RAG, fontes, custo
      Mentoria/            trilhas, módulos, aulas, progresso, materiais
        Domain/            entidades, regras, invariantes — não referencia nada de fora
        Application/       casos de uso, orquestração, contratos
        Infrastructure/    banco, storage, serviços externos
    Praxis.Shared/         kernel comum: tipos base, Result, abstrações de infra
  tests/
    Praxis.UnitTests/
    Praxis.IntegrationTests/
```

Direção das dependências, sempre: `Api → Application → Domain` e `Infrastructure → Application/Domain`. **`Domain` não referencia `Application`, `Infrastructure` nem pacote de framework.**

Um módulo **nunca** lê tabela de outro módulo.

**A única porta entre módulos é a camada `Application`.** Concretamente:

- O módulo dono expõe, em `Application`, um **contrato público**: `QueryService` para leitura e caso de uso para escrita, trocando **DTO**.
- O consumidor injeta a interface desse contrato. Não referencia `Domain` nem `Infrastructure` do outro módulo.
- **Entidade de domínio nunca atravessa a fronteira.** Sai DTO, entra DTO.
- Nada de `JOIN`, view ou `DbContext` cruzando módulo. Cada módulo é dono das suas tabelas.

Exemplo: `Copiloto` precisa saber se a organização pode consultar. Ele injeta `IConsultaDeDireitoDeUso` (exposta por `Assinaturas.Application`), recebe um DTO e segue. Ele não conhece a tabela, a entidade nem a regra de vigência — e não guarda cópia da resposta.

### 3.1 Acesso a dados: EF Core primeiro

- **EF Core é o padrão.** Escrita, leitura, migration, tudo.
- **Dapper é exceção**, só quando o EF tornaria a consulta inviável ou absurdamente ineficiente — e o motivo vai escrito em comentário na própria consulta.
- Toda consulta Dapper **referencia os nomes por `nameof`**, nunca por string solta:

```csharp
// Dapper: agregação com window function que o EF não traduz.
var sql = $"""
    SELECT {nameof(Aula.Id)}, {nameof(Aula.Titulo)}
    FROM mentoria_aula
    WHERE {nameof(Aula.ModuloId)} = @moduloId
    """;
```

O motivo é direto: renomear uma propriedade tem que quebrar a compilação da consulta, não virar erro em produção meses depois. Consulta Dapper com nome em string crua é achado que bloqueia commit.

## 4. Rodando

```bash
dotnet build
```

```bash
dotnet test
```

```bash
dotnet run --project src/Praxis.Api
```

## 5. Antes de encerrar qualquer tarefa

1. `dotnet build` sem warning novo.
2. `dotnet test` verde.
3. Teste de unidade escrito para o que mudou.
4. Teste de integração escrito, se tocou banco/storage/externo.
5. [docs/em-andamento.md](docs/em-andamento.md) com os passos atualizados — o que foi feito, o que está em curso, o que falta.
6. [docs/estado-atual.md](docs/estado-atual.md) atualizado com o que entrou.
7. [docs/decisoes.md](docs/decisoes.md) atualizado, se alguma decisão foi tomada.
8. [docs/backlog.md](docs/backlog.md) atualizado, se algo novo foi descoberto ou concluído.
9. Nada de segredo no diff.

Não faça commit nem push sem o usuário pedir. Quando pedir:

- **Commit** → revisão de [docs/revisao-pre-commit.md](docs/revisao-pre-commit.md) antes, achados relatados (§2.6).
- **Push** → suíte inteira verde antes (§2.7).
