# Comece aqui — backend

Guia de entrada para quem vai desenvolver neste repositório, com ou sem assistente de IA.

## 1. Como a pasta de trabalho é organizada

```
Sistema Contabilidade Tributaria/   ← pasta comum, SEM git próprio
  backend/                          ← este repositório (.NET, Railway)
  frontend/                         ← repositório separado (Next.js, Vercel)
```

Dois repositórios independentes, lado a lado. **A pasta que os contém não é um repositório** — não existe nada versionado nela, e nunca rode `git init` ali. Todo comando git roda dentro de `backend/` ou de `frontend/`, e commit e push são sempre por repositório.

**Abra sua ferramenta na pasta de cima**, a que contém os dois, e não dentro de `backend/`. Você vai precisar do frontend como contexto, e eventualmente vai atravessar os dois numa mesma feature.

## 2. Pré-requisitos

| Ferramenta | Versão | Conferir com |
|---|---|---|
| .NET SDK | 10 | `dotnet --version` |
| Git | qualquer recente | `git --version` |

## 3. Preparar

```bash
git -C backend config core.hooksPath .githooks
```

**Obrigatório e não dá para pular**: liga o `pre-push`, que roda a suíte inteira antes de qualquer push. Configuração de git não é versionada, então cada clone precisa fazer isso uma vez.

```bash
dotnet build backend/Praxis.slnx
```

### Abrindo no Visual Studio

A solução é `backend/Praxis.slnx` — o **formato novo**, em XML. O diálogo "Abrir projeto/solução" filtra por `*.sln` por padrão, então **o arquivo não aparece na lista**. Escolha "Todos os arquivos" no filtro, ou digite `Praxis.slnx` no campo de nome. Também dá para arrastar o arquivo para dentro do VS.

Visual Studio 2022 anterior ao 17.14 pede que o suporte seja ligado em **Ferramentas → Opções → Ambiente → Recursos de Versão Prévia → "Usar o modelo de persistência de arquivo de solução"**. `dotnet build` e Rider abrem sem configuração nenhuma.

## 4. Credenciais — o passo que trava todo mundo

`src/Praxis.Api/appsettings.Development.json` está no `.gitignore` **de propósito**: carrega as credenciais de Neon e Cloudflare R2. **Num clone novo ele não existe**, e a aplicação não sobe sem ele.

Peça o arquivo à pessoa responsável pelo projeto, por canal privado. Nunca por commit, issue, pull request ou grupo de mensagem.

As chaves esperadas estão todas em `appsettings.json`, vazias — use como gabarito. Precisou de um segredo novo? Adicione a **chave vazia** no `appsettings.json` versionado e peça o valor.

## 5. Leia nesta ordem

1. **[AGENTS.md](AGENTS.md)** — regras obrigatórias. É o arquivo canônico deste repositório.
2. **[docs/produto.md](docs/produto.md)** — o que é o produto e como é vendido.
3. **[docs/arquitetura.md](docs/arquitetura.md)** — módulos, modelo de dados, esteira de ingestão, RAG.
4. **[docs/glossario.md](docs/glossario.md)** — termos do domínio tributário. Escreva-os exatamente assim.
5. **[docs/em-andamento.md](docs/em-andamento.md)** — onde o trabalho parou.

> `docs/produto.md` existe **idêntico** em `frontend/docs/produto.md`, porque cada repositório precisa se sustentar sozinho. **Alterou aqui, replique lá na mesma tarefa.**

## 6. Se você vai usar IA

Codex, Cursor e Copilot carregam o `AGENTS.md` automaticamente **quando ele está na pasta que você abriu**. Como você vai abrir a pasta de cima, que não tem `AGENTS.md`, **nada é carregado sozinho**. A skill abaixo resolve isso.

### Instale a skill de contexto — uma vez só

O Codex procura skills em `.agents/skills/` **da pasta que ele abriu** — no nosso caso, a pasta que contém `backend/` e `frontend/`. O arquivo da skill mora versionado dentro dos repositórios; o comando abaixo só o materializa na raiz.

Rode **de dentro da pasta raiz** (`C:\Workspace\Sistema Contabilidade Tributaria`), no cmd:

```bash
xcopy /E /I /Y backend\.agents\skills\praxis-contexto .agents\skills\praxis-contexto
```

Ou no Git Bash:

```bash
mkdir -p .agents/skills && cp -r backend/.agents/skills/praxis-contexto .agents/skills/
```

> A cópia na raiz é **gerada**, não versionada — some se você refizer a pasta e não vem no `git clone`. Precisou mudar a skill? Mude em `backend/.agents/skills/praxis-contexto/SKILL.md`, replique no frontend, e rode o comando de novo. Editar direto a cópia da raiz é trabalho que se perde.

### Use no início de toda sessão

```
/praxis-contexto
```

A skill **não contém as regras** — ela manda ler os `AGENTS.md` dos repositórios, que são a fonte da verdade. É por isso que ela nunca desatualiza: quando uma regra muda no repositório, a skill continua correta. Nunca copie regra para dentro dela.

Rode de novo ao **trocar de repositório** no meio da sessão: as regras do front e do back não são iguais.

> O arquivo da skill existe **idêntico** em `frontend/.agents/skills/praxis-contexto/`, porque cada repositório precisa se sustentar sozinho. Alterou aqui, replique lá na mesma tarefa.

Se a skill não estiver disponível por algum motivo, o equivalente colado à mão é:

```
Antes de tocar em qualquer arquivo, leia inteiros backend/AGENTS.md e
backend/docs/em-andamento.md. Confirme listando a regra de teste, o que exige
revisão explícita minha, os portões de commit e push, e a regra de fronteira
entre módulos. Não faça commit nem push sem eu pedir.
```

**Não rode em modo totalmente automático** enquanto estiver aprendendo o projeto. As regras dependem de a IA **parar e perguntar** — regra de negócio, modelo de dados, contrato entre módulos, prompt do copiloto. Em modo automático ela não para, e o portão vira decoração.

No Claude Code, o `CLAUDE.md` aponta para o `AGENTS.md`, e as skills em `.claude/skills/` são atalhos cujo conteúdo mora em `docs/` — nada se perde em outra ferramenta.

## 7. As cinco coisas que mais dão problema

1. **Módulo lendo tabela de outro.** A única porta entre módulos é `Application`, trocando DTO. Entidade de domínio não atravessa fronteira.
2. **Dapper com nome de coluna em string crua.** EF Core é o padrão; Dapper é exceção com motivo escrito, e sempre com `nameof`.
3. **Teste apontando para o schema `public`.** Teste de integração roda em schema isolado, derrubado ao final. Ver [docs/teste-integracao.md](docs/teste-integracao.md).
4. **Regra de negócio dentro do endpoint.** `Api` traduz HTTP e volta; regra mora no domínio ou no caso de uso.
5. **Alterar regra de negócio sem pedir revisão.** Pare, descreva, aguarde resposta. Vale para modelo de dados e prompt do copiloto também.

## 8. Antes de pedir commit ou push

- **Commit** → revisão do diff seguindo [docs/revisao-pre-commit.md](docs/revisao-pre-commit.md), com os achados relatados a você.
- **Push** → `dotnet test` verde, suíte inteira.
- Terminou uma etapa? Atualize [docs/em-andamento.md](docs/em-andamento.md).

> A suíte tem 24 testes. Os de integração se pulam sozinhos quando não há banco configurado, então `dotnet test` fica verde mesmo antes de você receber as credenciais.

## 9. Onde pedir ajuda

Decisão que ainda não foi tomada e trava o trabalho: veja se já está em [docs/decisoes.md](docs/decisoes.md) ou em [docs/backlog.md](docs/backlog.md). Se não estiver, pergunte antes de escolher por conta própria — decisão tomada silenciosamente por IA é o tipo de coisa que só aparece três semanas depois.
